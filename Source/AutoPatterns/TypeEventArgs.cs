using System;
using AutoPatterns.Impl;

namespace AutoPatterns
{
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
