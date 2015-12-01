using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "XPublisher", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Subscribe", Tags = TAGS+", Broker", Author = AUTHOR)]
    #endregion PluginInfo
    public class XPublisherSocketNode : AbstractFlexibleSocketNode<XPublisherSocket>
    {
        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);
        }

        protected override XPublisherSocket NewSocket()
        {
            return Context.CreateXPublisherSocket();
        }

        public override bool IsBindDefaultTrue()
        {
            return true;
        }

    }
}
