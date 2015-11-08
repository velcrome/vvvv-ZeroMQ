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
    [PluginInfo(Name = "XSubscriber", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with Publisher", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class XSubscriberSocketNode : AbstractFlexibleSocketNode<XSubscriberSocket>
    {
        #region fields & pins
        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
        }

        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);
        }

        protected override XSubscriberSocket NewSocket()
        {
            return Context.CreateXSubscriberSocket();
        }


        public override bool IsBindDefaultTrue()
        {
            return true;
        }
    }
}
