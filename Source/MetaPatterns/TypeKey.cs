using System;
using System.Reflection;

namespace MetaPatterns
{
    #pragma warning disable 659 // abstract TypeKey overrides Equals, but GetHashCode is overridden by TypeKey's descendands, which produces warning 659.

    public abstract class TypeKey : IEquatable<TypeKey>
    {
        public override bool Equals(object obj)
        {
            var other = obj as TypeKey;

            if (other != null)
            {
                return Equals(other);
            }

            return base.Equals(obj);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract bool Equals(TypeKey other);

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract int Count { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        public abstract object this[int index] { get; }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------

        protected bool FieldsEqual<T>(T field1, T field2, bool isValueType)
        {
            if (!isValueType && ReferenceEquals(field1, null))
            {
                return ReferenceEquals(field2, null);
            }

            return field1.Equals(field2);
        }
    }

    #pragma warning restore 659

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeKey<T0> : TypeKey
    {
        private readonly T0 _value0;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(T0 value0)
        {
            _value0 = value0;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TypeKey

        public override bool Equals(TypeKey other)
        {
            var typedOther = other as TypeKey<T0>;

            if (typedOther != null)
            {
                if (!FieldsEqual(_value0, typedOther._value0, _s_isValueType0))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Count => 1;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _value0;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override int GetHashCode()
        {
            var hashCode = 0;

            if (_s_isValueType0 || !ReferenceEquals(_value0, null))
            {
                hashCode ^= _value0.GetHashCode();
            }

            return hashCode;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{_value0}";
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly bool _s_isValueType0 = typeof(T0).GetTypeInfo().IsValueType;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeKey<T0, T1> : TypeKey
    {
        private readonly T0 _value0;
        private readonly T1 _value1;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(T0 value0, T1 value1)
        {
            _value0 = value0;
            _value1 = value1;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TypeKey

        public override bool Equals(TypeKey other)
        {
            var typedOther = other as TypeKey<T0, T1>;

            if (typedOther != null)
            {
                if (!FieldsEqual(_value0, typedOther._value0, _s_isValueType0))
                {
                    return false;
                }
                if (!FieldsEqual(_value1, typedOther._value1, _s_isValueType1))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Count => 2;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _value0;
                    case 1: return _value1;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override int GetHashCode()
        {
            var hashCode = 0;

            if (_s_isValueType0 || !ReferenceEquals(_value0, null))
            {
                hashCode ^= _value0.GetHashCode();
            }
            if (_s_isValueType1 || !ReferenceEquals(_value1, null))
            {
                hashCode ^= _value1.GetHashCode();
            }

            return hashCode;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{_value0}_{_value1}";
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly bool _s_isValueType0 = typeof(T0).GetTypeInfo().IsValueType;
        private static readonly bool _s_isValueType1 = typeof(T1).GetTypeInfo().IsValueType;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeKey<T0, T1, T2> : TypeKey
    {
        private readonly T0 _value0;
        private readonly T1 _value1;
        private readonly T2 _value2;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(T0 value0, T1 value1, T2 value2)
        {
            _value0 = value0;
            _value1 = value1;
            _value2 = value2;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TypeKey

        public override bool Equals(TypeKey other)
        {
            var typedOther = other as TypeKey<T0, T1, T2>;

            if (typedOther != null)
            {
                if (!FieldsEqual(_value0, typedOther._value0, _s_isValueType0))
                {
                    return false;
                }
                if (!FieldsEqual(_value1, typedOther._value1, _s_isValueType1))
                {
                    return false;
                }
                if (!FieldsEqual(_value2, typedOther._value2, _s_isValueType2))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Count => 3;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _value0;
                    case 1: return _value1;
                    case 2: return _value2;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override int GetHashCode()
        {
            var hashCode = 0;

            if (_s_isValueType0 || !ReferenceEquals(_value0, null))
            {
                hashCode ^= _value0.GetHashCode();
            }
            if (_s_isValueType1 || !ReferenceEquals(_value1, null))
            {
                hashCode ^= _value1.GetHashCode();
            }
            if (_s_isValueType2 || !ReferenceEquals(_value2, null))
            {
                hashCode ^= _value2.GetHashCode();
            }

            return hashCode;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{_value0}_{_value1}_{_value2}";
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly bool _s_isValueType0 = typeof(T0).GetTypeInfo().IsValueType;
        private static readonly bool _s_isValueType1 = typeof(T1).GetTypeInfo().IsValueType;
        private static readonly bool _s_isValueType2 = typeof(T2).GetTypeInfo().IsValueType;
    }

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public class TypeKey<T0, T1, T2, T3> : TypeKey
    {
        private readonly T0 _value0;
        private readonly T1 _value1;
        private readonly T2 _value2;
        private readonly T3 _value3;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public TypeKey(T0 value0, T1 value1, T2 value2, T3 value3)
        {
            _value0 = value0;
            _value1 = value1;
            _value2 = value2;
            _value3 = value3;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of TypeKey

        public override bool Equals(TypeKey other)
        {
            var typedOther = other as TypeKey<T0, T1, T2, T3>;

            if (typedOther != null)
            {
                if (!FieldsEqual(_value0, typedOther._value0, _s_isValueType0))
                {
                    return false;
                }
                if (!FieldsEqual(_value1, typedOther._value1, _s_isValueType1))
                {
                    return false;
                }
                if (!FieldsEqual(_value2, typedOther._value2, _s_isValueType2))
                {
                    return false;
                }
                if (!FieldsEqual(_value3, typedOther._value3, _s_isValueType3))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override int Count => 4;

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override object this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _value0;
                    case 1: return _value1;
                    case 2: return _value2;
                    case 3: return _value3;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        #region Overrides of Object

        public override int GetHashCode()
        {
            var hashCode = 0;

            if (_s_isValueType0 || !ReferenceEquals(_value0, null))
            {
                hashCode ^= _value0.GetHashCode();
            }
            if (_s_isValueType1 || !ReferenceEquals(_value1, null))
            {
                hashCode ^= _value1.GetHashCode();
            }
            if (_s_isValueType2 || !ReferenceEquals(_value2, null))
            {
                hashCode ^= _value2.GetHashCode();
            }
            if (_s_isValueType3 || !ReferenceEquals(_value3, null))
            {
                hashCode ^= _value3.GetHashCode();
            }

            return hashCode;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        public override string ToString()
        {
            return $"{_value0}_{_value1}_{_value2}_{_value3}";
        }

        #endregion

        //-------------------------------------------------------------------------------------------------------------------------------------------------

        private static readonly bool _s_isValueType0 = typeof(T0).GetTypeInfo().IsValueType;
        private static readonly bool _s_isValueType1 = typeof(T1).GetTypeInfo().IsValueType;
        private static readonly bool _s_isValueType2 = typeof(T2).GetTypeInfo().IsValueType;
        private static readonly bool _s_isValueType3 = typeof(T3).GetTypeInfo().IsValueType;
    }
}
