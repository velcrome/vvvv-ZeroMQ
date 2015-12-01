using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Request", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Response", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class RequestNode : AbstractFlexibleSocketNode<RequestSocket>
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

        protected override RequestSocket NewSocket()
        {
            return Context.CreateRequestSocket();
        }


        public override bool IsBindDefaultTrue()
        {
            return false;
        }
    }
}
