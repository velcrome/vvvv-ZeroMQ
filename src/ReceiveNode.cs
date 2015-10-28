using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Receive", AutoEvaluate = true, Category = "0qm", Help = "Receives from a socket", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class ReceiveNode : IPluginEvaluate
    {
        #region fields & pins

        [Input("Socket")]
        public ISpread<IReceivingSocket> FSocket;

        [Output("Data")]
        public ISpread<ISpread<Stream>> FOutput;

        [Output("OnData", IsBang=true)]
        public ISpread<bool> FOnData;

        [Import()]
        public ILogger FLogger;

        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            if (FSocket.SliceCount == 0 || (FSocket.SliceCount == 1 && FSocket[0] == null))
            {
                FOutput.SliceCount = 1;
                FOutput[0].SliceCount = 0;
                FOnData.SliceCount = 1;
                FOnData[0] = false;
                return;
            }

            FOutput.SliceCount =
            FOnData.SliceCount = SpreadMax;

            //            var timeout = new TimeSpan(0, 0, 0, 1); // 1 sec

            for (int i = 0; i < SpreadMax; i++)
            {
                var socket = (NetMQSocket) FSocket[i];
                if (socket == null)
                {
                    FOutput[i].SliceCount = 0;
                    FOnData[i] = false;
                    continue;
                }

                FOutput[i].SliceCount = 0;
                FOnData[i] = socket.HasIn;

                while (socket.HasIn)
                {
                    var more = true;
                    while (more)
                    {
                        var buffer = socket.ReceiveFrameBytes(out more);

                        var stream = new MemoryStream();
                        stream.Write(buffer, 0, buffer.Length);

                        FOutput[i].Add(stream);
                    }
                }
            }
        }
    }
}
