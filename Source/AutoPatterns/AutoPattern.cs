using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoPatterns.Abstractions;
using AutoPatterns.Impl;

namespace AutoPatterns
{
    public abstract class AutoPattern
    {
        private readonly Compiler _compiler;
        private Factory  _compiler;

        protected abstract IAutoPatternTemplate[] BuildPipeline(MetaCompilerContext context);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Compiler CreateCompiler()
        {
            return new Compiler(this);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected virtual Factory CreateFactory(IEnumerable<Assembly> assemblies)
        {
            return new Factory(assemblies);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected class Compiler : AutoPatternCompiler
        {
            private readonly AutoPattern _ownerPattern;

            public Compiler(AutoPattern ownerPattern)
            {
                _ownerPattern = ownerPattern;
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            #region Overrides of AutoPatternCompiler

            protected override IAutoPatternTemplate[] BuildPipeline(MetaCompilerContext context)
            {
                return _ownerPattern.BuildPipeline(context);
            }

            #endregion
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected class Factory : AutoPatternFactory
        {
            public Factory(IEnumerable<Assembly> assemblies)
                : base(assemblies)
            {
            }

            //-------------------------------------------------------------------------------------------------------------------------------------------------

            public new object CreateInstance(TypeKey key, int constructorIndex)
            {
                return base.CreateInstance(key, constructorIndex);
            }
            public new object CreateInstance<T1>(TypeKey key, int constructorIndex, T1 arg1)
            {
                return base.CreateInstance<T1>(key, constructorIndex, arg1);
            }
            public new object CreateInstance<T1, T2>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2)
            {
                return base.CreateInstance<T1, T2>(key, constructorIndex, arg1, arg2);
            }
            public new object CreateInstance<T1, T2, T3>(TypeKey key, int constructorIndex, T1 arg1, T2 arg2, T3 arg3)
            {
                return base.CreateInstance<T1, T2, T3>(key, constructorIndex, arg1, arg2, arg3);
            }
        }
    }
}
