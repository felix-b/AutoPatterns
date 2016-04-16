using System;

namespace AutoPatterns.Runtime
{
    public interface IRuntimeLogger
    {
        void DebugClassWritten(Type patternType, TypeKey key, string classTypeFullName);
        void ErrorClassWriteError(Type patternType, TypeKey key, Exception error);
        void InfoAssemblyCompiled(string assemblyName);
        void ErrorAssemblyCompileFailed(string assemblyName, Exception error);
    }
}
