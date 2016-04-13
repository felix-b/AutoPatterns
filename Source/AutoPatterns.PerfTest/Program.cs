using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess;
using AutoPatterns.Tests;
using AutoPatterns.Tests.Examples;

namespace AutoPatterns.PerfTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestLibrary.UseRemoteCompilerService();
            new TcpRemoteEndpointFactory(50555).EnsureCompilerHostIsUp();
            var test = new ExampleTests();
            test.ExampleAutomaticPropertyAndDataContract();
        }
    }
}
