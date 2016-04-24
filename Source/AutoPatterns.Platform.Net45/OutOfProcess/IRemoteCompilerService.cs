using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.OutOfProcess.RequestReply;

namespace AutoPatterns.OutOfProcess
{
    [ServiceContract(Namespace = ContractNames.Namespace)]
    public interface IRemoteCompilerService
    {
        [OperationContract]
        void Hello();

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [OperationContract]
        CompileReply Compile(CompileRequest request);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [OperationContract]
        void GoodBye();
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static class ContractNames
    {
        public const string Namespace = "AutoPatterns.RemoteCompiler";
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    namespace RequestReply
    {
        [DataContract(Namespace = ContractNames.Namespace)]
        public class CompileRequest
        {
            [DataMember]
            public string AssemblyName { get; set; }
            [DataMember]
            public string SourceCode { get; set; }
            [DataMember]
            public string[] ReferencePaths { get; set; }
            [DataMember]
            public bool EnableDebug { get; set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [DataContract(Namespace = ContractNames.Namespace)]
        public class CompileReply
        {
            [DataMember]
            public bool Success { get; set; }
            [DataMember]
            public byte[] DllBytes { get; set; }
            [DataMember]
            public byte[] PdbBytes { get; set; }
            [DataMember]
            public IList<string> Errors { get; set; }
        }
    }
}
