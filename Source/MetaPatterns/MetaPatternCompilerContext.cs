using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaPatterns.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MetaPatterns
{
    public class MetaPatternCompilerContext
    {
        internal MetaPatternCompilerContext(TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            this.Input = new InputContext(
                typeKey, 
                baseType, 
                ImmutableArray.Create<Type>(primaryInterfaces.OrEmptyTypes()),
                ImmutableArray.Create<Type>(secondaryInterfaces.OrEmptyTypes()));

            this.Output = new OutputContext();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public InputContext Input { get; private set; }
        public OutputContext Output { get; private set; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class InputContext
        {
            public InputContext(TypeKey typeKey, Type baseType, ImmutableArray<Type> primaryInterfaces, ImmutableArray<Type> secondaryInterfaces)
            {
                TypeKey = typeKey;
                BaseType = baseType;
                PrimaryInterfaces = primaryInterfaces;
                SecondaryInterfaces = secondaryInterfaces;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeKey TypeKey { get; private set; }
            public Type BaseType { get; private set; }
            public ImmutableArray<Type> PrimaryInterfaces { get; private set; }
            public ImmutableArray<Type> SecondaryInterfaces { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class OutputContext
        {
            internal OutputContext()
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public List<BaseTypeSyntax> BaseTypes { get; } = new List<BaseTypeSyntax>();
            public List<FieldDeclarationSyntax> Fields { get; } = new List<FieldDeclarationSyntax>();
            public List<ConstructorDeclarationSyntax> Constructors { get; } = new List<ConstructorDeclarationSyntax>();
            public List<MethodDeclarationSyntax> Methods { get; } = new List<MethodDeclarationSyntax>();
            public List<PropertyDeclarationSyntax> Properties { get; } = new List<PropertyDeclarationSyntax>();
            public List<IndexerDeclarationSyntax> Indexers { get; } = new List<IndexerDeclarationSyntax>();
            public List<EventDeclarationSyntax> Events { get; } = new List<EventDeclarationSyntax>();
        }
    }
}
