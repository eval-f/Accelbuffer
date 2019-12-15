namespace Accelbuffer
{
    internal readonly struct FloatTag : IValueTag
    {
        public static readonly float Float32DefaultValue = 0;

        public static readonly double Float64DefaultValue = 0;

        public ValueTypeCode TypeCode { get; }

        public bool IsDefaultValue { get; }

        public bool IsValid
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (TypeCode)
                {
                    case ValueTypeCode.Float_32b:
                    case ValueTypeCode.Float_64b:
                        return true;
                    default:
                        return false;
                }
            }
        }


        public FloatTag(byte tag)
        {
            TypeCode = (ValueTypeCode)(tag >> 5);
            IsDefaultValue = ((tag >> 4) & 0x1) == 1;
        }

        public FloatTag(float value)
        {
            TypeCode = ValueTypeCode.Float_32b;
            IsDefaultValue = value == Float32DefaultValue;
        }

        public FloatTag(double value)
        {
            TypeCode = ValueTypeCode.Float_64b;
            IsDefaultValue = value == Float64DefaultValue;
        }

        public string ToErrorMessageString()
        {
            return $"type_code='{TypeCode.ToString()}', is_default_value='{IsDefaultValue.ToString()}'";
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool MatchTypeCode(ValueTypeCode typeCode)
        {
            return IsValid && TypeCode == typeCode;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public byte GetTagByte()
        {
            return (byte)(((int)TypeCode << 5) | ((IsDefaultValue ? 1 : 0) << 4));
        }
    }
}
