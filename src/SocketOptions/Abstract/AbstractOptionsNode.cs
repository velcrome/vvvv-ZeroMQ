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
    [PluginInfo(Name = "Watermark", Category = "Options ZSocket", Help = "Limits a sockets internal message buffer", Tags = "ZeroMQ", Author = "velcrome")]
    #endregion PluginInfo
    public abstract class AbstractOptionsNode : IPluginEvaluate
    {
        protected const string AUTHOR = "velcrome";
        protected const string OPTION_CATEGORY = "Option ZSocket";
        protected const string TAGS = "ZeroMQ";

        #region fields & pins
        [Input("SocketOptions", Order=-1)]
        public IDiffSpread<Options> FInput;

        [Output("SocketOptions", AutoFlush = false)]
        public ISpread<Options> FOutput;

        [Import()]
        public ILogger FLogger;

        #endregion fields & pins

        public abstract void Evaluate(int SpreadMax);

    }
}
