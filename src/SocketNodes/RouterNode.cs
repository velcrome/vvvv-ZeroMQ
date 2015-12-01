using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Router", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Dealer", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class RouterSocketNode : AbstractFlexibleSocketNode<RouterSocket>
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

        protected override RouterSocket NewSocket()
        {
            return Context.CreateRouterSocket();
        }


        public override bool IsBindDefaultTrue()
        {
           return true;
        }
    }
}
