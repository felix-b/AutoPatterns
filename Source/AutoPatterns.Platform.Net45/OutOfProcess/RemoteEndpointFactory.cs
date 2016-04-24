using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoPatterns.OutOfProcess
{
    public abstract class RemoteEndpointFactory
    {
        public abstract IServiceHost CreateServiceHost(RemoteCompilerService service);
        public abstract IRemoteCompilerService CreateClient();
        public abstract void CallCompilerService(Action<IRemoteCompilerService> doCall);

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

        private void StartCompilerHostProcess()
        {
            var directory = Path.GetDirectoryName(this.GetType().Assembly.Location);
            var compilerHostExeFilePath = Path.Combine(directory, "AutoPatterns.CompilerHost.exe");
            ProcessStartInfo info = new ProcessStartInfo(compilerHostExeFilePath);
            info.UseShellExecute = true; // child process will use its own console window

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

        public static string CompilerHostMutexName { get; } = @"Global\8FDE12BC-3DEC-48AA-913F-3D392883356F";
    }
}
