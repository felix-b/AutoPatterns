using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using AutoPatterns.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoPatterns.Runtime
{
    public sealed class PatternWriter
    {
        public const string FactoryMethodNamePrefix = "FactoryMethod__";

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private readonly AutoPattern _ownerPattern;
        private readonly ConcurrentDictionary<TypeKey, MemberDeclarationSyntax> _writtenMembers;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternWriter(AutoPattern ownerPattern)
        {
            _ownerPattern = ownerPattern;
            _writtenMembers = new ConcurrentDictionary<TypeKey, MemberDeclarationSyntax>();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureWritten(TypeKey typeKey, Type baseType = null, Type primaryInterface = null)
        {
            EnsureWritten(
                typeKey, 
                baseType, 
                primaryInterfaces: primaryInterface != null ? new[] { primaryInterface } : null, 
                secondaryInterfaces: null);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public void EnsureWritten(TypeKey typeKey, Type baseType, Type[] primaryInterfaces, Type[] secondaryInterfaces)
        {
            _writtenMembers.GetOrAdd(
                typeKey, 
                k => WriteClass(typeKey, baseType, primaryInterfaces, secondaryInterfaces));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public AutoPattern OwnerPattern => _ownerPattern;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal MemberDeclarationSyntax[] TakeMembersWrittenSoFar()
        {
            var members = _writtenMembers.Values.ToArray();
            _writtenMembers.Clear();
            return members;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private MemberDeclarationSyntax WriteClass(
            TypeKey typeKey, 
            Type baseType, 
            Type[] primaryInterfaces, 
            Type[] secondaryInterfaces)
        {
            var context = new PatternWriterContext(this, typeKey, baseType, primaryInterfaces, secondaryInterfaces);
            var pipeline = _ownerPattern.InternalBuildPipeline(context);

            for (int i = 0; i < pipeline.Length; i++)
            {
                pipeline[i].Apply(context);
            }

            var syntax = context.Output.ClassWriter.GetCompleteSyntax();
            return syntax;
        }
    }
}
