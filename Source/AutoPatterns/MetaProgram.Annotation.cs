using System;

namespace AutoPatterns
{
    public static partial class MetaProgram
    {
        public static class Annotation
        {
            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
            public class ClassTemplateAttribute : Attribute
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Event |
                AttributeTargets.Field,
                AllowMultiple = false,
                Inherited = true)]
            public class MetaMemberAttribute : Attribute
            {
                public MetaMemberAttribute()
                {
                    this.Repeat = RepeatOption.PerMatch;
                    this.Select = SelectOptions.All;
                    this.Implement = ImplementOptions.All;
                    this.Aspect = AspectOption.None;
                }

                public RepeatOption Repeat { get; set; }
                public SelectOptions Select { get; set; }
                public ImplementOptions Implement { get; set; }
                public AspectOption Aspect { get; set; }
                public Type CatchExceptionType { get; set; }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Event |
                AttributeTargets.Field,
                AllowMultiple = false,
                Inherited = true)]
            public class IncludeWithAttribute : Attribute
            {
                public IncludeWithAttribute(string metaMemberName)
                {
                    MetaMemberName = metaMemberName;
                }

                public string MetaMemberName { get; private set; }
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Event,
                AllowMultiple = false,
                Inherited = true)]
            public class DeclaredMemberAttribute : Attribute
            {
            }

            //-----------------------------------------------------------------------------------------------------------------------------------------------------

            [AttributeUsage(
                AttributeTargets.Constructor |
                AttributeTargets.Method |
                AttributeTargets.Property |
                AttributeTargets.Event |
                AttributeTargets.Field,
                AllowMultiple = false,
                Inherited = true)]
            public class NewMemberAttribute : Attribute
            {
            }
        }
    }
}
