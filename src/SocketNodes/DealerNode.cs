using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Dealer", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Router", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class DealerSocketNode : AbstractFlexibleSocketNode<DealerSocket>
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

        protected override DealerSocket NewSocket()
        {
            return Context.CreateDealerSocket();
        }


        public override bool IsBindDefaultTrue()
        {
            return false;
        }
    }
}
