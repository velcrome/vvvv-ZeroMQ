using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetMQ.Core;
using NetMQ;

namespace VVVV.ZeroMQ.Nodes.Core
{
    /// <summary>
    /// Options overcomes the "convenience" of NetMQ's SocketOptions, that all option descriptions need a socket.
    /// </summary>
    public class Options
    {
        /// <summary>
        /// The dummy data that this SocketOptions is referencing.
        /// </summary>
        private Dictionary<string, object> data = new Dictionary<string, object>();

        public Options()
        {
            // define no defaults

        }

        public static Options Default()
        {
            var o = new Options();
            o.Affinity = 0;

            o.Backlog = 100;
            o.Endian = Endianness.Big;
            o.IPv4Only = true;
            o.MaxMsgSize = -1;
            o.ReceiveHighWatermark = 1000;
            o.SendHighWatermark = 1000;

//  somdoron: linger is actually an integer of milliseconds, so -1 the socket will be closed after all messages sent.
            o.Linger = new TimeSpan(0, 0, 0, 0, -1);
            o.ReconnectInterval = new TimeSpan(0, 0, 0, 0, 100);
            o.ReconnectIntervalMax = new TimeSpan(0, 0, 0, 0);

            o.MulticastHops = 1;
            o.MulticastRate = 100;
            o.MulticastRecoveryInterval = new TimeSpan(0, 0, 0, 10, 0);

//            o.DelayOnClose = true;
//            o.DelayOnDisconnect = true;
//            o.SendTimeout = -1;

            o.TcpKeepalive = false;
            o.TcpKeepaliveIdle = new TimeSpan(0, 0, 0, 0, -1); ;
            o.TcpKeepaliveInterval = new TimeSpan(0, 0, 0, 0, -1); ;

            o.DisableTimeWait = false;

            o.ReceiveBuffer = 0;  // -1 in c
            o.SendBuffer = 0; // -1 in c
            o.DelayAttachOnConnect = false;

            o.RouterRawSocket = false;
            
            o.XPubVerbose = false; // ?
            o.XPubBroadcast = true; // ?

            return o;
     }

        public Options(Options template)
        {
            if (template == null) return;
            
            foreach (var field in template.data.Keys)
            {
                data[field] = template.data[field];
            }
        }

        public void CopyTo(NetMQSocket socket)
        {
            var options = socket.Options;
            var fields = ((IDictionary<string, object>)data).Keys.ToArray();

            foreach (var field in fields)
            {
                switch (field)
                {
                    case "Affinity": options.Affinity = this.Affinity;
                        break;
                    case "Backlog": options.Backlog = this.Backlog;
                        break;
                    case "DelayAttachOnConnect": options.DelayAttachOnConnect = this.DelayAttachOnConnect;
                        break;
                    case "Endian": options.Endian = this.Endian;
                        break;
                    case "IPv4Only": options.IPv4Only = this.IPv4Only;
                        break;
                    case "Identity": options.Identity = this.Identity;
                        break;
                    case "Linger": options.Linger = this.Linger;
                        break;
                    case "MaxMsgSize": options.MaxMsgSize = this.MaxMsgSize;
                        break;
                    case "MulticastHops": options.MulticastHops = this.MulticastHops;
                        break;
                    case "MulticastRate": options.MulticastRate = this.MulticastRate;
                        break;
                    case "MulticastRecoveryInterval": options.MulticastRecoveryInterval = this.MulticastRecoveryInterval;
                        break;
                    case "SendHighWatermark": options.SendHighWatermark = this.SendHighWatermark;
                        break;
                    case "ReceiveHighWatermark": options.ReceiveHighWatermark = this.ReceiveHighWatermark;
                        break;
                    case "SendBuffer": options.SendBuffer = this.SendBuffer;
                        break;
                    case "ReconnectIntervalMax": options.ReconnectIntervalMax = this.ReconnectIntervalMax;
                        break;
                    case "ReconnectInterval": options.ReconnectInterval = this.ReconnectInterval;
                        break;
                    case "ReceiveBuffer": options.ReceiveBuffer = this.ReceiveBuffer;
                        break;

                    case "TcpKeepalive": options.TcpKeepalive = this.TcpKeepalive;
                        break;
                    case "TcpKeepaliveIdle": options.TcpKeepaliveIdle = this.TcpKeepaliveIdle;
                        break;
                    case "TcpKeepaliveInterval": options.TcpKeepaliveInterval = this.TcpKeepaliveInterval;
                        break;
                    case "XPubVerbose": options.XPubVerbose = this.XPubVerbose;
                        break;


                }
            }


        }

