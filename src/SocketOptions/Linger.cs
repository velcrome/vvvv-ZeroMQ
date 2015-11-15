using NetMQ;
using System;
using System.ComponentModel.Composition;
using System.Dynamic;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.ZeroMQ.Nodes.Core;

namespace VVVV.ZeroMQ.Nodes
{
    using Options = VVVV.ZeroMQ.Nodes.Core.Options;

    #region PluginInfo
    [PluginInfo(Name = "Linger", Category = OPTION_CATEGORY, Help = "Configures the maximum time a Message will be kept AFTER the ", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class LingerOptionsNode : AbstractOptionsNode
    {
        #region fields & pins
        [Input("Linger")]
        public IDiffSpread<TimeSpan> FLinger;
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FLinger.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = new Options(FInput[i]);

                config.Linger = FLinger[i];

                FOutput[i] = config;
            }

            FOutput.Flush();
        }
    }
}
