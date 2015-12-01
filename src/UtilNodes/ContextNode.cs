using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Context", Category = "Network.ZeroMQ", Help = "Define a custom Context to separate inproc", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class ContextNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins
        [Input("Context ID", IsSingle = true, DefaultString="Default")]
        public IDiffSpread<string> FContextID;

        [Output("Context", IsSingle = true, AutoFlush = false)]
        public ISpread<NetMQContext> FContext;

        [Import()]
        public ILogger FLogger;

        public static Dictionary<string, NetMQContext> AllContexts = new Dictionary<string, NetMQContext>();
        
        
        
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FContextID.Changed += Change;
        }

        private void Change(IDiffSpread<string> spread)
        {
            FContext.SliceCount = 1;

            var id = "Default";
            if (FContextID.SliceCount > 0 && FContextID[0].Trim() != "") id = FContextID[0];

            var context = NetMQContextDictionary.GetContext(id);
            FContext[0] = context;
            FContext.Flush();
            
        }

        public void Evaluate(int SpreadMax)
        {
        }

    }
}
