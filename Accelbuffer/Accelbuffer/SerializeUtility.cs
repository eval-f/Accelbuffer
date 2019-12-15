using System;

namespace Accelbuffer
{
    internal static class SerializeUtility 
    {
        public static bool IsPrimitiveSerializableType(Type type)
        {
            return type == null ? false : IsPrimitiveSerializableTypeInternal(type, out _);
        }

        public static bool IsPrimitiveSerializableTypeInternal(Type type, out bool needEncoding)
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
                case TypeCode.Boolean:
                    needEncoding = false;
                    return true;
                case TypeCode.Char:
                case TypeCode.String:
                    needEncoding = true;
                    return true;
                default:
                    needEncoding = false;
                    return false;
            }
        }
    }
}
