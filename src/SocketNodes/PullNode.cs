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
    [PluginInfo(Name = "Pull", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Pull", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class PullSocketNode : AbstractFlexibleSocketNode<PullSocket>
    {
        #region fields & pins
        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            //Bind = false;
            base.OnImportsSatisfied();
        }

        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);

        }

        protected override PullSocket NewSocket()
        {
            return Context.CreatePullSocket();
        }

        public override bool IsBindDefaultTrue()
        {
            return false;
        }

    }
}
