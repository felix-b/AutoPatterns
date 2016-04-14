using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess.RequestReply;
using AutoPatterns.Runtime;

namespace AutoPatterns.OutOfProcess
{
    public class RemotePatternCompiler : IPatternCompiler
    {
        private readonly RemoteEndpointFactory _endpointFactory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public RemotePatternCompiler(int tcpPortNumber = 50555)
        {
            _endpointFactory = new TcpRemoteEndpointFactory(tcpPortNumber);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IPatternCompiler

        public bool CompileAssembly(
            string assemblyName,
            string sourceCode,
            string[] references,
            bool enableDebug,
            out byte[] dllBytes,
            out byte[] pdbBytes,
            out string[] errors)
        {
            var clock = Stopwatch.StartNew();

            var request = new CompileRequest() {
                AssemblyName = assemblyName,
                SourceCode = sourceCode,
                ReferencePaths = references,
                EnableDebug = enableDebug
            };

            CompileReply reply = null;

            try
            {
                Console.WriteLine($"PERF >> RemotePatternCompiler::CompileAssembly # 1 >> {clock.ElapsedMilliseconds} ms");

                _endpointFactory.CallCompilerService(
                    client => {
                        Console.WriteLine($"PERF >> RemotePatternCompiler::CompileAssembly # 2 >> {clock.ElapsedMilliseconds} ms");
                        reply = client.Compile(request);
                        Console.WriteLine($"PERF >> RemotePatternCompiler::CompileAssembly # 3 >> {clock.ElapsedMilliseconds} ms");
                    });

                Console.WriteLine($"PERF >> RemotePatternCompiler::CompileAssembly # 4 >> {clock.ElapsedMilliseconds} ms");

                dllBytes = reply.DllBytes;
                pdbBytes = reply.PdbBytes;
                errors = reply.Errors?.ToArray();

                Console.WriteLine($"PERF >> RemotePatternCompiler::CompileAssembly # 5 >> {clock.ElapsedMilliseconds} ms");

                return reply.Success;
            }
            catch (Exception e)
            {
                dllBytes = null;
                pdbBytes = null;
                errors = new[] { $"Failed to call remote compiler service! {e.GetType().FullName}: {e.Message}" };
                return false;
            }
        }

        #endregion
    }
}
