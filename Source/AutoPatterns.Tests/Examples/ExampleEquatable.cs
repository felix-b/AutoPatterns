using System;
using System.Reflection;
using TT = AutoPatterns.MetaProgram.TypeRef;

namespace AutoPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    public class ExampleEquatable : TT.TPrimaryContract, IEquatable<TT.TPrimaryContract>
    {
        [MetaProgram.Annotation.DeclaredMember]
        public override bool Equals(object obj)
        {
            TT.TPrimaryContract typedObj = obj as TT.TPrimaryContract;

            if (typedObj != null)
            {
                return Equals(typedObj);
            }

            return base.Equals(obj);
        }
        

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MetaProgram.Annotation.DeclaredMember]
        public override int GetHashCode()
        {
            var hashCode = 0;

            using (MetaProgram.TemplateLogic)
            {
                foreach (var field in MetaProgram.ThisObject.Fields)
                {
                    if (field.Info.FieldType.GetTypeInfo().IsValueType)
                    {
                        using (MetaProgram.TemplateOutput)
                        {
                            hashCode ^= field.Value.GetHashCode();
                        }
                    }
                    else
                    {
                        using (MetaProgram.TemplateOutput)
                        {
                            if (field.Value != null)
                            {
                                hashCode ^= field.Value.GetHashCode();
                            }
                        }
                    }
                }
            }

            return hashCode;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        [MetaProgram.Annotation.DeclaredMember]
        public bool Equals(TT.TPrimaryContract other)
        {
            using (MetaProgram.TemplateLogic)
            {
                foreach (var field in MetaProgram.ThisObject.Fields)
                {
                    using (MetaProgram.TemplateOutput)
                    {
                        if (field.Value != MetaProgram.Object(other).Field(field).Value)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
