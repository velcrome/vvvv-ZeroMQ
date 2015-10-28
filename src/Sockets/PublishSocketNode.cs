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
    [PluginInfo(Name = "Publish", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with Subscribe", Tags = "", Author="velcrome")]
    #endregion PluginInfo
    public class PublishSocketNode : AbstractSocketNode<PublisherSocket>
    {
       #region fields & pins
        [Config("Bind", DefaultBoolean=false, IsSingle=true)]
        public IDiffSpread<bool> FBind;

        #endregion fields & pins
        
        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);

        }

        protected override bool EnableSocket(bool enable, PublisherSocket socket, string address)
        {
            if (enable)
            {
                socket.Bind(address);
            }
            else
            {
                socket.Unbind(address);
            }

            return true;
        }

        protected override PublisherSocket NewSocket()
        {
            return Context.CreatePublisherSocket();
        }

    }
}
