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
    [PluginInfo(Name = "Request", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with Response", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class RequestSocketNode : AbstractSocketNode<RequestSocket>
    {
        #region fields & pins
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);

        }

        protected override bool EnableSocket(bool enable, RequestSocket socket, string address)
        {
            if (enable)
            {
                socket.Connect(address);
            }
            else
            {
                socket.Disconnect(address);
            }
            return true;
        }

        protected override RequestSocket NewSocket()
        {
            return Context.CreateRequestSocket();
        }

    }
}
