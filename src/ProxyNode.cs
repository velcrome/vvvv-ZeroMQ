using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ
{
    #region PluginInfo
    [PluginInfo(Name = "Proxy", AutoEvaluate = true, Category = "0qm", Help = "Proxy two sockets", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class ProxyNode : IPluginEvaluate
    {
        #region fields & pins

        [Input("Frontend Socket")]
        public ISpread<IReceivingSocket> FFrontendSocket;

        [Input("Backend Socket")]
        public ISpread<IReceivingSocket> FBackendSocket;

        [Input("Enable", IsToggle=true, IsSingle = true)]
        public IDiffSpread<bool> FEnable;

        [Import()]
        public ILogger FLogger;

        protected Poller Poll = new Poller();
        protected List<Proxy> Proxies = new List<Proxy>();

        #endregion fields & pins

        public ProxyNode()
        {
            Poll.PollTillCancelledNonBlocking();
        }

        public void Evaluate(int SpreadMax)
        {
            if (FFrontendSocket.SliceCount == 0 || (FFrontendSocket.SliceCount == 1 && FFrontendSocket[0] == null) ||
                FBackendSocket.SliceCount == 0 || (FBackendSocket.SliceCount == 1 && FBackendSocket[0] == null))
            {
                return;
            }

            if (FEnable.IsChanged && !FEnable[0])
            {
                foreach (var p in Proxies)
                {
                    p.Stop();
                }
                
            }



            // avoid spreading. proxy must be 1:1
            SpreadMax = FFrontendSocket.CombineWith(FBackendSocket);

            for (int i = 0; i < SpreadMax; i++)
            {
                var backend = (NetMQSocket)FBackendSocket[i];
                var frontend = (NetMQSocket)FFrontendSocket[i];

                if (frontend == null || backend == null)
                {
                    continue;
                }


                if (FEnable.IsChanged && FEnable[i])
                {
                    try
                    {
                        Poll.AddSocket(frontend);
                        Poll.AddSocket(backend);
                    }
                    catch (ArgumentException)
                    {
//                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Socket already added to Poller in Proxy. Nothing to worry about.");
                    }
                    catch (ObjectDisposedException)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Cannot add Socket to Proxy, already disposed.");
                    }

                    try
                    {
                        var p = new Proxy(frontend, backend, null, Poll);
                        p.Start();

                        Proxies.Add(p);
                    }
                    catch (InvalidOperationException e)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: start Proxy failed.\n"+e);
                    }
                }
            }
        }
    }
}
