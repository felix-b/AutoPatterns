using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess.RequestReply;

namespace AutoPatterns.OutOfProcess
{
    public class TcpRemoteEndpointFactory : RemoteEndpointFactory
    {
        private readonly int _tcpPortNumber;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TcpRemoteEndpointFactory(int tcpPortNumber)
        {
            _tcpPortNumber = tcpPortNumber;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of RemoteEndpointFactory

        public override IServiceHost CreateServiceHost(RemoteCompilerService service)
        {
            return new TcpServiceHost(service, _tcpPortNumber, workerThreadCount: 3);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IRemoteCompilerService CreateClient()
        {
            return new TcpServiceClient(_tcpPortNumber);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CallCompilerService(Action<IRemoteCompilerService> doCall)
        {
            using (var client = new TcpServiceClient(_tcpPortNumber))
            {
                doCall(client);
            }
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static class Protocol
        {
            public static void WriteActionByte(ActionByte value, Stream stream)
            {
                stream.WriteByte((byte)value);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ActionByte ReadActionByte(Stream stream)
            {
                return (ActionByte)stream.ReadByte();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void WriteCompileRequest(CompileRequest request, Stream stream)
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(request.AssemblyName);
                    writer.Write(request.EnableDebug);
                    writer.Write(request.SourceCode);
                    writer.Write(request.ReferencePaths.Length);

                    for (int i = 0 ; i < request.ReferencePaths.Length ; i++)
                    {
                        writer.Write(request.ReferencePaths[i]);
                    }

                    writer.Flush();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static CompileRequest ReadCompileRequest(Stream stream)
            {
                var request = new CompileRequest();

                using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
                {
                    request.AssemblyName = reader.ReadString();
                    request.EnableDebug = reader.ReadBoolean();
                    request.SourceCode = reader.ReadString();

                    var referencePathCount = reader.ReadInt32();
                    request.ReferencePaths = new string[referencePathCount];

                    for (int i = 0; i < referencePathCount; i++)
                    {
                        request.ReferencePaths[i] = reader.ReadString();
                    }
                }

                return request;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void WriteReplyByte(ReplyByte value, Stream stream)
            {
                stream.WriteByte((byte)value);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static ReplyByte ReadReplyByte(Stream stream)
            {
                return (ReplyByte)stream.ReadByte();
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void WriteCompileReply(CompileReply reply, Stream stream)
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(reply.DllBytes.Length);
                    writer.Write(reply.DllBytes);

                    if (reply.PdbBytes != null)
                    {
                        writer.Write(reply.PdbBytes.Length);
                        writer.Write(reply.PdbBytes);
                    }
                    else
                    {
                        writer.Write(0);
                    }

                    writer.Write(reply.Success);

                    if (reply.Errors != null)
                    {
                        writer.Write(reply.Errors.Count);

                        for (int i = 0; i < reply.Errors.Count; i++)
                        {
                            writer.Write(reply.Errors[i]);
                        }
                    }
                    else
                    {
                        writer.Write(0);
                    }

                    writer.Flush();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static CompileReply ReadCompileReply(Stream stream)
            {
                var reply = new CompileReply();

                using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
                {
                    var dllBytesLength = reader.ReadInt32();
                    reply.DllBytes = reader.ReadBytes(dllBytesLength);

                    var pdbBytesLength = reader.ReadInt32();
                    if (pdbBytesLength > 0)
                    {
                        reply.PdbBytes = reader.ReadBytes(pdbBytesLength);
                    }

                    reply.Success = reader.ReadBoolean();

                    var errorCount = reader.ReadInt32();
                    if (errorCount > 0)
                    {
                        reply.Errors = new string[errorCount];

                        for (int i = 0 ; i < errorCount; i++)
                        {
                            reply.Errors[i] = reader.ReadString();
                        }
                    }
                }

                return reply;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static void WriteFault(Exception fault, Stream stream)
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write($"{fault.GetType().FullName}: {fault.Message}");
                    writer.Flush();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public static Exception ReadFault(Stream stream)
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true))
                {
                    return new Exception(reader.ReadString());
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public enum ActionByte : byte
            {
                Hello = 1,
                Compile = 2,
                GoodBye = 3
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public enum ReplyByte : byte
            {
                OK = 1,
                Fault = 2,
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TcpServiceClient : IRemoteCompilerService, IDisposable
        {
            private readonly TcpClient _tcpClient;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TcpServiceClient(int tcpPortNumber)
            {
                _tcpClient = new TcpClient("localhost", tcpPortNumber);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IDisposable

            public void Dispose()
            {
                _tcpClient.Close();
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IRemoteCompilerService

            public void Hello()
            {
                using (var stream = _tcpClient.GetStream())
                {
                    Protocol.WriteActionByte(Protocol.ActionByte.Hello, stream);
                    stream.Flush();

                    if (Protocol.ReadReplyByte(stream) == Protocol.ReplyByte.Fault)
                    {
                        throw Protocol.ReadFault(stream);
                    }
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public CompileReply Compile(CompileRequest request)
            {
                using (var stream = _tcpClient.GetStream())
                {
                    Protocol.WriteActionByte(Protocol.ActionByte.Compile, stream);
                    Protocol.WriteCompileRequest(request, stream);

                    stream.Flush();

                    if (Protocol.ReadReplyByte(stream) == Protocol.ReplyByte.Fault)
                    {
                        throw Protocol.ReadFault(stream);
                    }

                    return Protocol.ReadCompileReply(stream);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void GoodBye()
            {
                using (var stream = _tcpClient.GetStream())
                {
                    Protocol.WriteActionByte(Protocol.ActionByte.GoodBye, stream);
                    stream.Flush();

                    if (Protocol.ReadReplyByte(stream) == Protocol.ReplyByte.Fault)
                    {
                        throw Protocol.ReadFault(stream);
                    }
                }
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private class TcpServiceHost : IServiceHost
        {
            private readonly RemoteCompilerService _service;
            private readonly int _tcpPortNumber;
            private readonly BlockingCollection<Socket> _pendingRequests;
            private readonly Task[] _workerThreads;
            private TcpListener _tcpListener;
            private Task _listenerThread;
            private CancellationTokenSource _cancellation;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TcpServiceHost(RemoteCompilerService service, int tcpPortNumber, int workerThreadCount)
            {
                _service = service;
                _tcpPortNumber = tcpPortNumber;
                _pendingRequests = new BlockingCollection<Socket>();
                _workerThreads = new Task[workerThreadCount];
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Implementation of IServiceHost

            public void Open()
            {
                _cancellation = new CancellationTokenSource();
                _tcpListener = new TcpListener(IPAddress.Any, _tcpPortNumber);
                _listenerThread = RunListenerThread();

                for (int i = 0 ; i < _workerThreads.Length ; i++)
                {
                    _workerThreads[i] = Task.Run(new Action(RunWorkerThread), _cancellation.Token);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public void Close(TimeSpan timeout)
            {
                var clock = Stopwatch.StartNew();

                _cancellation.Cancel();
                _tcpListener.Stop();
                _listenerThread.Wait(timeout);

                var remainingTimeout = timeout.Subtract(clock.Elapsed);

                try
                {
                    Task.WaitAll(
                        _workerThreads, 
                        remainingTimeout > TimeSpan.Zero ? remainingTimeout : TimeSpan.Zero);
                }
                catch (TaskCanceledException)
                {
                }
                catch (AggregateException aggregate)
                {
                    var realErrors = aggregate.Flatten().InnerExceptions.Where(e => !(e is TaskCanceledException)).ToArray();
                    if (realErrors.Length > 0)
                    {
                        throw new AggregateException(realErrors);
                    }
                }
            }

            #endregion

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private async Task RunListenerThread()
            {
                _tcpListener.Start(backlog: 100);

                while (!_cancellation.IsCancellationRequested)
                {
                    Socket socket;

                    try
                    {
                        socket = await _tcpListener.AcceptSocketAsync();
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }

                    _pendingRequests.Add(socket);
                    //var stream = new NetworkStream(socket);
                    //HandleRequest(stream);
                    //stream.Flush();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void RunWorkerThread()
            {
                while (!_cancellation.IsCancellationRequested)
                {
                    Socket socket;

                    try
                    {
                        socket = _pendingRequests.Take(_cancellation.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    var stream = new NetworkStream(socket);
                    HandleRequest(stream);
                    stream.Flush();
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void HandleRequest(NetworkStream stream)
            {
                Exception fault;
                CompileReply reply;

                try
                {
                    reply = DispatchServiceCall(stream);
                    fault = null;
                }
                catch (Exception e)
                {
                    reply = null;
                    fault = e;
                }

                if (fault == null)
                {
                    Protocol.WriteReplyByte(Protocol.ReplyByte.OK, stream);
                    if (reply != null)
                    {
                        Protocol.WriteCompileReply(reply, stream);
                    }
                }
                else
                {
                    Protocol.WriteReplyByte(Protocol.ReplyByte.Fault, stream);
                    Protocol.WriteFault(fault, stream);
                }
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private CompileReply DispatchServiceCall(NetworkStream stream)
            {
                var action = Protocol.ReadActionByte(stream);

                switch (action)
                {
                    case Protocol.ActionByte.Hello:
                        _service.Hello();
                        break;
                    case Protocol.ActionByte.GoodBye:
                        _service.GoodBye();
                        break;
                    case Protocol.ActionByte.Compile:
                        var request = Protocol.ReadCompileRequest(stream);
                        return _service.Compile(request);
                    default:
                        throw new InvalidDataException($"Unknown action byte: {(int)action}");
                }

                return null;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------
        }
    }
}
