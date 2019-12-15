namespace Accelbuffer
{
    internal readonly unsafe struct IntegerTag : IValueTag
    {
        public static readonly sbyte Integer8DefaultValue = 0;

        public static readonly byte UInteger8DefaultValue = 0;

        public static readonly short Integer16DefaultValue = 0;

        public static readonly ushort UInteger16DefaultValue = 0;

        public static readonly int Integer32DefaultValue = 0;

        public static readonly uint UInteger32DefaultValue = 0;

        public static readonly long Integer64DefaultValue = 0;

        public static readonly ulong UInteger64DefaultValue = 0;

        public ValueTypeCode TypeCode { get; }

        public IntegerSign Sign { get; }

        public int ByteCount { get; }

        public bool IsValid
        {
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
            get
            {
                switch (TypeCode)
                {
                    case ValueTypeCode.Integer_8b:
                    case ValueTypeCode.Integer_16b:
                    case ValueTypeCode.Integer_32b:
                    case ValueTypeCode.Integer_64b:
                        return true;
                    default:
                        return false;
                }
            }
        }


        public IntegerTag(byte tag)
        {
            TypeCode = (ValueTypeCode)(tag >> 5);
            Sign = (IntegerSign)((tag >> 4) & 0x1);
            ByteCount = tag & 0xF;
        }

        public IntegerTag(sbyte* value)
        {
            sbyte v = *value;

            TypeCode = ValueTypeCode.Integer_8b;

            Sign = (IntegerSign)(v >> 7 & 0x1);

            if (Sign == IntegerSign.Negative)
            {
                NegateBits(value);
            }

            ByteCount = GetUsedByteCount((byte*)value, 1);
        }

        public IntegerTag(byte* value)
        {
            TypeCode = ValueTypeCode.Integer_8b;

            Sign = IntegerSign.PositiveOrUnsigned;

            ByteCount = GetUsedByteCount(value, 1);
        }

        public IntegerTag(short* value)
        {
            short v = *value;

            TypeCode = ValueTypeCode.Integer_16b;

            Sign = (IntegerSign)(v >> 15 & 0x1);

            if (Sign == IntegerSign.Negative)
            {
                NegateBits(value);
            }

            ByteCount = GetUsedByteCount((byte*)value, 2);
        }

        public IntegerTag(ushort* value)
        {
            TypeCode = ValueTypeCode.Integer_16b;

            Sign = IntegerSign.PositiveOrUnsigned;

            ByteCount = GetUsedByteCount((byte*)value, 2);
        }

        public IntegerTag(int* value)
        {
            int v = *value;

            TypeCode = ValueTypeCode.Integer_32b;

            Sign = (IntegerSign)(v >> 31 & 0x1);

            if (Sign == IntegerSign.Negative)
            {
                NegateBits(value);
            }

            ByteCount = GetUsedByteCount((byte*)value, 4);
        }

        public IntegerTag(uint* value)
        {
            TypeCode = ValueTypeCode.Integer_32b;

            Sign = IntegerSign.PositiveOrUnsigned;

            ByteCount = GetUsedByteCount((byte*)value, 4);
        }

        public IntegerTag(long* value)
        {
            long v = *value;

            TypeCode = ValueTypeCode.Integer_64b;

            Sign = (IntegerSign)(v >> 63 & 0x1);

            if (Sign == IntegerSign.Negative)
            {
                NegateBits(value);
            }

            ByteCount = GetUsedByteCount((byte*)value, 8);
        }

        public IntegerTag(ulong* value)
        {
            TypeCode = ValueTypeCode.Integer_64b;

            Sign = IntegerSign.PositiveOrUnsigned;

            ByteCount = GetUsedByteCount((byte*)value, 8);
        }


        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public byte GetTagByte()
        {
            return (byte)(((int)TypeCode << 5) | ((int)Sign << 4) | ByteCount);
        }

        public string ToErrorMessageString()
        {
            return $"type_code='{TypeCode.ToString()}', sign='{Sign.ToString()}', byte_count='{ByteCount.ToString()}'";
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool MatchTypeCode(ValueTypeCode typeCode)
        {
            return IsValid && TypeCode == typeCode;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public bool MatchTypeCode(ValueTypeCode typeCode, IntegerSign sign)
        {
            return IsValid && TypeCode == typeCode && Sign == sign;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void NegateBits(sbyte* value)
        {
            *value = (sbyte)~*value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void NegateBits(short* value)
        {
            *value = (short)~*value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void NegateBits(int* value)
        {
            *value = ~*value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public static void NegateBits(long* value)
        {
            *value = ~*value;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private static int GetUsedByteCount(byte* p, int size)
        {
            p += size - 1;

            while (size > 0)
            {
                if (*p-- != 0)
                {
                    break;
                }

                size--;
            }

            return size;
        }
    }
}
