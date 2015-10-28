#region usings
using System;
using System.ComponentModel.Composition;
using System.IO;
using VVVV.PluginInterfaces.V2;



using VVVV.Core.Logging;
using NetMQ;
using NetMQ.Sockets;
#endregion usings

namespace VVVV.ZeroMQ.Nodes
{

    #region PluginInfo
    [PluginInfo(Name = "Request", Category = "0mq", AutoEvaluate = true, Help = "Basic template with one value in/out", Tags = "")]
    #endregion PluginInfo
    public class RequestNodeOld : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<Stream> FInput;

        [Input("Protocol", DefaultEnumEntry = "tcp")]
        public ISpread<TransportProtocolEnum> FProtocol;

        [Input("Server", DefaultString = "localhost")]
        public ISpread<string> FServer;

        [Input("Port", DefaultValue = 4444)]
        public ISpread<int> FPort;

        [Input("Enable", DefaultBoolean = false, IsSingle = true)]
        public IDiffSpread<bool> FEnable;


        
        [Output("Output")]
        public ISpread<Stream> FOutput;

        [Output("Blocked")]
        public ISpread<bool> FBlocked;

        [Output("OnData")]
        public ISpread<bool> FOnData;

        [Import()]
        public ILogger FLogger;

        protected RequestSocket client;
        protected string Address;


        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            var context = NetMQContext.Create();
            client = context.CreateRequestSocket();

            // unless FEnable is last pin in node, other pins will not hold valid data
            FEnable.Changed += Connect;
        }

        private void Connect(IDiffSpread<bool> spread)
        {
            if (FEnable.SliceCount == 0 || !FEnable[0])
            {
                // disconnect
                try
                {
                    client.Disconnect(Address);
                }
                catch (Exception e)
                {
                    FLogger.Log(e, LogType.Error);
                }

            }
            else
            {
                // reconnect
                Address = FProtocol[0].ToString() + "://" + FServer[0] + ":" + FPort[0];
                client.Connect(Address);
            }
        }


        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (!FEnable[0]) return;

            if (FInput.SliceCount > 0)
            {
                var length = (int)FInput[0].Length;
                var buffer = new byte[length];

                var stream = FInput[0].Read(buffer, 0, length);

                try
                {
                    FBlocked[0] = client.TrySendFrame(buffer); 
                }
                catch (Exception e)
                {
                    FLogger.Log(e, LogType.Error);
                }
            }

            if (client.HasIn)
            {
                FOutput.ResizeAndDispose(1, () => new MemoryStream());
                FOutput[0].SetLength(0);
                var buffer = client.ReceiveFrameBytes();
                FOutput[0].Write(buffer, 0, buffer.Length);
                FOnData[0] = true;
                FBlocked[0] = false;
            }
            else
            {
                FOutput.SliceCount = 0;
                FOnData[0] = false;
            }

            //FLogger.Log(LogType.Debug, "hi tty!");
        }

        public void Dispose()
        {
            client.Close();
        }
    }
}
