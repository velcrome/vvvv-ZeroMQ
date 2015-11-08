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
    [PluginInfo(Name = "Send", AutoEvaluate = true, Category = "0qm", Help = "Sends to a socket", Tags = "", Author = "velcrome")]
    #endregion PluginInfo
    public class SendNode : IPluginEvaluate
    {
        #region fields & pins

        [Input("Socket")]
        public ISpread<IOutgoingSocket> FSocket; 

        [Input("Data")]
        public ISpread<ISpread<Stream>> FInput;

        [Input("Send", IsBang = true, DefaultBoolean=true)]
        public ISpread<bool> FSend;
        
        [Output("Success", IsBang=true)]
        public ISpread<bool> FSuccess; 

        [Import()]
        public ILogger FLogger;

        #endregion fields & pins

        public void Evaluate(int SpreadMax)
        {
            if (FSocket.SliceCount == 0 || (FSocket.SliceCount == 1 && FSocket[0] == null))
            {
                FSuccess.SliceCount = 1;
                FSuccess[0] = false;
                return;
            }
            //            var timeout = new TimeSpan(0, 0, 0, 1); // 1 sec

            SpreadMax = FSocket.CombineWith(FInput); // FSent is cut off at the amount of FSockets
            FSuccess.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                var socket = FSocket[i];
                if (socket == null || FSend[i] == false) // socket has not been enabled, nor is any sending necessary
                {
                    FSuccess[i] = false;
                    continue; 
                }

                FSuccess[i] = true; // if just one frame fails, it will turn to false

                var dataBin = FInput[i];
                var binCount = dataBin.SliceCount;
                for (int j = 0; j < binCount; j++)
                {
                    var raw = dataBin[j];
                    var length = (int)raw.Length;
                    var buffer = new byte[length];

                    raw.Seek(0, SeekOrigin.Begin);
                    int c = raw.Read(buffer, 0, length);

                    bool more = j + 1 < binCount;
                    try
                    {
                        if (length > 0)
                        {
                            FSuccess[i] &= socket.TrySendFrame(buffer, length, more); 
                        }
                        else
                        {
                            FSuccess[i] &= socket.TrySendFrameEmpty(more);
                        }
                    }
                    catch (FiniteStateMachineException)
                    {
                        // likely cause: your send is blocked because nothing received yet.
                        // todo: FiniteStateMachineException under watch

                        FLogger.Log(LogType.Warning, "vvvv.ZeroMQ: Not allowed to send yet. \n");
//                        FLogger.Log(e, LogType.Warning);
                    }
                    catch (NotSupportedException e)
                    {
                        FSuccess[i] = false;
                        FLogger.Log(LogType.Error, "\nvvvv-ZeroMQ: "+socket.GetType() +" does not supoort Send. NotSupportedException.");
                        throw e;
                    }
                    
                    catch (Exception e)
                    {
                        FSuccess[i] = false;
                        FLogger.Log(LogType.Error, "\nvvvv-ZeroMQ: Problem in Send. \n" + e);
                        throw e; // internal error. escalate to node level?
                    }
                }
            }
        }

    }
}
