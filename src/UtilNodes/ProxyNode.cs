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
    [PluginInfo(Name = "Proxy", AutoEvaluate = true, Category = "Network ZSocket", Help = "Proxy for XPublisher and XSubscriber", Tags = "ZeroMQ", Author = "velcrome")]
    #endregion PluginInfo
    public class ProxyNode : IPluginEvaluate, IDisposable
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
                    try
                    {
                        p.Stop();
                    }
                    catch (InvalidOperationException e)
                    {
                        FLogger.Log(LogType.Warning, "\nvvvv.ZeroMQ: Proxy cannot be stopped. " + e.Message);
                    }
                }
                
            }

            // spreading could cause weird behaviour, when multiple proxies feed on the same sockets
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
                    
                    // FRONTEND
                    try
                    {
                        Poll.AddSocket(frontend);
                    }
                    catch (ArgumentException)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Socket already added to Poller in Proxy. Nothing to worry about.");
                    }
                    catch (ObjectDisposedException)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Cannot add Socket to Proxy, already disposed.");
                    }

                    // BACKEND                    
                    try
                    {
                        Poll.AddSocket(backend);
                    }
                    catch (ArgumentException)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Socket already added to Poller in Proxy. Nothing to worry about.");
                    }
                    catch (ObjectDisposedException)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Cannot add Socket to Proxy, already disposed.");
                    }


                    // PROXY
                    try
                    {
                        var p = new Proxy(frontend, backend, null, Poll);
                        p.Start(); // nonblocking from now on

                        Proxies.Add(p);
                    }
                    catch (InvalidOperationException e)
                    {
                        FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: start Proxy failed.\n"+e);
                    }
                }
            }
        }

        public void Dispose()
        {
            if (Poll.IsStarted) Poll.CancelAndJoin();
            Poll.Dispose();
        }
    }
}
