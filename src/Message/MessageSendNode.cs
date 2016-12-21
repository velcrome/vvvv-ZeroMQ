using NetMQ;
using System;
using System.ComponentModel.Composition;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.Packs.Messaging;
using MsgPack.Serialization;
using VVVV.Packs.Messaging.Serializing;

namespace VVVV.ZeroMQ
{
    #region PluginInfo
    [PluginInfo(Name = "Send", AutoEvaluate = true, Category = "Network.ZeroMQ", Version="Message", Help = "Sends to a socket", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class MessageSendNode : IPluginEvaluate, IPartImportsSatisfiedNotification
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

        protected MessagePackSerializer<Message> Serializer;

        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            var context = new SerializationContext();
            context.CompatibilityOptions.PackerCompatibilityOptions = MsgPack.PackerCompatibilityOptions.PackBinaryAsRaw;
            context.Serializers.RegisterOverride(new MsgPackMessageSerializer(context));
            Serializer = MessagePackSerializer.Get<Message>(context);
        }

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


                var dataBin = FInput[socketid];
                var binCount = dataBin.SliceCount;

                if (binCount == 0 || dataBin[0] == null) continue;

                FSuccess[socketid] = true; // if just one frame fails, it will turn to false

                for (int bin = 0; bin < binCount; bin++)
                {
                    var message = dataBin[bin];

                    if (message == null)
                    {
                        FLogger.Log(LogType.Warning, "vvvv.ZeroMQ: Null Message detected, dropped.\n");
                        continue; // drop message silently
                    }

                    var msg = new NetMQMessage();
                    msg.Append(message.Topic, Encoding.UTF8);

                    var raw = Serializer.PackSingleObject(message);
                    msg.Append(raw);

                    try
                    {
                        FSuccess[socketid] &= socket.TrySendMultipartMessage(msg);
                    }
                    catch (FiniteStateMachineException)
                    {
                        // likely cause: your send is blocked because nothing received yet.
                        // http://api.zeromq.org/2-1:zmq-send see under Error "EFSM"

                        FSuccess[socketid] = false;
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
