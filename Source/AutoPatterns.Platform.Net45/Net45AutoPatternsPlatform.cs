using System;
using System.IO;
using System.Reflection;
using AutoPatterns.OutOfProcess;
using AutoPatterns.Runtime;

namespace AutoPatterns
{
    public class Net45AutoPatternsPlatform : IAutoPatternsPlatform
    {
        void IAutoPatternsPlatform.ConsoleWriteLine(string text)
        {
            Console.WriteLine(text);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Assembly IAutoPatternsPlatform.AssemblyLoad(byte[] rawBytes)
        {
            return Assembly.Load(rawBytes);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        Assembly IAutoPatternsPlatform.AssemblyLoadFrom(string filePath)
        {
            return Assembly.LoadFrom(filePath);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        string IAutoPatternsPlatform.AssemblyLocation(Assembly assembly)
        {
            return assembly.Location;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        IPatternCompiler IAutoPatternsPlatform.CreateRemoteCompiler(int tcpPortNumber)
        {
            return new RemotePatternCompiler(tcpPortNumber);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAutoPatternsPlatform.DirectoryCreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAutoPatternsPlatform.FileWriteAllBytes(string filePath, byte[] contents)
        {
            File.WriteAllBytes(filePath, contents);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        void IAutoPatternsPlatform.FileWriteAllText(string filePath, string contents)
        {
            File.WriteAllText(filePath, contents);
        }
    }
}
