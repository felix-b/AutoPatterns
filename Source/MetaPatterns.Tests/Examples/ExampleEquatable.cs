using System;
using TT = MetaPatterns.MetaProgram.TypeParam;

namespace MetaPatterns.Tests.Examples
{
    [MetaProgram.Annotation.ClassTemplate]
    public class ExampleEquatable : TT.IPrimaryContract, IEquatable<TT.IPrimaryContract>
    {
        [MetaProgram.Annotation.DeclaredMember]
        public override bool Equals(object obj)
        {
            TT.IPrimaryContract typedObj = obj as TT.IPrimaryContract;

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
                    if (field.Info.FieldType.IsValueType)
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
        public bool Equals(TT.IPrimaryContract other)
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
