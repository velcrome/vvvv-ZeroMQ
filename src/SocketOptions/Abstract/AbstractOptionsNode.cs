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

    public abstract class AbstractOptionsNode : IPluginEvaluate
    {
        protected const string AUTHOR = "velcrome";
        protected const string OPTION_CATEGORY = "Network.ZeroMQ.Option";
        protected const string TAGS = "";

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
