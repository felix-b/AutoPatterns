using System.Runtime.Serialization;

namespace MetaPatterns.Tests.Examples
{
    [MetaProgram.Annotation.Template]
    [DataContract(Name = MetaProgram.Constant.String1, Namespace = MetaProgram.Constant.String2)]
    public class ExampleDataContract
    {
        [MetaProgram.Annotation.MetaMember]
        [DataMember]
        public object AProperty { get; set; }
    }
}
