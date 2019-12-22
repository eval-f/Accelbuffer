using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Accelbuffer.Experimental.RuntimeSerializeProxyInjection
{
    internal static class FieldILEmitUtility
    {
        private static readonly Type s_InputBufferType = typeof(InputBuffer);
        private static readonly Type s_OutputBufferType = typeof(OutputBuffer);
        private static readonly Type[] s_IndexAndCharEncodingTypes = new Type[] { typeof(byte), typeof(CharEncoding) };
        private static readonly Type[] s_IndexAndCharAndCharEncodingTypes = new Type[] { typeof(byte), typeof(char), typeof(CharEncoding) };
        private static readonly Type[] s_IndexAndStringAndCharEncodingTypes = new Type[] { typeof(byte), typeof(string), typeof(CharEncoding) };
        private static readonly Type[] s_IndexAndBoolTypes = new Type[] { typeof(byte), typeof(bool) };
        private static readonly Type[] s_IndexTypes = new Type[] { typeof(byte) };

        private static readonly string s_WriteValueString = "WriteValue";
        private static readonly string s_ReadString = "Read";
        private static readonly string s_FixedName = "Fixed";
        private static readonly string s_VariableName = "Variable";
   
        private static void EmitEncoding(this ILGenerator il, FieldInfo field)
        {
            EncodingAttribute attribute = field.GetCustomAttribute<EncodingAttribute>(true);
            CharEncoding encoding = attribute == null ? CharEncoding.Unicode : attribute.Encoding;
            il.Emit(OpCodes.Ldc_I4, (int)encoding);
        }

        private static void EmitIsFixedNumber(this ILGenerator il, FieldInfo field)
        {
            bool isFixed = field.IsFixedNumber();
            il.Emit(isFixed ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }

        private static bool IsFixedNumber(this FieldInfo field)
        {
            return field.GetCustomAttribute<VariableNumberAttribute>(true) == null;
        }

        private static string GetPrimitiveTypeName(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                    return "Int8";
                case TypeCode.Byte:
                    return "UInt8";
                case TypeCode.Int16:
                    return "Int16";
                case TypeCode.UInt16:
                    return "UInt16";
                case TypeCode.Int32:
                    return "Int32";
                case TypeCode.UInt32:
                    return "UInt32";
                case TypeCode.Int64:
                    return "Int64";
                case TypeCode.UInt64:
                    return "UInt64";
                case TypeCode.Single:
                    return "Float32";
                case TypeCode.Double:
                    return "Float64";
                case TypeCode.Boolean:
                    return "Boolean";
                case TypeCode.Char:
                    return "Char";
                case TypeCode.String:
                    return "String";
                default:
                    return string.Empty;
            }
        }

        public static void EmitFieldSerialize(this ILGenerator il, Type objType, FieldInfo field, Type fieldType, byte index)
        {
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldind_I);

            il.Emit(OpCodes.Ldc_I4, (int)index);

            il.Emit(OpCodes.Ldarg_1);

            if (!objType.IsValueType)
            {
                il.Emit(OpCodes.Ldind_Ref);
            }

            il.Emit(OpCodes.Ldfld, field);

            if (SerializeUtility.IsSerializablePrimitiveType(fieldType, out bool isChar, out bool isNumber))
            {
                if (isChar)
                {
                    EmitCharSerialize(il, field, fieldType);
                }
                else if (isNumber)
                {
                    EmitNumberSerialize(il, field, fieldType);
                }
                else//bool
                {
                    EmitBooleanSerialize(il);
                }
            }
            else if (fieldType.IsArray)
            {
                throw new NotSupportedException("数组序列化将在未来支持");
            }
            else
            {
                throw new NotSupportedException("复杂类型序列化将在未来支持");
            }
        }

        public static void EmitFieldDeserialize(this ILGenerator il, Type objType, FieldInfo field, Type fieldType, byte index)
        {
            if (objType.IsValueType)
            {
                il.Emit(OpCodes.Ldloca_S, (byte)0);
            }
            else
            {
                il.Emit(OpCodes.Dup);
            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldind_I);

            il.Emit(OpCodes.Ldc_I4, (int)index);

            if (SerializeUtility.IsSerializablePrimitiveType(fieldType, out bool isChar, out bool isNumber))
            {
                if (isChar)
                {
                    EmitCharDeserialize(il, field, fieldType);
                }
                else if (isNumber)
                {
                    EmitNumberDeserialize(il, field, fieldType);
                }
                else//bool
                {
                    EmitBooleanDeserialize(il);
                }
            }
            else if (fieldType.IsArray)
            {
                throw new NotSupportedException("数组反序列化将在未来支持");
            }
            else
            {
                throw new NotSupportedException("复杂类型反序列化将在未来支持");
            }

            il.Emit(OpCodes.Stfld, field);
        }

        public static void EmitNumberSerialize(ILGenerator il, FieldInfo field, Type fieldType)
        {
            il.EmitIsFixedNumber(field);
            MethodInfo method = s_OutputBufferType.GetMethod(s_WriteValueString, new Type[] { typeof(byte), fieldType, typeof(bool) });
            il.Emit(OpCodes.Call, method);
        }

        public static void EmitNumberDeserialize(ILGenerator il, FieldInfo field, Type fieldType)
        {
            string methodName = $"{s_ReadString}{(field.IsFixedNumber() ? s_FixedName : s_VariableName)}{fieldType.GetPrimitiveTypeName()}";
            MethodInfo method = s_InputBufferType.GetMethod(methodName, s_IndexTypes);
            il.Emit(OpCodes.Call, method);
        }

        public static void EmitCharSerialize(ILGenerator il, FieldInfo field, Type fieldType)
        {
            il.EmitEncoding(field);
            MethodInfo method = s_OutputBufferType.GetMethod(s_WriteValueString,
                fieldType == typeof(string) ? s_IndexAndStringAndCharEncodingTypes : s_IndexAndCharAndCharEncodingTypes);
            il.Emit(OpCodes.Call, method);
        }

        public static void EmitCharDeserialize(ILGenerator il, FieldInfo field, Type fieldType)
        {
            string methodName = $"{s_ReadString}{fieldType.GetPrimitiveTypeName()}";
            il.EmitEncoding(field);
            MethodInfo method = s_InputBufferType.GetMethod(methodName, s_IndexAndCharEncodingTypes);
            il.Emit(OpCodes.Call, method);
        }

        public static void EmitBooleanSerialize(ILGenerator il)
        {
            MethodInfo method = s_OutputBufferType.GetMethod(s_WriteValueString, s_IndexAndBoolTypes);
            il.Emit(OpCodes.Call, method);
        }

        public static void EmitBooleanDeserialize(ILGenerator il)
        {
            string methodName = $"{s_ReadString}{typeof(bool).GetPrimitiveTypeName()}";
            MethodInfo method = s_InputBufferType.GetMethod(methodName, s_IndexTypes);
            il.Emit(OpCodes.Call, method);
        }
    }
}
