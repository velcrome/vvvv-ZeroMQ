using NetMQ;
using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "XPublisher", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Subscribe", Tags = TAGS+", Broker", Author = AUTHOR)]
    #endregion PluginInfo
    public class XPublisherSocketNode : AbstractSocketNode<XPublisherSocket>
    {
        #region fields & pins
        [Input("Bind", Visibility = PinVisibility.True, Order=int.MaxValue-1, DefaultBoolean = true, IsSingle = true)]
        public IDiffSpread<bool> ConfigBind;

        #endregion fields & pins
        protected override NetMQSocket NewSocket(string address)
        {
            return new XPublisherSocket(address);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            ConfigBind.Changed += _ => Bind = ConfigBind[0];
        }


    }
}
