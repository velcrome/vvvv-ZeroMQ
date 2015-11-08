using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    public abstract class AbstractFlexibleSocketNode<T> : AbstractSocketNode<T> where T : NetMQSocket
    {
        #region fields & pins
        [Config("Bind", DefaultBoolean = true, IsSingle = true)]
        public IDiffSpread<bool> FBind;

        #endregion fields & pins
        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            
            FBind.Changed += FBindChanged;
        }

        private void FBindChanged(IDiffSpread<bool> spread)
        {

            // all enabled off
            foreach (var address in WorkingSockets.ToArray())
                EnableSocket(false, Sockets[address], address);

            Bind = FBind[0];

            // all on
            foreach (var address in Sockets.Keys)
                EnableSocket(false, Sockets[address], address);

        }
    }
}
