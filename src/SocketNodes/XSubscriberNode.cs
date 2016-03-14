using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "XSubscriber", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Publisher", Tags = TAGS+", Broker", Author = AUTHOR)]
    #endregion PluginInfo
    public class XSubscriberSocketNode : AbstractSocketNode<XSubscriberSocket>
    {
        #region fields & pins

        [Config("Bind", DefaultBoolean = true, IsSingle = true)]
        public IDiffSpread<bool> ConfigBind;

        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            ConfigBind.Changed += _ => Bind = ConfigBind[0];
        }

        #region subscribe
        protected virtual bool Subscribe(bool enable, XSubscriberSocket socket, IEnumerable<string> topic)
        {
            try
            {
                if (enable)
                    foreach (var t in topic) socket.Subscribe(t);
                else foreach (var t in topic) socket.Unsubscribe(t);
                return true;
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + (enable ? "Subscribing" : "Unsubscribing") + " threw an internal exception: " + e);
            }
            return false;
        }
        #endregion subscribe

    }
}
