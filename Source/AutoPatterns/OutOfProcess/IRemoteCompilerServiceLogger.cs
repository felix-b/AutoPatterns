using System;

namespace AutoPatterns.OutOfProcess
{
    public interface IRemoteCompilerServiceLogger
    { 
        void InfoRemoteCompilerService(Version version);
        void ErrorRemoteCompilerServiceAnotherInstanceRunning();
        void InfoRemoteCompilerServiceStarting();
        void InfoRemoteCompilerServiceUpAndRunning();
        void InfoRemoteCompileRequestCompleted(string assemblyName, bool success, int errorWarningCount, int millisecondsDuration);
        void ErrorRemoteCompileRequestFailed(Exception error);
        void InfoRemoteCompilerServiceStopping();
        void ErrorRemoteCompilerServiceStoppingTimedOut();
        void InfoRemoteCompilerServiceStopped();
        void ErrorRemoteCompilerServiceAbnormallyTerminated(Exception error);
    }
}