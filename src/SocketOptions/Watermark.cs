using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes
{
    using Options = VVVV.ZeroMQ.Nodes.Core.Options;

    #region PluginInfo
    [PluginInfo(Name = "Watermark", Category = OPTION_CATEGORY, Help = "Limits a sockets internal message buffer", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class WatermarkOptionsNode : AbstractOptionsNode
    {
        #region fields & pins
        [Input("Send High Watermark", DefaultValue = 1000)]
        public IDiffSpread<int> SendHighWatermark;

        [Input("Receive High Watermark", DefaultValue = 1000)]
        public IDiffSpread<int> ReceiveHighWatermark;
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !SendHighWatermark.IsChanged && !ReceiveHighWatermark.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = new Options(FInput[i]);

                config.SendHighWatermark = SendHighWatermark[i];
                config.ReceiveHighWatermark = ReceiveHighWatermark[i];

                FOutput[i] = config;
            }

            FOutput.Flush();
        }
    }
}
