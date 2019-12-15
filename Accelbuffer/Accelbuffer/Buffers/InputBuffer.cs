using System;
using System.Text;
using System.Runtime.InteropServices;

namespace Accelbuffer
{
    /// <summary>
    /// 用于输入数据的缓冲区
    /// </summary>
    public unsafe struct InputBuffer
    {
        public delegate int ReadByteFunction();

        private readonly byte* m_Buffer;
        private readonly long m_Size;
        private readonly IntPtr m_ReadByteFuncPtr;
        private long m_ReadCount;

        public InputBuffer(byte* source, long size)
        {
            m_Buffer = source;
            m_Size = size;
            m_ReadByteFuncPtr = IntPtr.Zero;
            m_ReadCount = 0;
        }

        public InputBuffer(ReadByteFunction readByteFunc, long size)
        {
            m_Buffer = null;
            m_Size = size;
            m_ReadByteFuncPtr = Marshal.GetFunctionPointerForDelegate<ReadByteFunction>(readByteFunc);
            m_ReadCount = 0;
        }

        public byte ReadByte()
        {
            if (m_ReadCount == m_Size)
            {
                throw new IndexOutOfRangeException();
            }

            if (m_Buffer == null)
            {
                m_ReadCount++;
                return (byte)Marshal.GetDelegateForFunctionPointer<ReadByteFunction>(m_ReadByteFuncPtr)();
            }

            return *(m_Buffer + m_ReadCount++);
        }

        public void ReadBytes(byte* buffer, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (m_ReadCount + length > m_Size)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (m_Buffer == null)
            {
                for (int i = 0; i < length; i++)
                {
                    buffer[i] = (byte)Marshal.GetDelegateForFunctionPointer<ReadByteFunction>(m_ReadByteFuncPtr)();
                    m_ReadCount++;
                }
            }
            else
            {
                while (length > 0)
                {
                    *buffer++ = *(m_Buffer + m_ReadCount++);
                    length--;
                }
            }
        }

        public sbyte ReadInt8()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_8b))
            {
                sbyte value = IntegerTag.Integer8DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                if (tag.Sign == IntegerSign.Negative)
                {
                    IntegerTag.NegateBits(&value);
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public byte ReadUInt8()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_8b, IntegerSign.PositiveOrUnsigned))
            {
                byte value = IntegerTag.UInteger8DefaultValue;

                ReadBytes(&value, tag.ByteCount);

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public short ReadInt16()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_16b))
            {
                short value = IntegerTag.Integer16DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                if (tag.Sign == IntegerSign.Negative)
                {
                    IntegerTag.NegateBits(&value);
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public ushort ReadUInt16()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_16b, IntegerSign.PositiveOrUnsigned))
            {
                ushort value = IntegerTag.UInteger16DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public int ReadInt32()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_32b))
            {
                int value = IntegerTag.Integer32DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                if (tag.Sign == IntegerSign.Negative)
                {
                    IntegerTag.NegateBits(&value);
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public uint ReadUInt32()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_32b, IntegerSign.PositiveOrUnsigned))
            {
                uint value = IntegerTag.UInteger32DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public long ReadInt64()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_64b))
            {
                long value = IntegerTag.Integer64DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                if (tag.Sign == IntegerSign.Negative)
                {
                    IntegerTag.NegateBits(&value);
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public ulong ReadUInt64()
        {
            IntegerTag tag = new IntegerTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Integer_64b, IntegerSign.PositiveOrUnsigned))
            {
                ulong value = IntegerTag.UInteger64DefaultValue;

                ReadBytes((byte*)&value, tag.ByteCount);

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public bool ReadBool()
        {
            BooleanTag tag = new BooleanTag(ReadByte());

            if (tag.IsValid)
            {
                return tag.Value;
            }

            throw new TagDismatchException(tag);
        }

        public char ReadChar(CharEncoding encoding)
        {
            CharTag tag = new CharTag(ReadByte());

            if (tag.MatchValueType(CharValueType.SingleChar, encoding))
            {
                char value = CharTag.CharDefaultValue;

                if (!tag.IsDefaultValue)
                {
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
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public float ReadFloat32()
        {
            FloatTag tag = new FloatTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Float_32b))
            {
                float value = FloatTag.Float32DefaultValue;

                if (!tag.IsDefaultValue)
                {
                    ReadBytes(((byte*)&value), 4);
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public double ReadFloat64()
        {
            FloatTag tag = new FloatTag(ReadByte());

            if (tag.MatchTypeCode(ValueTypeCode.Float_64b))
            {
                double value = FloatTag.Float64DefaultValue;

                if (!tag.IsDefaultValue)
                {
                    ReadBytes(((byte*)&value), 8);
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }

        public string ReadString(CharEncoding encoding)
        {
            CharTag tag = new CharTag(ReadByte());

            if (tag.MatchValueType(CharValueType.String, encoding))
            {
                string value = CharTag.StringDefaultValue;

                if (!tag.IsDefaultValue)
                {
                    if (tag.IsEmptyString)
                    {
                        value = string.Empty;
                    }
                    else
                    {
                        int len = ReadInt32();

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
                    }
                }

                return value;
            }

            throw new TagDismatchException(tag);
        }
    }
}
