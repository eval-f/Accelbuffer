using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accelbuffer
{
    internal static unsafe class SerializeUtility 
    {
        private const long DEFAULT_INITIAL_BUFFER_SIZE = 20L;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntegerSign GetSign(byte* value)
        {
            return (IntegerSign)((*value) >> 7 & 0x1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnesComplement(sbyte* value)
        {
            *value = (sbyte)~*value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnesComplement(short* value)
        {
            *value = (short)~*value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnesComplement(int* value)
        {
            *value = ~*value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnesComplement(long* value)
        {
            *value = ~*value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnesComplement(byte* value, int size)
        {
            while (size > 0)
            {
                *value++ = (byte)~*value;
                size--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetUsedByteCount(byte* p, int size)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSerializablePrimitiveType(Type type)
        {
            return type == null ? false : IsSerializablePrimitiveType(type, out _, out _);
        }

        public static bool IsSerializablePrimitiveType(Type type, out bool needEncoding, out bool isInt)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    needEncoding = false;
                    isInt = true;
                    return true;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Boolean:
                    needEncoding = false;
                    isInt = false;
                    return true;
                case TypeCode.Char:
                case TypeCode.String:
                    needEncoding = true;
                    isInt = false;
                    return true;
                default:
                    needEncoding = false;
                    isInt = false;
                    return false;
            }
        }

        public static bool IsSerializablePrimitiveCollection(Type objectType, out Type elementType)
        {
            elementType = null;

            if (objectType.IsArray)
            {
                elementType = objectType.GetElementType();
            }
            else if (objectType.IsGenericType)
            {
                if (objectType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    elementType = objectType.GenericTypeArguments[0];
                }
            }

            return IsSerializablePrimitiveType(elementType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetPrimitiveTypeBufferSize(Type objectType)
        {
            return objectType == typeof(string) ? DEFAULT_INITIAL_BUFFER_SIZE : Marshal.SizeOf(objectType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetBufferSize(Type objectType, long size)
        {
            if (size == 0)
            {
                long result;

                try
                {
                    result = Marshal.SizeOf(objectType);
                }
                catch
                {
                    result = DEFAULT_INITIAL_BUFFER_SIZE;
                }

                return result;
            }

            return size;
        }
    }
}
