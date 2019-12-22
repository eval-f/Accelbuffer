using System;
using System.Text;
using static Accelbuffer.SerializeUtility;
using static Accelbuffer.TagUtility;

namespace Accelbuffer
{
    /// <summary>
    /// 用于输入数据的缓冲区
    /// </summary>
    public unsafe struct InputBuffer
    {
        private readonly byte* m_Buffer;
        private readonly long m_Size;
        private long m_ReadCount;
        private bool m_StrictMode;

        /// <summary>
        /// 使用指定的字节指针床垫输入缓冲区
        /// </summary>
        /// <param name="source">指向一个字节数组的指针</param>
        /// <param name="size">缓冲区可以读取的字节数量</param>
        /// <param name="strictMode">是否启用严格模式（开启对序列化索引不匹配的错误警告）</param>
        public InputBuffer(byte* source, long size, bool strictMode)
        {
            m_Buffer = source;
            m_Size = size;
            m_ReadCount = 0;
            m_StrictMode = strictMode;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            if (m_ReadCount == m_Size)
            {
                throw new IndexOutOfRangeException("缓冲区已经读取至末尾");
            }

            return *(m_Buffer + m_ReadCount++);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        public void ReadBytes(byte* buffer, long length)
        {
            if (length <= 0)
            {
                return;
            }

            if (m_ReadCount + length > m_Size)
            {
                length = m_Size - m_ReadCount;
            }

            while (length > 0)
            {
                *buffer++ = *(m_Buffer + m_ReadCount++);
                length--;
            }
        }

        private void OnIndexNotMatch(byte index)
        {
            if (m_StrictMode)
            {
                throw new MissingSerializedValueException(index);
            }

            m_ReadCount--;
        }

        private uint ReadLength()
        {
            ToVariableNumberTag(ReadByte(), out _, out _, out int byteCount);
            uint value = default;
            ReadBytes((byte*)&value, byteCount);
            return value;
        }

        private T ReadUVarNumber<T>(byte index, ValueTypeCode expected) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToVariableNumberTag(ReadByte(), out ValueTypeCode typeCode, out NumberSign sign, out int byteCount);

            if (typeCode != expected)
            {
                throw new TagDismatchException(expected, typeCode);
            }

            if (sign != NumberSign.PositiveOrUnsigned)
            {
                throw new TagDismatchException(NumberSign.PositiveOrUnsigned, sign);
            }

            if (byteCount > sizeof(T))
            {
                throw new TagDismatchException(sizeof(T), byteCount);
            }

            T value = default;

            ReadBytes((byte*)&value, byteCount);

            return value;
        }

        private T ReadSVarNumber<T>(byte index, ValueTypeCode expected) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToVariableNumberTag(ReadByte(), out ValueTypeCode typeCode, out NumberSign sign, out int byteCount);

            if (typeCode != expected)
            {
                throw new TagDismatchException(expected, typeCode);
            }

            if (byteCount > sizeof(T))
            {
                throw new TagDismatchException(sizeof(T), byteCount);
            }

            T value = default;

            ReadBytes((byte*)&value, byteCount);

            if (sign == NumberSign.Negative)
            {
                OnesComplement((byte*)&value, sizeof(T));
            }

            return value;
        }

        private T ReadFixedNumber<T>(byte index, ValueTypeCode expected) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToFixedNumberTag(ReadByte(), out ValueTypeCode typeCode, out int byteCount);

            if (typeCode != expected)
            {
                throw new TagDismatchException(expected, typeCode);
            }

            if (byteCount != sizeof(T) && byteCount != 0)
            {
                throw new TagDismatchException(sizeof(T), byteCount);
            }

            T value = default;

