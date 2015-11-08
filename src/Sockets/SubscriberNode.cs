using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.ZeroMQ.Nodes;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    #region PluginInfo
    [PluginInfo(Name = "Subscriber", Category = "0qm Socket", Help = "Creates a socket, use in conjunction with Publisher", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class SubscriberSocketNode : AbstractSocketNode<SubscriberSocket>
    {
        #region fields & pins
        [Input("Topic", DefaultString = "Event")]
        public IDiffSpread<string> FTopic;

        protected List<string> Topic = new List<string>();

        #endregion fields & pins

        public override void OnImportsSatisfied()
        {
            base.OnImportsSatisfied();

            FTopic.Changed += FTopicChanged;
         }

        private void FTopicChanged(IDiffSpread<string> spread)
        {
            foreach (var address in WorkingSockets.ToArray())
                Subscribe(false, Sockets[address], Topic);

            Topic.Clear();
            Topic = FTopic.ToList();

            foreach (var address in WorkingSockets.ToArray())
                Subscribe(true, Sockets[address], Topic);
             
        }

        public override void Evaluate(int SpreadMax)
        {
            base.Evaluate(SpreadMax);
        }

        protected override bool EnableSocket(bool enable, SubscriberSocket socket, string address)
        {
            if (!enable)
            {
                Subscribe(false, socket, Topic);
            }

            var success = base.EnableSocket(enable, socket, address);

            Topic.AssignFrom(FTopic);
            if (success && enable)
            {
                Subscribe(true, socket, FTopic);
            }
            return true;
        }

        protected override SubscriberSocket NewSocket()
        {
            return Context.CreateSubscriberSocket();
        }

    }
}
