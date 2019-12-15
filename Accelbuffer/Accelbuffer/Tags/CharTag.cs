namespace Accelbuffer
{
    internal readonly struct CharTag : IValueTag
    {
        public static readonly char CharDefaultValue = '\0';

        public static readonly string StringDefaultValue = null;

        public ValueTypeCode TypeCode { get; }

        public CharValueType ValueType { get; }

        public bool IsDefaultValue { get; }

        public CharEncoding Encoding { get; }

        public bool IsEmptyString { get; }

        public bool IsValid
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get { return TypeCode == ValueTypeCode.Char; }
        }


        public CharTag(byte tag)
        {
            TypeCode = (ValueTypeCode)(tag >> 5);
            ValueType = (CharValueType)((tag >> 4) & 0x1);
            IsDefaultValue = ((tag >> 3) & 0x1) == 1;
            Encoding = (CharEncoding)((tag >> 1) & 0x3);
            IsEmptyString = (tag & 0x1) == 1;
        }

        private CharTag(CharValueType valueType, bool isDefaultValue, CharEncoding encoding, bool isEmptyString)
        {
            TypeCode = ValueTypeCode.Char;
            ValueType = valueType;
            IsDefaultValue = isDefaultValue;
            Encoding = encoding;
            IsEmptyString = isEmptyString;
        }

        public CharTag(char value, CharEncoding encoding) : this(CharValueType.SingleChar, value == default, encoding, false) { }

        public CharTag(string value, CharEncoding encoding) : this(CharValueType.String, value == default, encoding, value == string.Empty) { }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool MatchValueType(CharValueType valueType, CharEncoding encoding)
        {
            return IsValid && ValueType == valueType && Encoding == encoding;
        }

        public string ToErrorMessageString()
        {
            return $"type_code='{TypeCode.ToString()}', value_type='{ValueType.ToString()}', is_default_value='{IsDefaultValue.ToString()}', encoding='{Encoding.ToString()}'";
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public byte GetTagByte()
        {
            return (byte)(((int)TypeCode << 5) | ((int)ValueType << 4) | ((IsDefaultValue ? 1 : 0) << 3) | ((int)Encoding << 1) | (IsEmptyString ? 1 : 0));
        }
    }
}
