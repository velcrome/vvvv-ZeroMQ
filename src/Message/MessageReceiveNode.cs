using NetMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Utils;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;
using System.IO;

namespace VVVV.ZeroMQ
{
    #region PluginInfo
    [PluginInfo(Name = "Receive", AutoEvaluate = true, Category = "Network.ZeroMQ", Version="Message", Help = "Receives from a socket", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageReceiveNode : IPluginEvaluate
    {
        #region fields & pins

        [Input("Socket")]
        public ISpread<IReceivingSocket> FSocket;

        [Output("Message", AutoFlush = false)]
        public ISpread<Message> FOutput;

        [Output("Meta", AutoFlush = false)]
        public Pin<Stream> FMeta;

        [Output("Meta Bin Size", AutoFlush = false)]
        public Pin<int> FMetaBinSize;

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

                FSocketBin.SliceCount =  1;
                FSocketBin[0] = 0;

                FOnData[0] = false;
                return;
            }

            FSocketBin.SliceCount = SpreadMax;
            FOutput.SliceCount = 0;


            FMeta.SliceCount = 0;
            FMetaBinSize.SliceCount = 0;
            var doMeta = FMeta.IsConnected || FMetaBinSize.IsConnected;

            for (int socketid = 0; socketid < SpreadMax; socketid++)
            {
                var socket = (NetMQSocket)FSocket[socketid];
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
                    try
                    {
                        var msg = socket.ReceiveMultipartMessage(2); // expect 2, but read all, nonetheless

                        if (doMeta)
                        {
                            var mBin = 0;
                            for (int i = 0; i < msg.FrameCount - 2; i++)
                            {
                                var stream = new MemoryStream();
                                var f = msg[i];

                                stream.Write(f.Buffer, 0, f.BufferSize);

                                FMeta.Add(stream);
                                mBin++;
                            }
                            FMetaBinSize.Add(mBin);
                        }

                        var payload = System.Text.Encoding.UTF8.GetString(msg.Last.ToByteArray());
                        var result = JsonConvert.DeserializeObject(payload);

                        if (result is JObject)
                        {
                            FOutput.Add((result as JObject).ToObject<Message>());
                            sBin++;
                        }
                    }
                    catch (Exception e)
                    {
                        FLogger.Log(LogType.Warning, "vvvv-ZeroMQ: Received Data is not a valid Message. "+e);
                    }
                    more = socket.HasIn;
                }

                FSocketBin[socketid] = sBin;

            }

            if (FOutput.SliceCount == 0)
            {
                FOnData[0] = false;
            }
            else FOnData[0] = true;

            FOutput.Flush();
            FMeta.Flush();
            FMetaBinSize.Flush();
        }
    }
}
