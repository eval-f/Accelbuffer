namespace Accelbuffer
{
    internal readonly struct BooleanTag : IValueTag
    {
        public ValueTypeCode TypeCode { get; }

        public bool Value { get; }

        public bool IsValid
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get { return TypeCode == ValueTypeCode.Boolean; }
        }


        public BooleanTag(bool value)
        {
            TypeCode = ValueTypeCode.Boolean;
            Value = value;
        }

        public BooleanTag(byte tag)
        {
            TypeCode = (ValueTypeCode)(tag >> 5);
            Value = ((tag >> 4) & 0x1) == 1;
        }


        public string ToErrorMessageString()
        {
            return $"type_code='{TypeCode.ToString()}', value='{Value.ToString()}'";
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public byte GetTagByte()
        {
            return (byte)(((int)TypeCode << 5) | ((Value ? 1 : 0) << 4));
        }
    }
}