        /// <summary>
        /// Get or set the I/O-thread affinity. This is a 64-bit value used to specify  which threads from the I/O thread-pool
        /// associated with the socket's context shall handle newly-created connections.
        /// 0 means no affinity, meaning that work shall be distributed fairly among all I/O threads.
        /// For non-zero values, the lowest bit corresponds to thread 1, second lowest bit to thread 2, and so on.
        /// </summary>
        public long Affinity
        {
            get { return (long) data["Affinity"]; }
            set { data["Affinity"] = value; }
        }

        /// <summary>
        /// Get or set unique identity of the socket, from a message-queueing router's perspective.
        /// This is a byte-array of at most 255 bytes.
        /// </summary>
        public byte[] Identity
        {
            get { return (byte[])data["Identity"]; }
            set { data["Identity"] = value; }
        }

        /// <summary>
        /// Get or set the maximum send or receive data rate for multicast transports on the specified socket.
        /// </summary>
        public int MulticastRate
        {
            get { return (int) data["Rate"]; }
            set { data["Rate"] = value; }
        }

        /// <summary>
        /// Get or set the recovery-interval for multicast transports using the specified socket.
        /// This option determines the maximum time that a receiver can be absent from a multicast group
        /// before unrecoverable data loss will occur. Default is 10,000 ms (10 seconds).
        /// </summary>
        public TimeSpan MulticastRecoveryInterval
        {
            get { return (TimeSpan) data["RecoveryIvl"]; }
            set { data["RecoveryIvl"] = value; }
        }

        /// <summary>
        /// Get or set the size of the transmit buffer for the specified socket.
        /// </summary>
        public int SendBuffer
        {
            get { return (int) data["SendBuffer"]; }
            set { data["SendBuffer"] = value; }
        }

        /// <summary>
        /// Get or set the size of the receive buffer for the specified socket.
        /// A value of zero means that the OS default is in effect.
        /// </summary>
        public int ReceiveBuffer
        {
            get { return (int) data["ReceiveBuffer"]; }
            set { data["ReceiveBuffer"] = value; }
        }

        /// <summary>
        /// Get or set the linger period for the specified socket,
        /// which determines how long pending messages which have yet to be sent to a peer
        /// shall linger in memory after a socket is closed.
        /// </summary>
        /// <remarks>
        /// This also affects the termination of the socket's context.
        /// -1: The default value of -1 specifies an infinite linger period. Pending messages shall not be discarded after the socket is closed;
        /// attempting to terminate the socket's context shall block until all pending messages have been sent to a peer.
        /// 0: Specifies no linger period. Pending messages shall be discarded immediately when the socket is closed.
        /// Positive values specify an upper bound for the linger period. Pending messages shall not be discarded after the socket is closed;
        /// attempting to terminate the socket's context shall block until either all pending messages have been sent to a peer,
        /// or the linger period expires, after which any pending messages shall be discarded.
        /// </remarks>
        public TimeSpan Linger
        {
            get { return (TimeSpan)data["Linger"]; }
            set { data["Linger"] = value; }
        }

