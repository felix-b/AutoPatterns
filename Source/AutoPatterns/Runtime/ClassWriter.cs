using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Runtime
{
    public class ClassWriter
    {
        private readonly PatternWriterContext _context;
        private readonly List<BaseTypeSyntax> _baseTypes = new List<BaseTypeSyntax>();
        private readonly List<FieldMember> _fields = new List<FieldMember>();
        private readonly List<ConstructorMember> _constructors = new List<ConstructorMember>();
        private readonly List<MethodMember> _methods = new List<MethodMember>();
        private readonly List<PropertyMember> _properties = new List<PropertyMember>();
        private readonly List<IndexeryMember> _indexers = new List<IndexeryMember>();
        private readonly List<EventMember> _events = new List<EventMember>();
        private readonly Dictionary<MemberInfo, IMember> _memberByDeclaration = new Dictionary<MemberInfo, IMember>();

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        internal ClassWriter(PatternWriterContext context, string namespaceName, string className)
        {
            _context = context;

            this.NamespaceName = namespaceName;
            this.ClassName = className;

            WriteBaseTypes();
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        //public void AddField(string name, Type filedType, FieldInfo declaration)
        //{
        //    //_baseTypes.Add(SimpleBaseType(SyntaxHelper.GetTypeSyntax()));
        //}

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public BaseTypeSyntax AddBaseType(Type type)
        {
            _context.Library.EnsureMetadataReference(type);

            var syntax = SimpleBaseType(SyntaxHelper.GetTypeSyntax(type));
            _baseTypes.Add(syntax);

            return syntax;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMember AddPublicProperty(string name, Type propertyType, PropertyInfo declaration = null)
        {
            var syntax = PropertyDeclaration(
                SyntaxHelper.GetTypeSyntax(propertyType), 
                Identifier(name)
            )
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword)
            ));

            var member = new PropertyMember(name, syntax, declaration);
            _properties.Add(member);
            RegisterDeclaration(member);
            return member;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public MethodMember AddPublicVoidMethod(string name, MethodInfo declaration = null)
        {
            var syntax = MethodDeclaration(
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                Identifier(name)
            )
            .WithModifiers(TokenList(
                Token(SyntaxKind.PublicKeyword)
            ));

            var member = new MethodMember(name, syntax, declaration);
            _methods.Add(member);
            RegisterDeclaration(member);
            return member;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public string NamespaceName { get; private set; }
        public string ClassName { get; private set; }
        public IReadOnlyList<BaseTypeSyntax> BaseTypes => _baseTypes;
        public IReadOnlyList<FieldMember> Fields => _fields;
        public IReadOnlyList<ConstructorMember> Constructors => _constructors;
        public IReadOnlyList<MethodMember> Methods => _methods;
        public IReadOnlyList<PropertyMember> Properties => _properties;
        public IReadOnlyList<IndexeryMember> Indexers => _indexers;
        public IReadOnlyList<EventMember> Events => _events;
        public IReadOnlyDictionary<MemberInfo, IMember> MemberByDeclaration => _memberByDeclaration;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public TMember TryGetMember<TMember>(MemberInfo declaration)
            where TMember : class, IMember
        {
            IMember member;

            if (_memberByDeclaration.TryGetValue(declaration, out member))
            {
                return (member as TMember);
            }

            return null;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public MemberDeclarationSyntax GetCompleteSyntax()
        {
            WriteFactoryMethods();

            return
                NamespaceDeclaration(IdentifierName(this.NamespaceName))
                .WithUsings(
                    SingletonList<UsingDirectiveSyntax>(UsingDirective(IdentifierName("System")))
                )
                .WithMembers(
                    List<MemberDeclarationSyntax>(
                        new MemberDeclarationSyntax[] {
                            ClassDeclaration(this.ClassName)
                                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                                .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(this.BaseTypes)))
                                .WithMembers(List<MemberDeclarationSyntax>(
                                    this.ConcatAllMemberSyntaxes()
                                ))
                        }
                    )
                );
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax[] ConcatAllMemberSyntaxes()
        {
            return
                Fields.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>()
                .Concat(Constructors.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>()
                .Concat(Methods.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>()
                .Concat(Properties.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>()
                .Concat(Indexers.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>()
                .Concat(Events.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>())))))
                .ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteBaseTypes()
        {
            _context.Library.EnsureMetadataReference(typeof(object));

            if (_context.Input.BaseType != null)
            {
                AddBaseType(_context.Input.BaseType);
            }

            foreach (var interfaceType in _context.Input.PrimaryInterfaces)
            {
                AddBaseType(interfaceType);
            }

            foreach (var interfaceType in _context.Input.SecondaryInterfaces)
            {
                AddBaseType(interfaceType);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteFactoryMethods()
        {
            var constructorList = _constructors;

            if (constructorList.Count > 0)
            {
                for (int index = 0; index < constructorList.Count; index++)
                {
                    WriteFactoryMethod(constructorList[index].Syntax, index);
                }
            }
            else
            {
                WriteDefaultFactoryMethod();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteFactoryMethod(ConstructorDeclarationSyntax constructor, int index)
        {
            var methodName = $"FactoryMethod__{index}";
            var syntax = MethodDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)), Identifier(methodName))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                .WithParameterList(constructor.ParameterList)
                .WithBody(Block(SingletonList<StatementSyntax>(
                    ReturnStatement(
                        ObjectCreationExpression(IdentifierName(this.ClassName))
                            .WithArgumentList(SyntaxHelper.CopyParametersToArguments(constructor.ParameterList)))
                 )));

            _methods.Add(new MethodMember(methodName, syntax, declaration: null));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private void WriteDefaultFactoryMethod()
        {
            var methodName = "FactoryMethod__0";
            var syntax = MethodDeclaration(PredefinedType(Token(SyntaxKind.ObjectKeyword)), Identifier(methodName))
                .WithModifiers(TokenList(new[] { Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword) }))
                .WithBody(Block(SingletonList<StatementSyntax>(
                    ReturnStatement(ObjectCreationExpression(IdentifierName(this.ClassName)).WithArgumentList(ArgumentList())))
                ));

            _methods.Add(new MethodMember(methodName, syntax, declaration: null));
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private void RegisterDeclaration(IMember member)
        {
            if (member.Declaration != null)
            {
                _memberByDeclaration[member.Declaration] = member;
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public interface IMember
        {
            string Name { get; }
            MemberDeclarationSyntax Syntax { get; }
            MemberInfo Declaration { get; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract class Member<TSyntax, TDeclaration> : IMember
            where TSyntax : MemberDeclarationSyntax
            where TDeclaration : MemberInfo
        {
            protected Member(string name, TSyntax syntax, TDeclaration declaration)
            {
                this.Name = name;
                this.Syntax = syntax;
                this.Declaration = declaration;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            MemberDeclarationSyntax IMember.Syntax => this.Syntax;
            MemberInfo IMember.Declaration => this.Declaration;

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public string Name { get; private set; }
            public TSyntax Syntax { get; set; }
            public TDeclaration Declaration { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class FieldMember : Member<FieldDeclarationSyntax, FieldInfo>
        {
            public FieldMember(string name, FieldDeclarationSyntax syntax, FieldInfo declaration)
                : base(name, syntax, declaration)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class ConstructorMember : Member<ConstructorDeclarationSyntax, ConstructorInfo>
        {
            public ConstructorMember(string name, ConstructorDeclarationSyntax syntax, ConstructorInfo declaration)
                : base(name, syntax, declaration)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class MethodMember : Member<MethodDeclarationSyntax, MethodInfo>
        {
            public MethodMember(string name, MethodDeclarationSyntax syntax, MethodInfo declaration)
                : base(name, syntax, declaration)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PropertyMember : Member<PropertyDeclarationSyntax, PropertyInfo>
        {
            public PropertyMember(string name, PropertyDeclarationSyntax syntax, PropertyInfo declaration)
                : base(name, syntax, declaration)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class IndexeryMember : Member<IndexerDeclarationSyntax, PropertyInfo>
        {
            public IndexeryMember(string name, IndexerDeclarationSyntax syntax, PropertyInfo declaration)
                : base(name, syntax, declaration)
            {
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class EventMember : Member<EventDeclarationSyntax, EventInfo>
        {
            public EventMember(string name, EventDeclarationSyntax syntax, EventInfo declaration)
                : base(name, syntax, declaration)
            {
            }
        }
    }
}
