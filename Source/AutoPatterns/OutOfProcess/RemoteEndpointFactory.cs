using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AutoPatterns.OutOfProcess
{
    public class RemoteEndpointFactory
    {
        private readonly int _tcpPortNumber;
        private readonly ChannelFactory<IRemoteCompilerService> _channelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RemoteEndpointFactory(int tcpPortNumber)
        {
            _tcpPortNumber = tcpPortNumber;
            _channelFactory = new ChannelFactory<IRemoteCompilerService>(CreateServiceEndpoint());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public IRemoteCompilerService CreateClient()
        {
            return _channelFactory.CreateChannel();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void CallCompilerService(Action<IRemoteCompilerService> doCall)
        {
            var client = CreateClient();

            try
            {
                doCall(client);
                ((ICommunicationObject)client).Close();
            }
            catch (CommunicationException)
            {
                ((ICommunicationObject)client).Abort();
                throw;
            }
            catch
            {
                ((ICommunicationObject)client).Close();
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureCompilerHostIsUp()
        {
            Mutex mutex;

            if (Mutex.TryOpenExisting(CompilerHostMutexName, out mutex))
            {
                mutex.Dispose();
                return;
            }

            StartCompilerHostProcess();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureCompilerHostIsDown()
        {
            Mutex mutex;

            if (Mutex.TryOpenExisting(CompilerHostMutexName, out mutex))
            {
                mutex.Dispose();

                try
                {
                    CallCompilerService(client => client.GoodBye());
                }
                catch
                {
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public ServiceHost CreateServiceHost(RemoteCompilerService service)
        {
            var host = new ServiceHost(service);
            host.AddServiceEndpoint(CreateServiceEndpoint());
            return host;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ServiceEndpoint CreateServiceEndpoint()
        {
            var contract = ContractDescription.GetContract(typeof(IRemoteCompilerService));
            return new ServiceEndpoint(contract, CreateTcpBinding(), new EndpointAddress($"net.tcp://localhost:{_tcpPortNumber}"));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private Binding CreateTcpBinding()
        {
            return new NetTcpBinding(SecurityMode.None) {
                ReaderQuotas = XmlDictionaryReaderQuotas.Max
            };
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void StartCompilerHostProcess()
        {
            var directory = Path.GetDirectoryName(this.GetType().Assembly.Location);
            var compilerHostExeFilePath = Path.Combine(directory, "AutoPatterns.CompilerHost.exe");
            ProcessStartInfo info = new ProcessStartInfo(compilerHostExeFilePath);
            info.UseShellExecute = false; // causes consoles to share window 

            Console.WriteLine("STARTING COMPILER HOST...");
            Process.Start(info);

            for (int retry = 10; retry > 0; retry--)
            {
                Thread.Sleep(200);

                try
                {
                    CallCompilerService(client => client.Hello());
                    return;
                }
                catch
                {
                }
            }

            throw new TimeoutException("Compiler host could not be properly started.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public static string CompilerHostMutexName = @"Global\8FDE12BC-3DEC-48AA-913F-3D392883356F";
    }
}
