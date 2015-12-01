using NetMQ;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes
{
    using Options = VVVV.ZeroMQ.Nodes.Core.Options;

    #region PluginInfo
    [PluginInfo(Name = "Endian", Category = OPTION_CATEGORY, Help = "Change the Endian, Default is BigEndian", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class EndianOptionsNode : AbstractOptionsNode
    {
        #region fields & pins
        [Input("Little Endian", IsToggle = true, DefaultBoolean=false)]
        public IDiffSpread<bool> FEndian;
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FEndian.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = new Options(FInput[i]);
                config.Endian = FEndian[i] ? Endianness.Little : Endianness.Big;
                FOutput[i] = config;
            }

            FOutput.Flush();
        }
    }
}
