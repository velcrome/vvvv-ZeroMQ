using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "XSubscriber", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Publisher", Tags = TAGS+", Broker", Author = AUTHOR)]
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
