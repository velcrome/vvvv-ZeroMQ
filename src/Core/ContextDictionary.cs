using NetMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.ZeroMQ.Nodes
{
    public class NetMQContextDictionary
    {
        private static Dictionary<string, NetMQContext> AllContexts;
        
        public static NetMQContext GetContext(string id = "Default") {
            
            if (AllContexts == null) {
                AllContexts = new Dictionary<string, NetMQContext>();
            }
            
            if (!AllContexts.ContainsKey(id)) {
                AllContexts.Add(id, NetMQContext.Create());
            }

            return AllContexts[id];

        }

    }
}
