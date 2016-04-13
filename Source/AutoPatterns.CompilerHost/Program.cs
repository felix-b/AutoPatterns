using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess;

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
                RunCompilerHost();
                Console.WriteLine("AutoPatterns Compiler Service >> GOODBYE.");
                return 0;
            }
            catch (Exception e)
            {
                string message;
                var aggregate = e as AggregateException;
                if (aggregate != null)
                {
                    message = string.Join("; ", aggregate.Flatten().InnerExceptions.Select(x => x.Message));
                }
                else
                {
                    message = e.Message;
                }
                Console.WriteLine($"AutoPatterns Compiler Service >> ABNORMALLY TERMINATED! {e.GetType().Name}: {message}");
                return 2;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private static void RunCompilerHost()
        {
            var shutdownEvent = new ManualResetEvent(initialState: false);
            var service = new RemoteCompilerService();

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
    }
}
