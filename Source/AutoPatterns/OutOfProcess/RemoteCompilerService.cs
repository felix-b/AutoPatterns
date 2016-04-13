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
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.Single)]
    public class RemoteCompilerService : IRemoteCompilerService
    {
        private readonly InProcessPatternCompiler _compiler = new InProcessPatternCompiler();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        #region Implementation of IRemoteCompilerService

        public void Hello()
        {
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public CompileReply Compile(CompileRequest request)
        {
            Console.WriteLine($"RCS > Processing COMPILE REQUEST, assemblyName={request.AssemblyName}");

            byte[] dllBytes;
            byte[] pdbBytes;
            string[] errors;

            try
            {
                var clock = Stopwatch.StartNew();

                var success = _compiler.CompileAssembly(
                    request.AssemblyName,
                    request.SourceCode,
                    request.ReferencePaths,
                    request.EnableDebug,
                    out dllBytes,
                    out pdbBytes,
                    out errors);

                var statusText = (success ? "SUCCESS" : "FAILURE");
                var errorsWarnings = (errors?.Length).GetValueOrDefault();

                Console.WriteLine($"RCS > done {request.AssemblyName}: {statusText}, {errorsWarnings} errors/warnings, {clock.ElapsedMilliseconds} ms.");

                return new CompileReply() {
                    DllBytes = dllBytes,
                    PdbBytes = pdbBytes,
                    Errors = errors,
                    Success = success
                };
            }
            catch (Exception e)
            {
                Console.WriteLine("RCS > FAILED WITH EXCEPTION! {0}: {1}", e.GetType().Name, e.Message);
                throw;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void GoodBye()
        {
            ShutdownRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler ShutdownRequested;
    }
}
