using NetMQ.Sockets;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Pair", Category = SOCKET_CATEGORY, Help = "Creates a socket, use in conjunction with another Pair", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class PairSocketNode : AbstractFlexibleSocketNode<PairSocket>
    {
        #region fields & pins
        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            Bind = false;
            base.OnImportsSatisfied();
        }

        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);

        }

        protected override PairSocket NewSocket()
        {
            return Context.CreatePairSocket();
        }

        public override bool IsBindDefaultTrue()
        {
            return false;
        }

    }
}
