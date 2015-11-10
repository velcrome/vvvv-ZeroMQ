using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Publisher", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with Subscribe", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class PublisherSocketNode : AbstractFlexibleSocketNode<PublisherSocket>
    {
        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);
        }

        protected override PublisherSocket NewSocket()
        {
            return Context.CreatePublisherSocket();
        }

        public override bool IsBindDefaultTrue()
        {
            return true;
        }

    }
}
