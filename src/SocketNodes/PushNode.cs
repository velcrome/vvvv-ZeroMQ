﻿using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Push", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Pull", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class PushSocketNode : AbstractFlexibleSocketNode<PushSocket>
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

        protected override PushSocket NewSocket()
        {
            return Context.CreatePushSocket();
        }

        public override bool IsBindDefaultTrue()
        {
            return true;
        }

    }
}
