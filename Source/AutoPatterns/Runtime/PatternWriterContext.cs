using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Runtime
{
    public class PatternWriterContext
    {
        internal PatternWriterContext(PatternWriter writer, TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            this.Pattern = writer.OwnerPattern;
            this.Library = writer.OwnerPattern.Library;
            this.Input = new InputContext(
                typeKey, 
                baseType, 
                primaryInterfaces.OrEmptyTypes(),
                secondaryInterfaces.OrEmptyTypes());

            var classWriter = new ClassWriter(this, Pattern.NamespaceName, Pattern.GetClassName(typeKey));
            this.Output = new OutputContext(classWriter);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary Library { get; private set; }
        public AutoPattern Pattern { get; private set; }
        public InputContext Input { get; private set; }
        public OutputContext Output { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InputContext
        {
            public InputContext(TypeKey typeKey, Type baseType, IReadOnlyList<Type> primaryInterfaces, IReadOnlyList<Type> secondaryInterfaces)
            {
                TypeKey = typeKey;
                BaseType = baseType;
                PrimaryInterfaces = primaryInterfaces;
                SecondaryInterfaces = secondaryInterfaces;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeKey TypeKey { get; private set; }
            public Type BaseType { get; private set; }
            public IReadOnlyList<Type> PrimaryInterfaces { get; private set; }
            public IReadOnlyList<Type> SecondaryInterfaces { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OutputContext
        {
            public OutputContext(ClassWriter classWriter)
            {
                this.ClassWriter = classWriter;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public ClassWriter ClassWriter { get; private set; }
        }
    }
}
