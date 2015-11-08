using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

namespace VVVV.ZeroMQ.Nodes.Sockets
{
    public abstract class AbstractSocketNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable where T:NetMQSocket
    {
        #region fields & pins
        [Input("Protocol", DefaultEnumEntry = "tcp")]
        public IDiffSpread<TransportProtocolEnum> FProtocol;

        [Input("Server", DefaultString = "localhost")]
        public IDiffSpread<string> FAddress;

        [Input("Port", DefaultValue = 4444, MinValue= 1024, MaxValue=Int16.MaxValue)]
        public IDiffSpread<int> FPort;

        // unless FEnable is last pin in node, other pins will not hold valid data whin it is first changed.
        [Input("Enable", DefaultBoolean = false, Order = int.MaxValue, IsToggle=true)]
        public IDiffSpread<bool> FEnable;

        [Output("Sockets")]
        public ISpread<T> FOutput;

        [Output("Valid", IsToggle = true)]
        public ISpread<bool> FValid;

        [Import()]
        public ILogger FLogger;

        protected bool Bind {get;set;}

        protected Dictionary<string, T> Sockets = new Dictionary<string, T>();
        protected HashSet<string> WorkingSockets = new HashSet<string>();

        public NetMQContext Context
        {
            get
            {
                return NetMQContext.Create();
            }
        }

        #endregion fields & pins

        #region abstract Methods

        protected abstract T NewSocket();

        #endregion abstract methods

        // returns success
        protected virtual bool EnableSocket(bool enable, T socket, string address)
        {
            try
            {
                if (enable)
                {
                    if (Bind) socket.Bind(address);
                    else socket.Connect(address);

                    WorkingSockets.Add(address);
                }
                else
                {
                    if (Bind) socket.Unbind(address);
                    else socket.Disconnect(address);

                    WorkingSockets.Remove(address);
                }

                return true;
            }
            catch (AddressAlreadyInUseException e)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + address + " is already in use.");
                WorkingSockets.Remove(address);
                throw e; // this needs do be escalated to node level. Repatch!
            }

            catch (TerminatingException)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + Sockets[address].GetType().Name + " is terminating already.: Cannot  " + (enable ? "enable" : "disable") + " socket at " + address);
                WorkingSockets.Remove(address);
            }

            catch (EndpointNotFoundException)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: Cannot " + (enable ? "enable" : "disable") + " endpoint of +" + Sockets[address].GetType().Name + ": " + address);
                WorkingSockets.Remove(address);
            }

            catch (ObjectDisposedException)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + Sockets[address].GetType().Name + " is disposed already. Cannot " + (enable ? "enable" : "disable") + " " + address);
                WorkingSockets.Remove(address);
            }

            catch (NetMQException e)
            {
                WorkingSockets.Remove(address);

                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + (enable ? "Enable" : "Disable") + " of " + Sockets[address].GetType().Name + " failed. NetMQ threw an internal exception with " + address + "\n " + e.Message + "\n");
                //                        FLogger.Log(e.InnerException, LogType.Debug);                        
                throw e;
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + address + " threw an internal exception: " + e);
                //               throw e;
            }

            return false;

        }

        
        // will only work for Sub and XSub
        protected virtual bool Subscribe(bool enable, T socket, IEnumerable<string> topic)
        {
            try
            {
                if (enable)
                    foreach (var t in topic) socket.Subscribe(t);
                else foreach (var t in topic) socket.Unsubscribe(t);

                return true;
            }
            catch (Exception e)
            {
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + (enable? "Subscribing":"Unsubscribing") + " threw an internal exception: " + e);
//               throw e;
            }

            return false;
        }
        
        public virtual void Evaluate(int SpreadMax) {

            var addresses = CreateAddresses(); // in proper spread order

            SpreadMax = FProtocol.CombineWith(FAddress).CombineWith(FPort);

            int i = 0;
            foreach (var address in addresses)
            {
                var enabled = WorkingSockets.Contains(address);
                var enable  = FEnable[i];

                i++;

                if (enabled != enable)
                {
                    // bind or connect
                    enabled = EnableSocket(enable, Sockets[address], address) && enable;

                }
            }


            // null all non-working sockets now.
            var partiallyNulled = from socket in Sockets
                                  select WorkingSockets.Contains(socket.Key) ? socket.Value : null;

            FOutput.AssignFrom(partiallyNulled);

            var valid = from socket in Sockets
                        select WorkingSockets.Contains(socket.Key);

            FValid.AssignFrom(valid);    
          
        }



        public virtual void OnImportsSatisfied()
        {
            var context = NetMQContext.Create();

             FPort.Changed += UpdatePort;
            FProtocol.Changed += UpdateProtocol;
            FAddress.Changed += UpdateAddress;
        }

        private void UpdateAddress(IDiffSpread<string> spread)
        {
            UpdateSockets();
        }

        private void UpdateProtocol(IDiffSpread<TransportProtocolEnum> spread)
        {
            UpdateSockets();
        }

        private void UpdatePort(IDiffSpread<int> spread)
        {
            UpdateSockets();
        }

        protected IEnumerable<string> CreateAddresses()
        {
            var needsPort = new List<TransportProtocolEnum>();
            needsPort.Add(TransportProtocolEnum.tcp);
            needsPort.Add(TransportProtocolEnum.pgm);

            var spreadMax = FProtocol.CombineWith(FAddress).CombineWith(FPort);

            for (int i = 0; i < spreadMax; i++)
            {
                var protocol = FProtocol[i];
                var server = FAddress[i];
                var port = FPort[i];


                var p = needsPort.Contains(protocol) ? ":" + port : "";
                                    
                yield return protocol.ToString() + "://" + server + p;
            }
        }
         
        private void UpdateSockets()
        {
            var addresses = CreateAddresses();
            
            var remove = (
                from address in Sockets.Keys.ToArray()
                where !addresses.Contains(address)
                select address
                ).ToArray();



            foreach (var address in remove)
            {
                try
                {
                    var socket = Sockets[address];
                    EnableSocket(false, socket, address);
                    Sockets[address].Dispose();
                }
                catch (Exception e)
                {
                    FLogger.Log(e, LogType.Warning);
                }
                Sockets.Remove(address);
                WorkingSockets.Remove(address);
            }


            var fresh = from address in addresses
                      where !Sockets.ContainsKey(address)
                      select address;

            foreach (var address in fresh)
            {
                var s = Sockets[address] = NewSocket();
            }

        }



        public void Dispose()
        {
            foreach (var address in Sockets.Keys.ToArray())
            {
                try
                {
                    EnableSocket(false, Sockets[address], address); // disable
                }
                catch (Exception e)
                {
                    FLogger.Log(LogType.Warning, "vvvv.ZeroMQ: Could not disable "+address);
                }

                Sockets.Remove(address);

                try {
                    Sockets[address].Dispose();
                } catch (Exception e) {
                    FLogger.Log(LogType.Warning, "vvvv.ZeroMQ: Could not dispose socket " + address);
                }

            }
        }
    }
}
