using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.ZeroMQ.Nodes;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Subscriber", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with Publisher", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class SubscriberSocketNode : AbstractSocketNode<SubscriberSocket>
    {
        #region fields & pins
        [Input("Topic", DefaultString = "*")]
        public IDiffSpread<bool> FTopic;

        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);

        }

        protected override bool EnableSocket(bool enable, SubscriberSocket socket, string address)
        {
            if (enable)
            {
                socket.Connect(address);
                socket.Subscribe("foo");
            }
            else
            {
                socket.Unsubscribe("foo");
                socket.Disconnect(address);
            }
            return true;
        }

        protected override SubscriberSocket NewSocket()
        {
            return Context.CreateSubscriberSocket();
        }

    }
}
