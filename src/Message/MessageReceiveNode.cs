using NetMQ;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Utils;
using VVVV.Packs.Messaging;
using VVVV.PluginInterfaces.V2;


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
                        var msg = socket.ReceiveMultipartMessage(2);
                        var s = System.Text.Encoding.UTF8.GetString(msg.Last.ToByteArray());

                        var result = JsonConvert.DeserializeObject(s);

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
        }
    }
}
