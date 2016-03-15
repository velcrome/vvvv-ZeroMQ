using NetMQ;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;
using VVVV.ZeroMQ.Nodes.Core;

namespace VVVV.ZeroMQ.Nodes
{
    public abstract class AbstractSocketNode<T> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable where T:NetMQSocket
    {
        protected const string AUTHOR = "velcrome";
        protected const string SOCKET_CATEGORY = "Network.ZeroMQ";
//        protected const string SOCKET_VERSION = "";
        protected const string TAGS = "Socket";

        #region fields & pins
//      Changing any part of this will dispose all         
        [Input("Protocol", DefaultEnumEntry = "tcp")]
        public IDiffSpread<TransportProtocolEnum> FProtocol;

        [Input("Server", DefaultString = "localhost")]
        public IDiffSpread<string> FAddress;

        [Input("Port", DefaultValue = 4444, MinValue= 1024, MaxValue=Int16.MaxValue)]
        public IDiffSpread<int> FPort;

        [Input("Options", Visibility = PinVisibility.Hidden)]
        public IDiffSpread<Options> FOptions;

        // unless FEnable is last pin in node, other pins will not hold valid data whin it is first changed.
        [Input("Enable", DefaultBoolean = false, Order = int.MaxValue, IsToggle=true)]
        public IDiffSpread<bool> FEnable;

        [Output("Sockets")]
        public ISpread<T> FOutput;

        [Output("Valid", IsToggle = true)]
        public ISpread<bool> FValid;

        [Import()]
        public ILogger FLogger;

        bool bind;
        protected bool Bind { get { return bind; }
        set
            {
                // all enabled off
                foreach (var address in WorkingSockets.ToArray())
                    EnableSocket(false, Sockets[address], address);

                bind = value;

                // all on
                foreach (var address in Sockets.Keys)
                    EnableSocket(false, Sockets[address], address);
            }
        }

        protected Dictionary<string, T> Sockets = new Dictionary<string, T>();
        protected HashSet<string> WorkingSockets = new HashSet<string>();


        #endregion fields & pins



        #region enable/disable
        /// <summary>
        /// Enable or Disable a socket with <paramref name="address"/>.
        /// </summary>
        /// <param name="address">a string representing the address to enable or disable this socket with</param>
        /// <exception cref="AddressAlreadyInUseException">The specified address is already in use.</exception>
        /// <exception cref="NetMQException">No IO thread was found, or the protocol's listener encountered an
        /// error during initialisation.</exception>
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
                FLogger.Log(LogType.Error, "\nvvvv.ZeroMQ: " + address + " threw an internal exception: " + e.Message+"\n");
                throw e;
            }

            return false;

        }

        #endregion enable/disable



        #region Socket Lifecycle
        public virtual void OnImportsSatisfied()
        {
            FPort.Changed += UpdatePort;
            FProtocol.Changed += UpdateProtocol;
            FAddress.Changed += UpdateAddress;

        }

        protected abstract NetMQSocket NewSocket(string address = null);

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

        private void UpdateSockets()
        {

            var addresses = CreateAddresses().ToArray();

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
                    FLogger.Log(LogType.Warning, "vvvv-ZeroMQ: Internal Error while removing old socket: " + address + "\n" + e.Message);
                }
                Sockets.Remove(address);
                WorkingSockets.Remove(address);
            }

            var fresh = (
                            from address in addresses
                            where !Sockets.ContainsKey(address)
                            select address
                        ).Distinct().ToArray();

            foreach (var address in fresh)
            {
                try
                {
                    var s = NewSocket() as T;
                    Sockets[address] = s;
                }
                catch (NetMQException e)
                {
                    FLogger.Log(LogType.Error, "vvvv-ZeroMQ: Error while creating new socket for " + address +". \n"+e.Message+"\n"+e.ErrorCode);
                }
                catch (Exception e)
                {
                    FLogger.Log(LogType.Error, "vvvv-ZeroMQ: Unknown Error while creating new socket for " + address);
                    throw e;
                }
            }

        }


        #endregion Socket Lifecycle

        #region Evaluate

        public virtual void Evaluate(int SpreadMax) {
            if (FEnable.SliceCount <= 0) return;

            var addresses = CreateAddresses(); // in proper spread order

            SpreadMax = FProtocol.CombineWith(FAddress).CombineWith(FPort);

            int i = 0;
            foreach (var address in addresses)
            {
                var enabled = WorkingSockets.Contains(address);
                var enable  = FEnable[i];
                
                var switchOn = false;

                if (enabled != enable)
                {
                    // bind or connect
                    switchOn =
                    enabled = EnableSocket(enable, Sockets[address], address) && enable;

                }

                if (FOptions.IsChanged || switchOn)
                {
                    var socket = Sockets[address];
                    var options = FOptions.SliceCount == 0 ? null : FOptions[i];

                    if (options != null)
                        options.CopyTo(socket);
//                    else Options.Default().CopyTo(socket); // TODO: Enable this do reset options on link disconnect
                }

                i++;
            }


            // null all non-working sockets now.
            var partiallyNulled = from socket in Sockets
                                  select WorkingSockets.Contains(socket.Key) ? socket.Value : null;

            FOutput.AssignFrom(partiallyNulled);

            var valid = from socket in Sockets
                        select WorkingSockets.Contains(socket.Key);

            FValid.AssignFrom(valid.DefaultIfEmpty(false));    
          
        }

        #endregion Evaluate




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


        public void Dispose()
        {
            foreach (var address in WorkingSockets.ToArray())
            {
               EnableSocket(false, Sockets[address], address); // disable
            }

            foreach (var socket in Sockets.Values) {
                try {
                    socket.Dispose();
                } catch (Exception) {
                    FLogger.Log(LogType.Warning, "vvvv.ZeroMQ: Could not dispose socket. ");
                }


            }
        }
    }
}