            ReadBytes((byte*)&value, byteCount);

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public sbyte ReadVariableInt8(byte index)
        {
            return ReadSVarNumber<sbyte>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte ReadVariableUInt8(byte index)
        {
            return ReadUVarNumber<byte>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public short ReadVariableInt16(byte index)
        {
            return ReadSVarNumber<short>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort ReadVariableUInt16(byte index)
        {
            return ReadUVarNumber<ushort>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ReadVariableInt32(byte index)
        {
            return ReadSVarNumber<int>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint ReadVariableUInt32(byte index)
        {
            return ReadUVarNumber<uint>(index, ValueTypeCode.VariableInteger);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long ReadVariableInt64(byte index)
        {
            return ReadSVarNumber<long>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ulong ReadVariableUInt64(byte index)
        {
            return ReadUVarNumber<ulong>(index, ValueTypeCode.VariableInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public sbyte ReadFixedInt8(byte index)
        {
            return ReadFixedNumber<sbyte>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte ReadFixedUInt8(byte index)
        {
            return ReadFixedNumber<byte>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public short ReadFixedInt16(byte index)
        {
            return ReadFixedNumber<short>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort ReadFixedUInt16(byte index)
        {
            return ReadFixedNumber<ushort>(index, ValueTypeCode.FixedInteger);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ReadFixedInt32(byte index)
        {
            return ReadFixedNumber<int>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint ReadFixedUInt32(byte index)
        {
            return ReadFixedNumber<uint>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long ReadFixedInt64(byte index)
        {
            return ReadFixedNumber<long>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ulong ReadFixedUInt64(byte index)
        {
            return ReadFixedNumber<ulong>(index, ValueTypeCode.FixedInteger);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ReadBoolean(byte index)
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToBooleanTag(ReadByte(), out ValueTypeCode typeCode, out bool value);

            if (typeCode != ValueTypeCode.Boolean)
            {
                throw new TagDismatchException(ValueTypeCode.Boolean, typeCode);
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public char ReadChar(byte index, CharEncoding encoding)
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToCharTag(ReadByte(), out ValueTypeCode typeCode, out CharType valueType, out bool isDefaultValue, out CharEncoding e, out _);

            if (typeCode != ValueTypeCode.Char)
            {
                throw new TagDismatchException(ValueTypeCode.Char, typeCode);
            }

            if (valueType != CharType.SingleChar)
            {
                throw new TagDismatchException(CharType.SingleChar, valueType);
            }

            if (encoding != e)
            {
                throw new TagDismatchException(encoding, e);
            }

            if (isDefaultValue)
            {
                return default;
            }

            char value;

            switch (encoding)
            {
                case CharEncoding.Unicode:
                    ReadBytes((byte*)&value, 2);
                    break;

                case CharEncoding.ASCII:
                    byte asciiByte;
                    asciiByte = ReadByte();
                    Encoding.ASCII.GetChars(&asciiByte, 1, &value, 1);
                    break;

                default://case CharEncoding.UTF8:
                    byte* utf8Byte = stackalloc byte[4];
                    byte b = *utf8Byte = ReadByte();
                    int byteCount = 1;

                    if ((b >> 3) == 0x1E)
                    {
                        ReadBytes(utf8Byte + 1, 3);
                        byteCount = 4;
                    }
                    else if ((b >> 4) == 0xE)
                    {
                        ReadBytes(utf8Byte + 1, 2);
                        byteCount = 3;
                    }
                    else if ((b >> 5) == 0x6)
                    {
                        ReadBytes(utf8Byte + 1, 1);
                        byteCount = 2;
                    }

                    Encoding.UTF8.GetChars(utf8Byte, byteCount, &value, 1);
                    break;
            }

            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float ReadVariableFloat32(byte index)
        {
            return ReadUVarNumber<float>(index, ValueTypeCode.VariableFloat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double ReadVariableFloat64(byte index)
        {
            return ReadUVarNumber<double>(index, ValueTypeCode.VariableFloat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public float ReadFixedFloat32(byte index)
        {
            return ReadFixedNumber<float>(index, ValueTypeCode.FixedFloat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double ReadFixedFloat64(byte index)
        {
            return ReadFixedNumber<double>(index, ValueTypeCode.FixedFloat);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public string ReadString(byte index, CharEncoding encoding)
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToCharTag(ReadByte(), out ValueTypeCode typeCode, out CharType valueType, out bool isDefaultValue, out CharEncoding e, out bool isEmpty);

            if (typeCode != ValueTypeCode.Char)
            {
                throw new TagDismatchException(ValueTypeCode.Char, typeCode);
            }

            if (valueType != CharType.String)
            {
                throw new TagDismatchException(CharType.String, valueType);
            }

            if (encoding != e)
            {
                throw new TagDismatchException(encoding, e);
            }

            if (isDefaultValue)
            {
                return default;
            }

            if (isEmpty)
            {
                return string.Empty;
            }

            string value;

            int len = (int)ReadLength();

            byte* bs = stackalloc byte[len];
            ReadBytes(bs, len);

            switch (encoding)
            {
                case CharEncoding.Unicode:
                    value = Encoding.Unicode.GetString(bs, len);
                    break;

                case CharEncoding.ASCII:
                    value = Encoding.ASCII.GetString(bs, len);
                    break;

                default://case CharEncoding.UTF32:
                    value = Encoding.UTF8.GetString(bs, len);
                    break;
            }

            return value;
        }
    }
}
