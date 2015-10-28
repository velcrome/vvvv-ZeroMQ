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
	[PluginInfo(Name = "Response", Category = "0mq", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class ResponseNodeOld : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
        [Input("Protocol", DefaultEnumEntry = "tcp")]
        public IDiffSpread<TransportProtocolEnum> FProtocol;

		[Input("Server", DefaultString = "localhost")]
		public IDiffSpread<string> FServer;
		
		[Input("Port", DefaultValue = 4444)]
		public IDiffSpread<int> FPort;

        [Input("Response")]
        public ISpread<Stream> FResponse; 

        [Input("Enable", DefaultBoolean = false, IsSingle = true)]
        public IDiffSpread<bool> FEnable;

        
        
        [Output("Output")]
		public ISpread<ISpread<Stream>> FOutput;

        [Output("OnData")]
        public ISpread<bool> FOnData;

		[Import()]
		public ILogger FLogger;

        protected ResponseSocket server;
        protected string Address;
        

		#endregion fields & pins

		public void OnImportsSatisfied() {
            var context = NetMQContext.Create();
            server = context.CreateResponseSocket();

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
                   server.Unbind(Address);
                }
                catch (Exception e) {
                    FLogger.Log(e, LogType.Error);
                }
            }
            else
            {
                // reconnect
                Address = FProtocol[0].ToString() + "://" + FServer[0] + ":" + FPort[0];
                server.Bind(Address);

            }
        }
		
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

            if (!server.HasIn)
            {
                FOutput.SliceCount = 0;
                FOnData[0] = false;
                return;
            }

//          set notification pin
            FOnData[0] = true;


            var bin = 0;            
            while (server.HasIn)
            {

                FOutput.SliceCount = bin + 1;
                
                var more = true;
                while (more)
                {
                    var buffer = server.ReceiveFrameBytes(out more);
                    FOutput[bin].SliceCount = 0;

                    var stream = new MemoryStream();
                    stream.Write(buffer, 0, buffer.Length);
                    
                    FOutput[bin].Add(stream);
                }
            

                bin++;

                // response necessary, or the clients cannot send more
                
                var length = (int) FResponse[0].Length;
                var response = new byte[length];
                FResponse[0].Read(response, 0, length);
                server.SendFrame(response);
            }
          

			//FLogger.Log(LogType.Debug, "hi tty!");
		}

        public void Dispose()
        {
            if (FEnable[0])
            {
                server.Unbind(Address);
            }
            server.Close();
        }
	}
}
