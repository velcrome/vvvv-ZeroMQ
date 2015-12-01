using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ
{
    #region PluginInfo
    [PluginInfo(Name = "Receive", AutoEvaluate = true, Category = "Network.ZeroMQ", Version ="Raw", Help = "Receives from a socket", Tags = "ZeroMQ", Author = "velcrome")]
    #endregion PluginInfo
    public class ReceiveNode : IPluginEvaluate
    {
        #region fields & pins

        [Input("Socket")]
        public ISpread<IReceivingSocket> FSocket;

        [Output("Frames", AutoFlush=false)]
        public ISpread<Stream> FOutput;

        [Output("Frames Bin Size")]
        public ISpread<int> FMessageBin;

        [Output("Message Count")]
        public ISpread<int> FSocketBin;

        [Output("OnData", IsBang = true, IsSingle = true)]
        public ISpread<bool> FOnData;

        [Import()]
        public ILogger FLogger;

        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            if (FSocket.SliceCount == 0 || (FSocket.SliceCount == 1 && FSocket[0] == null))
            {
                FOutput.SliceCount = 0;
                FOutput.Flush();

                FSocketBin.SliceCount = FMessageBin.SliceCount = 1;
                FSocketBin[0] = FMessageBin[0] = 0;
                
                FOnData[0] = false;
                return;
            }

            FSocketBin.SliceCount = SpreadMax;
            FOutput.SliceCount =
            FMessageBin.SliceCount = 0;

            for (int socketid = 0; socketid < SpreadMax; socketid++)
            {
                var socket = (NetMQSocket) FSocket[socketid];
                if (socket == null)
                {
                    FOnData[socketid] = false;
                    FSocketBin[socketid] = 0;
                    continue;
                }

                bool more = socket.HasIn;
                var sBin = 0;
                FOnData[socketid] = more;

                while (more)
                {
                    var mBin = 0;
                    
                    while (more)
                    {
                        var buffer = socket.ReceiveFrameBytes(out more);

                        var stream = new MemoryStream();
                        stream.Write(buffer, 0, buffer.Length);

                        FOutput.Add(stream);
                        mBin++;
                    }

                    FMessageBin.Add(mBin);
                    sBin++;                    
                    more = socket.HasIn;
                }

                FSocketBin[socketid] = sBin;
            }

            if (FMessageBin.SliceCount == 0)
            {
                FMessageBin.SliceCount = 1;
                FMessageBin[0] = 0;

                FOnData[0] = false;
            }
            else FOnData[0] = true;

            FOutput.Flush();
        }
    }
}
