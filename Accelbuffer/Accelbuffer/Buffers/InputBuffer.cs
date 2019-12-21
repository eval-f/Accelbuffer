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
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="strictMode"></param>
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
            ToVariableIntegerTag(ReadByte(), out _, out _, out int byteCount);
            uint value = default;
            ReadBytes((byte*)&value, byteCount);
            return value;
        }

        private T ReadUVarInt<T>(byte index) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToVariableIntegerTag(ReadByte(), out ValueTypeCode typeCode, out IntegerSign sign, out int byteCount);

            if (typeCode != ValueTypeCode.VariableInteger)
            {
                throw new TagDismatchException(ValueTypeCode.VariableInteger, typeCode);
            }

            if (sign != IntegerSign.PositiveOrUnsigned)
            {
                throw new TagDismatchException(IntegerSign.PositiveOrUnsigned, sign);
            }

            if (byteCount > sizeof(T))
            {
                throw new TagDismatchException(sizeof(T), byteCount);
            }

            T value = default;

            ReadBytes((byte*)&value, byteCount);

            return value;
        }

        private T ReadSVarInt<T>(byte index) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToVariableIntegerTag(ReadByte(), out ValueTypeCode typeCode, out IntegerSign sign, out int byteCount);

            if (typeCode != ValueTypeCode.VariableInteger)
            {
                throw new TagDismatchException(ValueTypeCode.VariableInteger, typeCode);
            }

            if (byteCount > sizeof(T))
            {
                throw new TagDismatchException(sizeof(T), byteCount);
            }

            T value = default;

            ReadBytes((byte*)&value, byteCount);

            if (sign == IntegerSign.Negative)
            {
                OnesComplement((byte*)&value, sizeof(T));
            }

            return value;
        }

        private T ReadUFixedInt<T>(byte index) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToFixedIntegerTag(ReadByte(), out ValueTypeCode typeCode, out int byteCount);

            if (typeCode != ValueTypeCode.FixedInteger)
            {
                throw new TagDismatchException(ValueTypeCode.FixedInteger, typeCode);
            }

            if (byteCount != sizeof(T))
            {
                throw new TagDismatchException(sizeof(T), byteCount);
            }

            T value = default;

            ReadBytes((byte*)&value, byteCount);

            return value;
        }

        private T ReadSFixedInt<T>(byte index) where T : unmanaged
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToFixedIntegerTag(ReadByte(), out ValueTypeCode typeCode, out int byteCount);

            if (typeCode != ValueTypeCode.FixedInteger)
            {
                throw new TagDismatchException(ValueTypeCode.FixedInteger, typeCode);
            }

            if (byteCount != sizeof(T))
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
            return ReadSVarInt<sbyte>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte ReadVariableUInt8(byte index)
        {
            return ReadUVarInt<byte>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public short ReadVariableInt16(byte index)
        {
            return ReadSVarInt<short>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort ReadVariableUInt16(byte index)
        {
            return ReadUVarInt<ushort>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ReadVariableInt32(byte index)
        {
            return ReadSVarInt<int>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint ReadVariableUInt32(byte index)
        {
            return ReadUVarInt<uint>(index);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long ReadVariableInt64(byte index)
        {
            return ReadSVarInt<long>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ulong ReadVariableUInt64(byte index)
        {
            return ReadUVarInt<ulong>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public sbyte ReadFixedInt8(byte index)
        {
            return ReadSFixedInt<sbyte>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte ReadFixedUInt8(byte index)
        {
            return ReadUFixedInt<byte>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public short ReadFixedInt16(byte index)
        {
            return ReadSFixedInt<short>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ushort ReadFixedUInt16(byte index)
        {
            return ReadUFixedInt<ushort>(index);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int ReadFixedInt32(byte index)
        {
            return ReadSFixedInt<int>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public uint ReadFixedUInt32(byte index)
        {
            return ReadUFixedInt<uint>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public long ReadFixedInt64(byte index)
        {
            return ReadSFixedInt<long>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ulong ReadFixedUInt64(byte index)
        {
            return ReadUFixedInt<ulong>(index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool ReadBool(byte index)
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

            ToCharTag(ReadByte(), out ValueTypeCode typeCode, out CharValueType valueType, out bool isDefaultValue, out CharEncoding e, out _);

            if (typeCode != ValueTypeCode.Char)
            {
                throw new TagDismatchException(ValueTypeCode.Char, typeCode);
            }

            if (valueType != CharValueType.SingleChar)
            {
                throw new TagDismatchException(CharValueType.SingleChar, valueType);
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
        public float ReadFloat32(byte index)
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToFloatTag(ReadByte(), out ValueTypeCode typeCode, out bool isFloat32, out bool isDefaultValue);

            if (typeCode != ValueTypeCode.Float)
            {
                throw new TagDismatchException(ValueTypeCode.Float, typeCode);
            }

            if (!isFloat32)
            {
                throw new TagDismatchException("转换无效(float64 -> float32)");
            }

            if (isDefaultValue)
            {
                return default;
            }

            float value;
            ReadBytes(((byte*)&value), 4);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double ReadFloat64(byte index)
        {
            if (ReadByte() != index)
            {
                OnIndexNotMatch(index);
                return default;
            }

            ToFloatTag(ReadByte(), out ValueTypeCode typeCode, out bool isFloat32, out bool isDefaultValue);

            if (typeCode != ValueTypeCode.Float)
            {
                throw new TagDismatchException(ValueTypeCode.Float, typeCode);
            }

            if (isFloat32)
            {
                throw new TagDismatchException("转换无效(float32 -> float64)");
            }

            if (isDefaultValue)
            {
                return default;
            }

            double value;
            ReadBytes(((byte*)&value), 8);
            return value;
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

            ToCharTag(ReadByte(), out ValueTypeCode typeCode, out CharValueType valueType, out bool isDefaultValue, out CharEncoding e, out bool isEmpty);

            if (typeCode != ValueTypeCode.Char)
            {
                throw new TagDismatchException(ValueTypeCode.Char, typeCode);
            }

            if (valueType != CharValueType.String)
            {
                throw new TagDismatchException(CharValueType.String, valueType);
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
