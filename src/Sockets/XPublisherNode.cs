using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "XPublisher", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with Subscribe", Tags = "", Author = "velcrome")]
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

    }
}
