using NetMQ;
using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Pair", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with another Pair", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class PairSocketNode : AbstractSocketNode<PairSocket>
    {
        #region fields & pins

        [Input("Bind", Visibility = PinVisibility.True, Order=int.MaxValue-1, DefaultBoolean = false, IsSingle = true)]
        public IDiffSpread<bool> ConfigBind;

        #endregion fields & pins
        protected override NetMQSocket NewSocket(string address)
        {
            return new PairSocket(address);
        }

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();
            ConfigBind.Changed += _ => Bind = ConfigBind[0];
        }


    }
}
