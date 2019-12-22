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
        public static NumberSign GetSign(byte* value)
        {
            return (NumberSign)((*value) >> 7 & 0x1);
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
            switch (size)
            {
                case 1: OnesComplement((sbyte*)value); break;
                case 2: OnesComplement((short*)value); break;
                case 4: OnesComplement((int*)value); break;
                case 8: OnesComplement((long*)value); break;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSerializablePrimitiveType(Type type, out bool isChar, out bool isNumber)
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
                case TypeCode.Single:
                case TypeCode.Double:
                    isChar = false;
                    isNumber = true;
                    return true;
                case TypeCode.Boolean:
                    isChar = false;
                    isNumber = false;
                    return true;
                case TypeCode.Char:
                case TypeCode.String:
                    isChar = true;
                    isNumber = false;
                    return true;
                default:
                    isChar = false;
                    isNumber = false;
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
