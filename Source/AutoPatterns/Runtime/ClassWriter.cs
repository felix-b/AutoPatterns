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
        private readonly List<AttributeSyntax> _classAttributes = new List<AttributeSyntax>();
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

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AttributeSyntax AddClassAttribute(Type attributeType)
        {
            _context.Library.EnsureMetadataReference(attributeType);

            var syntax = Attribute(SyntaxHelper.GetTypeSyntax(attributeType));
            _classAttributes.Add(syntax);

            return syntax;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public FieldMember AddPrivateField(string name, Type fieldType)
        {
            _context.Library.EnsureMetadataReference(fieldType);

            var syntax = FieldDeclaration(
                VariableDeclaration(
                    SyntaxHelper.GetTypeSyntax(fieldType)
                )
                .WithVariables(
                    SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(
                            Identifier(name)
                        )
                    )
                )
            )
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PrivateKeyword)
                )
            );

            var member = new FieldMember(name, syntax, declaration: null);
            _fields.Add(member);
            RegisterDeclaration(member);
            return member;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public PropertyMember AddPublicProperty(string name, Type propertyType, PropertyInfo declaration = null)
        {
            _context.Library.EnsureMetadataReference(propertyType);

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
        public IReadOnlyList<AttributeSyntax> ClassAttributes => _classAttributes;
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
                            BuildClassDeclarationSyntax()
                            //ClassDeclaration(this.ClassName)
                            //    .WithAttributeLists(SingletonList(AttributeList(SeparatedList(_classAttributes))))
                            //    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                            //    .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(this.BaseTypes)))
                            //    .WithMembers(List<MemberDeclarationSyntax>(
                            //        this.ConcatAllMemberSyntaxes()
                            //    ))
                        }
                    )
                );
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private ClassDeclarationSyntax BuildClassDeclarationSyntax()
        {
            var syntax = ClassDeclaration(this.ClassName);

            if (_classAttributes.Count > 0)
            {
                syntax = syntax.WithAttributeLists(SingletonList(AttributeList(SeparatedList(_classAttributes))));
            }

            syntax = syntax
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBaseList(BaseList(SeparatedList<BaseTypeSyntax>(this.BaseTypes)))
                .WithMembers(List<MemberDeclarationSyntax>(
                    this.ConcatAllCompleteMemberSyntaxes()
                ));

            return syntax;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax[] ConcatAllCompleteMemberSyntaxes()
        {
            return
                Fields.Select(x => x.Syntax).Cast<MemberDeclarationSyntax>()
                .Concat(Constructors.Select(x => x.GetCompleteSyntax()).Cast<MemberDeclarationSyntax>()
                .Concat(Methods.Select(x => x.GetCompleteSyntax()).Cast<MemberDeclarationSyntax>()
                .Concat(Properties.Select(x => x.GetCompleteSyntax()).Cast<MemberDeclarationSyntax>()
                .Concat(Indexers.Select(x => x.GetCompleteSyntax()).Cast<MemberDeclarationSyntax>()
                .Concat(Events.Select(x => x.GetCompleteSyntax()).Cast<MemberDeclarationSyntax>())))))
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
            MemberDeclarationSyntax GetCompleteSyntax();
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

            public virtual MemberDeclarationSyntax GetCompleteSyntax()
            {
                return this.Syntax;
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
                CreateAccessors(declaration.CanRead, declaration.CanWrite);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PropertyMember(string name, PropertyDeclarationSyntax syntax, bool canRead, bool canWrite)
                : base(name, syntax, declaration: null)
            {
                CreateAccessors(canRead, canWrite);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public override MemberDeclarationSyntax GetCompleteSyntax()
            {
                AccessorListSyntax accessorList;

                if (Getter != null && Setter != null)
                {
                    accessorList = AccessorList(List(new[] { Getter, Setter }));
                }
                else if (Getter != null)
                {
                    accessorList = AccessorList(List(new[] { Getter }));
                }
                else
                {
                    accessorList = AccessorList(List(new[] { Setter }));
                }

                return base.Syntax.WithAccessorList(accessorList);
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public AccessorDeclarationSyntax Getter { get; set; }
            public AccessorDeclarationSyntax Setter { get; set; }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            private void CreateAccessors(bool canRead, bool canWrite)
            {
                if (canRead)
                {
                    this.Getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration);
                }

                if (canWrite)
                {
                    this.Setter = AccessorDeclaration(SyntaxKind.SetAccessorDeclaration);
                }
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
