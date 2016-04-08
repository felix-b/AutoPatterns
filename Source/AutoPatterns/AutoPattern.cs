using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using AutoPatterns.Impl;

namespace AutoPatterns
{
    public abstract class AutoPattern
    {
        private readonly string _namespaceName;
        private readonly AutoPatternCompiler _compiler;
        private readonly AutoPatternFactory _factory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AutoPattern(AutoPatternLibrary library, string namespaceName = null)
        {
            _namespaceName = namespaceName ?? this.GetType().Name.TrimSuffix("Pattern");
            _compiler = new AutoPatternCompiler(library, namespaceName, PrivateBuildPipeline);
            _factory = new AutoPatternFactory(library, namespaceName, GetClassName, OnTypeBound);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string GetClassName(TypeKey key)
        {
            return key.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NamespaceName => _namespaceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<TypeEventArgs> TypeBound;
        public event EventHandler<PipelineEventArgs> BuildingPipeline;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual void OnTypeBound(TypeKey key, Type type)
        {
            TypeBound?.Invoke(this, new TypeEventArgs(key, type));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract IAutoPatternTemplate[] BuildPipeline(MetaCompilerContext context, Pipeline pipeline);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AutoPatternCompiler Compiler => _compiler;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AutoPatternFactory Factory => _factory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        private IAutoPatternTemplate[] PrivateBuildPipeline(MetaCompilerContext context)
        {
            var pipeline = new Pipeline();

            BuildPipeline(context, pipeline);
            BuildingPipeline?.Invoke(this, new PipelineEventArgs(pipeline));

            return pipeline.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class Pipeline
        {
            private readonly List<IAutoPatternTemplate> _sinks = new List<IAutoPatternTemplate>();

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertFirst(params IAutoPatternTemplate[] templates)
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertLast(params IAutoPatternTemplate[] templates)
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertBefore<TTemplate>(params IAutoPatternTemplate[] templates)
                where TTemplate : IAutoPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertAfter<TTemplate>(params IAutoPatternTemplate[] templates)
                where TTemplate : IAutoPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void Remove<TTemplate>() 
                where TTemplate : IAutoPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void Replace<TTemplate>(params IAutoPatternTemplate[] replacingTemplates)
                where TTemplate : IAutoPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public bool HasTemplate<TTemplate>()
                where TTemplate : IAutoPatternTemplate
            {
                return false;
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public IAutoPatternTemplate[] ToArray()
            {
                return _sinks.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PipelineEventArgs : EventArgs
        {
            public PipelineEventArgs(Pipeline pipeline)
            {
                Pipeline = pipeline;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public Pipeline Pipeline { get; private set; }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class TypeEventArgs : EventArgs
        {
            public TypeEventArgs(TypeKey typeKey, Type type)
            {
                TypeKey = typeKey;
                Type = type;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public TypeKey TypeKey { get; private set; }
            public Type Type { get; private set; }
        }
    }
}
