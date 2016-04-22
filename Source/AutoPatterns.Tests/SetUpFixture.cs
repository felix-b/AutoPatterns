using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess;
using NUnit.Framework;

namespace AutoPatterns.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        //private readonly RemoteEndpointFactory _endpointFactory = new TcpRemoteEndpointFactory(tcpPortNumber: 50555);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [SetUp]
        public void RunBeforeAnyTests()
        {
            TestLibrary.UseInProcessCompiler();
            //TestLibrary.UseRemoteCompilerService();
            //_endpointFactory.EnsureCompilerHostIsUp();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------


        [TearDown]
        public void RunAfterAnyTests()
        {
            //_endpointFactory.EnsureCompilerHostIsDown();
        }
    }
}
