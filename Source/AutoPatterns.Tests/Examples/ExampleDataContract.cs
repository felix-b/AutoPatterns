using System.Runtime.Serialization;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    [DataContract(Name = MetaProgram.Constant.String1, Namespace = MetaProgram.Constant.String2)]
    public class ExampleDataContract
    {
        [MetaProgram.Annotation.MetaMember]
        [DataMember]
        public object AProperty { get; set; }
    }
}
