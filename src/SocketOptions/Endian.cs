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
 //   [PluginInfo(Name = "Endian", Category = OPTION_CATEGORY, Help = "Configures the maximum time a Message will be kept AFTER the ", Tags = TAGS, Author = AUTHOR)]
    #endregion PluginInfo
    public class EndianOptionsNode : AbstractOptionsNode
    {
        #region fields & pins
        [Input("Endian")]
        public IDiffSpread<Endianness> FEndian;
        #endregion fields & pins

        public override void Evaluate(int SpreadMax)
        {
            if (!FInput.IsChanged && !FEndian.IsChanged) return;

            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var config = new Options(FInput[i]);

                var a = config.Endian = FEndian[i];
               


                FOutput[i] = config;
            }

            FOutput.Flush();
        }
    }
}
