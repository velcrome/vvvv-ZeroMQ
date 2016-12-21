﻿using NetMQ;
using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Response", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Request", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class ResponseNode : AbstractSocketNode<ResponseSocket>
    {
        #region fields & pins

        [Input("Bind", Visibility = PinVisibility.True, Order=int.MaxValue-1, DefaultBoolean = true, IsSingle = true)]
        public IDiffSpread<bool> ConfigBind;

        #endregion fields & pins
        protected override NetMQSocket NewSocket(string address)
        {
            return new ResponseSocket(address);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            ConfigBind.Changed += _ => Bind = ConfigBind[0];
        }


    }
}