        /// <summary>
        /// Get or set the initial reconnection interval for the specified socket.
        /// This is the period to wait between attempts to reconnect disconnected peers
        /// when using connection-oriented transports. The default is 100 ms.
        /// -1 means no reconnection.
        /// </summary>
        /// <remarks>
        /// With ZeroMQ, the reconnection interval may be randomized to prevent reconnection storms
        /// in topologies with a large number of peers per socket.
        /// </remarks>
        public TimeSpan ReconnectInterval
        {
            get { return (TimeSpan) data["ReconnectInterval"]; }
            set { data["ReconnectInterval"] = value; }
        }

        /// <summary>
        /// Get or set the maximum reconnection interval for the specified socket.
        /// This is the maximum period to shall wait between attempts
        /// to reconnect. On each reconnect attempt, the previous interval shall be doubled
        /// until this maximum period is reached.
        /// The default value of zero means no exponential backoff is performed.
        /// </summary>
        /// <remarks>
        /// This is the maximum period NetMQ shall wait between attempts
        /// to reconnect. On each reconnect attempt, the previous interval shall be doubled
        /// until this maximum period is reached.
        /// This allows for an exponential backoff strategy.
        /// The default value of zero means no exponential backoff is performed
        /// and reconnect interval calculations are only based on ReconnectIvl.
        /// </remarks>
        public TimeSpan ReconnectIntervalMax
        {
            get { return (TimeSpan) data["ReconnectIvlMax"]; }
            set { data["ReconnectIvlMax"] = value; }
        }

        /// <summary>
        /// Get or set the maximum length of the queue of outstanding peer connections
        /// for the specified socket. This only applies to connection-oriented transports.
        /// Default is 100.
        /// </summary>
        public int Backlog
        {
            get { return (int)data["Backlog"]; }
            set { data["Backlog"] = value; }
        }

        /// <summary>
        /// Get or set the upper limit to to the size for inbound messages.
        /// If a peer sends a message larger than this it is disconnected.
        /// The default value is -1, which means no limit.
        /// </summary>
        public long MaxMsgSize
        {
            get { return (int)data["MaxMessageSize"]; }
            set { data["MaxMessageSize"] = value; }
        }

        /// <summary>
        /// Get or set the high-water-mark for transmission.
        /// This is a hard limit on the number of messages that are allowed to queue up
        /// before mitigative action is taken.
        /// The default is 1000.
        /// </summary>
        public int SendHighWatermark
        {
            get { return (int)data["SendHighWatermark"]; }
            set { data["SendHighWatermark"] = value; }
        }

        /// <summary>
        /// Get or set the high-water-mark for reception.
        /// This is a hard limit on the number of messages that are allowed to queue up
        /// before mitigative action is taken.
        /// The default is 1000.
        /// </summary>
        public int ReceiveHighWatermark
        {
            get { return (int)data["ReceiveHighWatermark"]; }
            set { data["ReceiveHighWatermark"] = value; }

        }

        /// <summary>
        /// Get or set the time-to-live (maximum number of hops) that outbound multicast packets
        /// are allowed to propagate.
        /// The default value of 1 means that the multicast packets don't leave the local network.
        /// </summary>
        public int MulticastHops
        {
            get { return (int)data["MulticastHops"]; }
            set { data["MulticastHops"] = value; }
        }

        /// <summary>
        /// Get or set whether the underlying socket is for IPv4 only (not IPv6),
        /// as opposed to one that allows connections with either IPv4 or IPv6.
        /// </summary>
        public bool IPv4Only
        {
            get { return (bool)data["IPv4Only"]; }
            set { data["IPv4Only"] = value; }
        }

