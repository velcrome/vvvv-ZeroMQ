using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes
{
    using Options = VVVV.ZeroMQ.Nodes.Core.Options;

    #region PluginInfo
    [PluginInfo(Name = "KeepAlive", Category = OPTION_CATEGORY, Help = "Finetune how long TCP will survive", Tags = TAGS+", tcp", Author = AUTHOR)]
    #endregion PluginInfo
    public class KeepAliveOptionsNode : AbstractOptionsNode
    {
        #region fields & pins
        [Input("Period")]
        public IDiffSpread<TimeSpan> FTcpKeepAliveInterval;

        [Input("Maximum Retry Time")]
        public IDiffSpread<TimeSpan> FTcpKeepAliveIdle;

        [Input("Enable", DefaultBoolean = false, IsToggle = true)]
        public IDiffSpread<bool> FTcpKeepAlive;
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FTcpKeepAlive.IsChanged && !FTcpKeepAliveIdle.IsChanged && !FTcpKeepAliveInterval.IsChanged) return;

            FOutput.SliceCount = SpreadMax;
   //         var Default = Options.Default();

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = new Options(FInput[i]);

                config.TcpKeepalive = FTcpKeepAlive[i];
                config.TcpKeepaliveIdle = FTcpKeepAliveIdle[i];
                config.TcpKeepaliveInterval = FTcpKeepAliveInterval[i];

                FOutput[i] = config;
            }

            FOutput.Flush();
        }
    }
}
