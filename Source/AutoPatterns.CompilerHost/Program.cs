using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess;
using AutoPatterns.OutOfProcess.RequestReply;

namespace AutoPatterns.CompilerHost
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("AutoPatterns Compiler Service >> STARTING.");

            bool newMutexCreated;
            var mutex = new Mutex(true, RemoteEndpointFactory.CompilerHostMutexName, out newMutexCreated);

            if (!newMutexCreated)
            {
                Console.WriteLine("AutoPatterns Compiler Service >> ANOTHER RUNNING INSTANCE DETECTED. EXITING.");
                return 1;
            }

            try
            {
                RunCompilerHost(args);
                Console.WriteLine("AutoPatterns Compiler Service >> GOODBYE.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"AutoPatterns Compiler Service >> ABNORMALLY TERMINATED! {e.GetType().Name}: {GetTerminationExceptionMessage(e)}");
                return 2;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void RunCompilerHost(string[] args)
        {
            var shutdownEvent = new ManualResetEvent(initialState: false);
            var service = new RemoteCompilerService();

            if (!args.Any(arg => StringComparer.InvariantCultureIgnoreCase.Equals(arg, "NOWRAMUP")))
            {
                Console.WriteLine("AutoPatterns Compiler Service >> WARMING UP . . .");
                Task.Run(() => WarmUp(service));
            }
            else
            {
                Console.WriteLine("AutoPatterns Compiler Service >> WARM-UP suppressed through command line.");
            }

            service.ShutdownRequested += (sender, e) => shutdownEvent.Set();

            var endpointFactory = new TcpRemoteEndpointFactory(tcpPortNumber: 50555);
            var serviceHost = endpointFactory.CreateServiceHost(service);

            serviceHost.Open();

            Console.CancelKeyPress += (sender, e) => {
                shutdownEvent.Set();
                e.Cancel = true;
            };

            Console.WriteLine("AutoPatterns Compiler Service >> RUNNING. PRESS CTRL+C TO SHUT DOWN.");

            shutdownEvent.WaitOne();

            Console.WriteLine("AutoPatterns Compiler Service >> STOPPING.");

            serviceHost.Close(TimeSpan.FromSeconds(10));

            Console.WriteLine("AutoPatterns Compiler Service >> STOPPED.");
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void WarmUp(RemoteCompilerService service)
        {
            var request = new CompileRequest() {
                AssemblyName = "WarmUp_0",
                SourceCode = "using System;namespace WarmUp{[Serializable] public class MyClass{public void Print(){Console.WriteLine(\"Warming up\");}}}",
                ReferencePaths = new[] { typeof(object).Assembly.Location },
                EnableDebug = false
            };

            service.Compile(request);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static string GetTerminationExceptionMessage(Exception e)
        {
            var aggregate = e as AggregateException;

            if (aggregate != null)
            {
                return string.Join("; ", aggregate.Flatten().InnerExceptions.Select(x => x.Message));
            }
            else
            {
                return e.Message;
            }
        }
    }
}
