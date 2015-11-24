using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Packs.Messaging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VVVV.ZeroMQ
{
    #region PluginInfo
    [PluginInfo(Name = "Send", AutoEvaluate = true, Category = "Message ZSocket", Help = "Sends to a socket", Tags = "ZeroMQ", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSendNode : IPluginEvaluate
    {
        #region fields & pins

        [Input("Socket")]
        public ISpread<IOutgoingSocket> FSocket;

        [Input("Message")]
        public ISpread<ISpread<Message>> FInput;

        [Input("Send", IsBang = true, DefaultBoolean = true)]
        public ISpread<bool> FSend;

        [Output("Success", IsBang = true)]
        public ISpread<bool> FSuccess;

        [Import()]
        public ILogger FLogger;

        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            if (FSocket.SliceCount == 0 || (FSocket.SliceCount == 1 && FSocket[0] == null))
            {
                FSuccess.SliceCount = 1;
                FSuccess[0] = false;
                return;
            }

            SpreadMax = FSocket.CombineWith(FInput); // FSent is cut off at the amount of FSockets
            FSuccess.SliceCount = SpreadMax;

            for (int socketid = 0; socketid < SpreadMax; socketid++)
            {
                var socket = FSocket[socketid];
                if (socket == null || FSend[socketid] == false) // socket has not been enabled, nor is any sending necessary
                {
                    FSuccess[socketid] = false;
                    continue;
                }

                FSuccess[socketid] = true; // if just one frame fails, it will turn to false

                var dataBin = FInput[socketid];
                var binCount = dataBin.SliceCount;
                for (int bin = 0; bin < binCount; bin++)
                {
                    var message = dataBin[bin];
                    var msg = new NetMQMessage();
                    msg.Append(message.Topic, Encoding.UTF8);

                    JsonSerializer ser = new JsonSerializer();
                    JsonSerializerSettings settings = new JsonSerializerSettings();
                    settings.Formatting = Formatting.None;
                    settings.TypeNameHandling = TypeNameHandling.None;


                    string json = JsonConvert.SerializeObject(message, settings);
                    msg.Append(json, Encoding.UTF8);

                    try
                    {
                        FSuccess[socketid] &= socket.TrySendMultipartMessage(msg);
                    }
                    catch (FiniteStateMachineException)
                    {
                        // likely cause: your send is blocked because nothing received yet.
                        // http://api.zeromq.org/2-1:zmq-send see under Error "EFSM"

                        FLogger.Log(LogType.Warning, "vvvv.ZeroMQ: Not allowed to send yet. Need a response first.\n");
                    }
                    catch (NotSupportedException e)
                    {
                        FSuccess[socketid] = false;
                        FLogger.Log(LogType.Error, "\nvvvv-ZeroMQ: " + socket.GetType() + " does not supoort Send.");
                        throw e;
                    }

                    catch (Exception e)
                    {
                        FSuccess[socketid] = false;
                        FLogger.Log(LogType.Error, "\nvvvv-ZeroMQ: Problem in Send. \n" + e);
                        throw e; // some unknown internal error. escalate to node level
                    }
                }
            }
        }

    }
}
