using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static Accelbuffer.TagUtility;

namespace Accelbuffer
{
    /// <summary>
    /// 用于输出数据的缓冲区
    /// </summary>
    public unsafe struct OutputBuffer
    {
        private byte* m_Buffer;
        private long m_ByteCount;
        private readonly bool m_PublicFreeable;

        /// <summary>
        /// 获取当前缓冲区的大小
        /// </summary>
        public long Size { get; private set; }

        internal OutputBuffer(long defaultSize, bool publicFreeable)
        {
            m_Buffer = (byte*)Marshal.AllocHGlobal(new IntPtr(defaultSize)).ToPointer();
            Size = defaultSize;
            m_ByteCount = 0;
            m_PublicFreeable = publicFreeable;
        }

        /// <summary>
        /// 创建一个具有默认大小的缓冲区
        /// </summary>
        /// <param name="defaultSize">缓冲区的默认大小</param>
        /// <exception cref="ArgumentOutOfRangeException">缓冲区的大小小于等于0</exception>
        public OutputBuffer(long defaultSize) : this(defaultSize, true)
        {
            if (defaultSize <= 0)
            {
                throw new ArgumentOutOfRangeException("字节缓冲区的大小不能小于等于0");
            }
        }

        /// <summary>
        /// 尝试释放当前缓冲区使用的内存
        /// </summary>
        /// <exception cref="InvalidOperationException">正在尝试释放内部创建的缓冲区</exception>
        public void TryFree()
        {
            if (!m_PublicFreeable)
            {
                throw new InvalidOperationException("无法释放释放内部创建的缓冲区");
            }

            Free();
        }

        internal void Free()
        {
            if (m_Buffer != null)
            {
                Marshal.FreeHGlobal(new IntPtr(m_Buffer));
                m_Buffer = null;
                Size = 0;
                m_ByteCount = 0;
            }
        }

        /// <summary>
        /// 将当前的缓冲区指针置为起始位置
        /// </summary>
        public void Reset()
        {
            m_ByteCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureSize(long min)
        {
            long size = Size << 1;
            Size = size < min ? min : size;
            m_Buffer = (byte*)Marshal.ReAllocHGlobal(new IntPtr(m_Buffer), new IntPtr(Size)).ToPointer();
        }

        /// <summary>
        /// 将当前缓冲区内的所有字节都转换为托管字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            if (m_Buffer == null)
            {
                return null;
            }

            byte[] result = m_ByteCount == 0 ? Array.Empty<byte>() : new byte[m_ByteCount];
            CopyToArray(new ArraySegment<byte>(result));
            return result;
        }

        /// <summary>
        /// 将当前缓冲区内的所有字节都拷贝至托管字节数组
        /// </summary>
        /// <param name="array">需要被拷贝到的字节数组</param>
        /// <exception cref="ArgumentException">字节数组容量不足</exception>
        /// <returns>拷贝的字节数量</returns>
        public long CopyToArray(ArraySegment<byte> array)
        {
            long count = m_ByteCount;

            if (array.Count < count)
            {
                throw new ArgumentException("字节数组容量不足");
            }

            byte* source = m_Buffer;

            fixed (byte* ptr = array.Array)
            {
                byte* p = ptr + array.Offset;

                while (count-- > 0)
                {
                    *p++ = *source++;
                }
            }

            return m_ByteCount;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public void WriteBytes(byte* bytes, int length)
        {
            if (length < 0)
            {
                return;
            }

            long size = m_ByteCount + length;

            if (size >= Size)
            {
                EnsureSize(size);
            }

            byte* p = m_Buffer + m_ByteCount;

            while (length > 0)
            {
                *p++ = *bytes++;
                m_ByteCount++;
                length--;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="b"></param>
        public void WriteByte(byte b)
        {
            long size = m_ByteCount + 1;

            if (size >= Size)
            {
                EnsureSize(size);
            }

            m_Buffer[m_ByteCount] = b;
            m_ByteCount++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, sbyte value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, byte value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes(&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, int value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, uint value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, short value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, ushort value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, long value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, ulong value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedIntegerTag(value, out int byteCount) : MakeVariableIntegerTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void WriteValue(byte index, bool value)
        {
            WriteByte(index);
            WriteByte(MakeBooleanTag(value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public void WriteValue(byte index, char value, CharEncoding encoding)
        {
            byte tag = MakeCharTag(value, encoding, out bool isDefaultValue);
            WriteByte(index);
            WriteByte(tag);

            if (isDefaultValue)
            {
                return;
            }

            switch (encoding)
            {
                case CharEncoding.Unicode:
                    WriteBytes((byte*)&value, 2);
                    break;

                case CharEncoding.ASCII:
                    byte asciiByte;
                    Encoding.ASCII.GetBytes(&value, 1, &asciiByte, 1);
                    WriteByte(asciiByte);
                    break;

                default://case CharEncoding.UTF8:
                    byte* utf8CharByte = stackalloc byte[4];
                    WriteBytes(utf8CharByte, Encoding.UTF8.GetBytes(&value, 1, utf8CharByte, 4));
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, float value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedFloatTag(value, out int byteCount) : MakeVariableFloatTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="isFixed"></param>
        public void WriteValue(byte index, double value, bool isFixed)
        {
            byte tag = isFixed ? MakeFixedFloatTag(value, out int byteCount) : MakeVariableFloatTag(&value, out byteCount);
            WriteByte(index);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <param name="encoding"></param>
        public void WriteValue(byte index, string value, CharEncoding encoding)
        {
            byte tag = MakeCharTag(value, encoding, out bool isDefaultValue, out bool isEmpty);
            WriteByte(index);
            WriteByte(tag);

            if (isDefaultValue || isEmpty)
            {
                return;
            }

            int byteCount;
            byte* bytes;

            fixed (char* p = value)
            {
                switch (encoding)
                {
                    case CharEncoding.Unicode:
                        int unicodeLen = value.Length << 1;
                        byte* unicodeByte = stackalloc byte[unicodeLen];
                        byteCount = Encoding.Unicode.GetBytes(p, value.Length, unicodeByte, unicodeLen);
                        bytes = unicodeByte;
                        break;

                    case CharEncoding.ASCII:
                        byte* asciiByte = stackalloc byte[value.Length];
                        byteCount = Encoding.ASCII.GetBytes(p, value.Length, asciiByte, value.Length);
                        bytes = asciiByte;
                        break;

                    default://case CharEncoding.UTF8:
                        int utf8Len = value.Length << 2;
                        byte* utf8Byte = stackalloc byte[utf8Len];
                        byteCount = Encoding.UTF8.GetBytes(p, value.Length, utf8Byte, utf8Len);
                        bytes = utf8Byte;
                        break;
                }
            }

            WriteLength((uint)byteCount);
            WriteBytes(bytes, byteCount);
        }

        private void WriteLength(uint value)
        {
            byte tag = MakeVariableIntegerTag(&value, out int byteCount);
            WriteByte(tag);
            WriteBytes((byte*)&value, byteCount);
        }
    }
}