        /// <summary>
        /// Get or set whether to use TCP keepalive.
        /// </summary>
        /// <remarks>
        /// When Keepalive is enabled, then your socket will periodically send an empty keepalive probe packet
        /// with the ACK flag on. The remote endpoint does not need to support keepalive at all, just TCP/IP.
        /// If you receive a reply to your keepalive probe, you can assume that the connection is still up and running.
        /// This procedure is useful because if the other peers lose their connection (for example, by rebooting)
        /// you will notice that the connection is broken, even if you don't have traffic on it.
        /// If the keepalive probes are not replied to by your peer, you can assert that the connection
        /// cannot be considered valid and then take the corrective action.
        /// </remarks>
        public bool TcpKeepalive
        {
            get { return (bool)data["TcpKeepalive"]; }
            set { data["TcpKeepalive"] = value; }
            // TODO: What about the value -1, which means use the OS default?  jh
            // See  http://api.zeromq.org/3-2:zmq-getsockopt
        }

        /// <summary>
        /// Get or set the keep-alive time - the duration between two keepalive transmissions in idle condition.
        /// The TCP keepalive period is required by socket implementers to be configurable and by default is
        /// set to no less than 2 hours.
        /// </summary>
        public TimeSpan TcpKeepaliveIdle
        {
            get { return (TimeSpan) data["TcpKeepaliveIdle"]; }
            set { data["TcpKeepaliveIdle"] = value; }
            

            // TODO: What about the value -1, which means use the OS default?  jh
            // See  http://api.zeromq.org/3-2:zmq-getsockopt
        }

        /// <summary>
        /// Get or set the TCP keep-alive interval - the duration between two keepalive transmission if no response was received to a previous keepalive probe.
        /// </summary>
        /// <remarks>
        /// By default a keepalive packet is sent every 2 hours or 7,200,000 milliseconds
        /// (TODO: Check these comments concerning default values!  jh)
        /// if no other data have been carried over the TCP connection.
        /// If there is no response to a keepalive, it is repeated once every KeepAliveInterval seconds.
        /// The default is one second.
        /// </remarks>
        public TimeSpan TcpKeepaliveInterval
        {
            get { return (TimeSpan) data["TcpKeepaliveIntvl"]; }
            set { data["TcpKeepaliveIntvl"] = value; }
        }

        /// <summary>
        /// Get or set the attach-on-connect value.
        /// If set to true, this will delay the attachment of a pipe on connect until
        /// the underlying connection has completed. This will cause the socket
        /// to block if there are no other connections, but will prevent queues
        /// from filling on pipes awaiting connection.
        /// Default is false.
        /// </summary>
        public bool DelayAttachOnConnect
        {
            get { return (bool)data["DelayAttachOnConnect"]; }
            set { data["DelayAttachOnConnect"] = value; }
        }

        /// <summary>
        /// This applies only to publisher sockets.
        /// Set whether to send all subscription messages upstream, not just unique ones.
        /// The default is false.
        /// </summary>
        public bool XPubVerbose
        {
            get { return (bool)data["XPubVerbose"]; }
            set { data["XPubVerbose"] = value; }
        }


        /// <summary>
        /// This applies only to publisher sockets.
        /// Set whether to support broadcast functionality
        /// </summary>
        public bool XPubBroadcast {
            get { return (bool)data["XPubBroadcast"]; }
            set { data["XPubBroadcast"] = value; }
        }


        public bool RouterRawSocket
        {
            get { return (bool)data["RouterRawSocket"]; }
            set { data["RouterRawSocket"] = value; }
        }

        /// <summary>
        /// Get or set the byte-order: big-endian, vs little-endian.
        /// </summary>
        public Endianness Endian
        {
            get { return (Endianness)data["Endian"]; }
            set { data["Endian"] = value; }
        }

        public bool DisableTimeWait
        {
            get { return (bool)data["DisableTimeWait"]; }
            set { data["DisableTimeWait"] = value; }
        }

        public override String ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("vvvv-ZeroMQ Option");

            var fields = ((IDictionary<string, object>)(data));
            foreach (var name in fields.Keys.ToArray())
            {
               sb.AppendLine("["+name+"] \t= "+fields[name].ToString());
            }
            sb.AppendLine("vvvv-ZeroMQ ======");
           
            
            return sb.ToString();
        }
    }
}