using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoPatterns.Runtime
{
    public interface IAutoPatternsPlatform
    {
        void ConsoleWriteLine(string text);
        void DirectoryCreateDirectory(string path);
        void FileWriteAllBytes(string filePath, byte[] contents);
        void FileWriteAllText(string filePath, string contents);
        string AssemblyLocation(Assembly assembly);
        Assembly AssemblyLoadFrom(string filePath);
        Assembly AssemblyLoad(byte[] rawBytes);
        IPatternCompiler CreateRemoteCompiler(int tcpPortNumber);
    }
}
