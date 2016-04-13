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
    public class WcfRemoteEndpointFactory : RemoteEndpointFactory
    {
        private readonly int _tcpPortNumber;
        private readonly ChannelFactory<IRemoteCompilerService> _wcfCannelFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public WcfRemoteEndpointFactory(int tcpPortNumber)
        {
            _tcpPortNumber = tcpPortNumber;
            _wcfCannelFactory = new ChannelFactory<IRemoteCompilerService>(CreateServiceEndpoint());
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override IRemoteCompilerService CreateClient()
        {
            return _wcfCannelFactory.CreateChannel();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public override void CallCompilerService(Action<IRemoteCompilerService> doCall)
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

        public override IServiceHost CreateServiceHost(RemoteCompilerService service)
        {
            var host = new WcfServiceHost(service);
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

        private class WcfServiceHost : ServiceHost, IServiceHost
        {
            public WcfServiceHost(object singletonServiceInstance) 
                : base(singletonServiceInstance)
            {
            }
        }
    }
}
