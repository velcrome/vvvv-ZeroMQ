using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
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

        [Input("Start", IsBang=true)]
        public ISpread<bool> FEnable;

        [Output("Data")]
        public ISpread<ISpread<Stream>> FOutput;

        [Output("OnData", IsBang = true)]
        public ISpread<bool> FOnData;

        [Import()]
        public ILogger FLogger;

        protected Poller Poll = new Poller();

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
                FOutput.SliceCount = 1;
                FOutput[0].SliceCount = 0;
                FOnData.SliceCount = 1;
                FOnData[0] = false;
                return;
            }


            // avoid spreading. proxy must be 1:1
            SpreadMax = Math.Min(FFrontendSocket.SliceCount, FBackendSocket.SliceCount);

            FOutput.SliceCount =
            FOnData.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var backend = (NetMQSocket)FBackendSocket[i];
                var frontend = (NetMQSocket)FFrontendSocket[i];

                //if (frontend == null || backend == null)
                //{
                //    FOutput[i].SliceCount = 0;
                //    FOnData[i] = false;
                //    continue;
                //}

                FOutput[i].SliceCount = 0;

                if (FEnable[0])
                {
                    Poll.AddSocket(frontend);
                    Poll.AddSocket(backend);

                    var proxy = new Proxy(frontend, backend, null, Poll);
                    proxy.Start();

                    //Poll.PollOnce();

                }




                //FOnData[i] = frontend.HasIn;

                //while (frontend.HasIn)
                //{
                //    var more = true;
                //    while (more)
                //    {
                //        var buffer = frontend.ReceiveFrameBytes(out more);

                //        var stream = new MemoryStream();
                //        stream.Write(buffer, 0, buffer.Length);

                //        FOutput[i].Add(stream);

                //        backend.SendFrame(buffer, more);
                //    }
                //}
            }
        }
    }
}
