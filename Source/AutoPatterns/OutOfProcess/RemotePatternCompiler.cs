using System;
using System.Collections.Generic;
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
            _endpointFactory = new RemoteEndpointFactory(tcpPortNumber);
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
            var request = new CompileRequest() {
                AssemblyName = assemblyName,
                SourceCode = sourceCode,
                ReferencePaths = references,
                EnableDebug = enableDebug
            };

            CompileReply reply = null;

            try
            {
                _endpointFactory.CallCompilerService(
                    client => {
                        reply = client.Compile(request);
                    });

                dllBytes = reply.DllBytes;
                pdbBytes = reply.PdbBytes;
                errors = reply.Errors?.ToArray();

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
