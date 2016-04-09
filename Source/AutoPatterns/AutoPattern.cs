using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Abstractions;
using AutoPatterns.Extensions;
using AutoPatterns.Runtime;

namespace AutoPatterns
{
    public abstract class AutoPattern
    {
        private readonly PatternLibrary _library;
        private readonly string _namespaceName;
        private readonly PatternWriter _writer;
        private readonly PatternFactory _factory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected AutoPattern(PatternLibrary library, string namespaceName = null)
        {
            _library = library;
            _namespaceName = namespaceName ?? this.GetType().Name.TrimSuffix("Pattern");
            _writer = new PatternWriter(this);
            _factory = new PatternFactory(this);

            library.AddWriter(_writer);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public virtual string GetClassName(TypeKey key)
        {
            return key.ToString();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public string NamespaceName => _namespaceName;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public PatternLibrary Library => _library;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public event EventHandler<TypeEventArgs> TypeBound;
        public event EventHandler<PipelineEventArgs> BuildingPipeline;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected abstract void BuildPipeline(PatternWriterContext context, PipelineBuilder pipeline);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PatternWriter Writer => _writer;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected PatternFactory Factory => _factory;

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal protected virtual void OnTypeBound(PatternFactory.TypeEntry typeEntry)
        {
            TypeBound?.Invoke(this, new TypeEventArgs(typeEntry.Key, typeEntry.Type));
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        internal IPatternTemplate[] InternalBuildPipeline(PatternWriterContext context)
        {
            var pipeline = new PipelineBuilder();

            BuildPipeline(context, pipeline);
            BuildingPipeline?.Invoke(this, new PipelineEventArgs(pipeline));

            return pipeline.ToArray();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PipelineBuilder
        {
            private readonly LinkedList<IPatternTemplate> _sinks = new LinkedList<IPatternTemplate>();

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            internal PipelineBuilder()
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertFirst(params IPatternTemplate[] templates)
            {
                for (int i = templates.Length - 1; i >= 0; i--)
                {
                    _sinks.AddFirst(templates[i]);
                }
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertLast(params IPatternTemplate[] templates)
            {
                for (int i = 0 ; i < templates.Length ; i++)
                {
                    _sinks.AddLast(templates[i]);
                }
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertBefore<TTemplate>(params IPatternTemplate[] templates)
                where TTemplate : IPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void InsertAfter<TTemplate>(params IPatternTemplate[] templates)
                where TTemplate : IPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void Remove<TTemplate>() 
                where TTemplate : IPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public void Replace<TTemplate>(params IPatternTemplate[] replacingTemplates)
                where TTemplate : IPatternTemplate
            {
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public bool HasTemplate<TTemplate>()
                where TTemplate : IPatternTemplate
            {
                return false;
            }

            //------------------------------------------------------------------------------------------------------------------------------------------------- 

            public IPatternTemplate[] ToArray()
            {
                return _sinks.ToArray();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public class PipelineEventArgs : EventArgs
        {
            public PipelineEventArgs(PipelineBuilder pipeline)
            {
                Pipeline = pipeline;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public PipelineBuilder Pipeline { get; private set; }
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
