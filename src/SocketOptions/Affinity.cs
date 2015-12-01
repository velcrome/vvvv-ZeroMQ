using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes
{
    using Options = VVVV.ZeroMQ.Nodes.Core.Options;

    #region PluginInfo
    [PluginInfo(Name = "Affinity", Category = OPTION_CATEGORY, Help = "Configures which core this socket will prefer.", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class AffinityOptionsNode : AbstractOptionsNode
    {
        #region fields & pins
        [Input("Affinity")]
        public IDiffSpread<long> FAffinity;
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FAffinity.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = new Options(FInput[i]);

                config.Affinity = FAffinity[i];

                FOutput[i] = config;
            }

            FOutput.Flush();
        }
    }
}
