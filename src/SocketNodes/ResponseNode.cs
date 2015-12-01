using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Response", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Request", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class ResponseNode : AbstractFlexibleSocketNode<ResponseSocket>
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

        protected override ResponseSocket NewSocket()
        {
            return Context.CreateResponseSocket();
        }


        public override bool IsBindDefaultTrue()
        {
            return true;
        }
    }
}
