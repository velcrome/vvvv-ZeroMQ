using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    public abstract class AbstractFlexibleSocketNode<T> : AbstractSocketNode<T> where T : NetMQSocket
    {
        #region fields & pins

//      [Config("Bind", DefaultBoolean = IsBindDefaultTrue(), IsSingle = true)]
        public IIOContainer ConfigBind;

        protected IDiffSpread<bool> FBind
        {
            get
            {
                return ((IDiffSpread<bool>)(ConfigBind.RawIOObject));
            }
        } 

        [Import()]
        protected IIOFactory FIOFactory;

        #endregion fields & pins

        // must be overridden in inheriting nodes to flag if Bind (true) or Connect (false) is default node behaviour
        public abstract bool IsBindDefaultTrue();        
        
        public override void OnImportsSatisfied()
        {
            var attr = new ConfigAttribute("Bind");
            attr.DefaultBoolean = IsBindDefaultTrue();
            attr.IsToggle = true;
            attr.IsSingle = true;
            
            Type pinType = typeof(IDiffSpread<>).MakeGenericType(typeof(bool));
            ConfigBind = FIOFactory.CreateIOContainer(pinType, attr);

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
