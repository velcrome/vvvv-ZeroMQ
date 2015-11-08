using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.ZeroMQ.Nodes;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Pair", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with another Pair", Tags = "", Author = "velcrome")]
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
