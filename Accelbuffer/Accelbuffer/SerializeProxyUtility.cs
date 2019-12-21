using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accelbuffer
{
    /// <summary>
    /// 公开对序列化代理运行时注入的接口
    /// </summary>
    public static class SerializeProxyUtility
    {
        private struct FieldData
        {
            public readonly FieldInfo Field;
            public readonly byte Index;

            public FieldData(FieldInfo field, byte index)
            {
                Field = field;
                Index = index;
            }
        }

        private static readonly ModuleBuilder s_ModuleBuilder;

        /// <summary>
        /// 此属性用于测试，当代理注入功能完成时，将移除该属性
        /// </summary>
        public static AssemblyBuilder AssemblyBuilder { get; }//测试结束，移除

        private static readonly TypeAttributes s_TypeAttributes;

        private static readonly string s_ParameterName_Obj;
        private static readonly string s_ParameterName_Buffer;

        private static readonly string s_SerializeMethodName;
        private static readonly string s_DeserializeMethodName;

        private static readonly string s_FixedName;
        private static readonly string s_VariableName;

        private static readonly Type s_IsReadOnlyAttributeType;
        private static readonly ConstructorInfo s_IsReadOnlyAttributeCtor;
        private static readonly byte[] s_IsReadOnlyAttributeBytes;

        private static readonly Type[] s_IndexAndCharEncodingTypes;
        private static readonly Type[] s_IndexTypes;
        private static readonly Type[] s_InputBufferPtrTypes;

        private static readonly Type[][] s_EmptyTypes1;
        private static readonly Type[][] s_EmptyTypes2;
        private static readonly Type[][] s_InAttr1;
        private static readonly Type[][] s_InAttr2;

        private static readonly MethodAttributes s_OverrideMethodAttributes;

        static SerializeProxyUtility()
        {
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SerializeProxies.dll"), AssemblyBuilderAccess.RunAndSave);
            s_ModuleBuilder = builder.DefineDynamicModule("SerializeProxies", "SerializeProxies.dll");//测试结束，移除
            AssemblyBuilder = builder;

            //AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SerializeProxies.dll"), AssemblyBuilderAccess.Run);
            //s_ModuleBuilder = builder.DefineDynamicModule("SerializeProxies");
            //s_Builder = builder;

            s_TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

            s_ParameterName_Obj = "obj";
            s_ParameterName_Buffer = "buffer";

            s_SerializeMethodName = "Serialize";
            s_DeserializeMethodName = "Deserialize";

            s_FixedName = "Fixed";
            s_VariableName = "Variable";

            s_IsReadOnlyAttributeType = Type.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute");
            s_IsReadOnlyAttributeCtor = s_IsReadOnlyAttributeType.GetConstructor(Type.EmptyTypes);
            s_IsReadOnlyAttributeBytes = new byte[] { 1, 0, 0, 0 };


            s_IndexAndCharEncodingTypes = new Type[] { typeof(byte), typeof(CharEncoding) };
            s_IndexTypes = new Type[] { typeof(byte) };
            s_InputBufferPtrTypes = new Type[] { typeof(InputBuffer*).MakeByRefType() };

            s_EmptyTypes1 = new Type[][] { Type.EmptyTypes };
            s_EmptyTypes2 = new Type[][] { Type.EmptyTypes , Type.EmptyTypes };
            s_InAttr1 = new Type[][] { new Type[] { typeof(InAttribute) } };
            s_InAttr2 = new Type[][] { new Type[] { typeof(InAttribute) }, new Type[] { typeof(InAttribute) } };

            s_OverrideMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        }

        /// <summary>
        /// 注入指定类型对象的序列化代理
        /// </summary>
        /// <param name="objType">被序列化的对象类型</param>
        /// <returns>注入的序列化代理类型</returns>
        public static Type GenerateProxy(Type objType)
        {
            string typeName = objType.Name + "SerializeProxy";

            Type interfaceType = typeof(ISerializeProxy<>).MakeGenericType(objType);

            TypeBuilder builder = s_ModuleBuilder.DefineType(typeName, s_TypeAttributes, typeof(object), new Type[] { interfaceType });

            builder.DefineDefaultConstructor(MethodAttributes.Public);

            objType.GetSerializedFields(out List <FieldData> fields);

            DefineSerializeMethod(builder, objType, interfaceType, fields);

            DefineDeserializeMethod(builder, objType, interfaceType, fields);

            return builder.CreateType();
        }

        private static void GetSerializedFields(this Type objType, out List<FieldData> fields)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo[] allFields = objType.GetFields(flags);
            fields = new List<FieldData>(allFields.Length);

            for (int i = 0; i < allFields.Length; i++)
            {
                FieldInfo field = allFields[i];

                if (field.GetCustomAttribute<CompilerGeneratedAttribute>() != null || field.IsInitOnly)
                {
                    continue;
                }

                SerializedValueAttribute attribute = field.GetCustomAttribute<SerializedValueAttribute>(true);

                if (attribute != null)
                {
                    fields.Add(new FieldData(field, attribute.SerializeIndex));
                }
            }

            fields.Sort((f1, f2) => f1.Index - f2.Index);
        }

        private static void EmitEncoding(this ILGenerator il, FieldInfo field)
        {
            EncodingAttribute attribute = field.GetCustomAttribute<EncodingAttribute>(true);
            CharEncoding encoding = attribute == null ? CharEncoding.Unicode : attribute.Encoding;
            il.Emit(OpCodes.Ldc_I4, (int)encoding);
        }

        private static void EmitIsFixedInteger(this ILGenerator il, FieldInfo field)
        {
            bool isFixed = field.IsFixedInteger();
            il.Emit(isFixed ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0);
        }

        private static bool IsFixedInteger(this FieldInfo field)
        {
            return field.GetCustomAttribute<VariableIntegerAttribute>(true) == null;
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
                    return "Bool";
                case TypeCode.Char:
                    return "Char";
                case TypeCode.String:
                    return "String";
                default:
                    return string.Empty;
            }
        }

        private static void EmitFieldSerialize(this ILGenerator il, Type objType, FieldInfo field, Type fieldType, byte index)
        {
            if (SerializeUtility.IsSerializablePrimitiveType(fieldType, out bool needEncoding, out bool isInt))
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

                Type[] argList;

                if (needEncoding)
                {
                    il.EmitEncoding(field);
                    argList = new Type[] { typeof(byte), fieldType, typeof(CharEncoding) };
                }
                else if (isInt)
                {
                    il.EmitIsFixedInteger(field);
                    argList = new Type[] { typeof(byte), fieldType, typeof(bool) };
                }
                else
                {
                    argList = new Type[] { typeof(byte), fieldType };
                }

                il.Emit(OpCodes.Call, typeof(OutputBuffer).GetMethod("WriteValue", argList));
            }
            else
            {

            }
        }

        private static void EmitFieldDeserialize(this ILGenerator il, Type objType, FieldInfo field, Type fieldType, byte index)
        {
            if (SerializeUtility.IsSerializablePrimitiveType(fieldType, out bool needEncoding, out bool isInt))
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

                string methodName = string.Empty;

                if (needEncoding)
                {
                    il.EmitEncoding(field);
                }
                else if (isInt)
                {
                    methodName = (field.IsFixedInteger() ? s_FixedName : s_VariableName);
                }

                Type[] argList = needEncoding ? s_IndexAndCharEncodingTypes : s_IndexTypes;
                il.Emit(OpCodes.Call, typeof(InputBuffer).GetMethod("Read" + methodName + fieldType.GetPrimitiveTypeName(), argList));

                il.Emit(OpCodes.Stfld, field);
            }
            else
            {

            }
        }

        private static void DefineSerializeMethod(TypeBuilder builder, Type objType, Type interfaceType, List<FieldData> fields)
        {
            MethodBuilder method = builder.DefineMethod(s_SerializeMethodName,
                                                        s_OverrideMethodAttributes,
                                                        CallingConventions.Standard,
                                                        typeof(void),
                                                        Type.EmptyTypes,
                                                        Type.EmptyTypes,
                                                        new Type[] { objType.MakeByRefType(), typeof(OutputBuffer*).MakeByRefType() },
                                                        s_InAttr2, s_EmptyTypes2);

            method.DefineParameter(1, ParameterAttributes.In, s_ParameterName_Obj).SetCustomAttribute(s_IsReadOnlyAttributeCtor, s_IsReadOnlyAttributeBytes);
            method.DefineParameter(2, ParameterAttributes.In, s_ParameterName_Buffer).SetCustomAttribute(s_IsReadOnlyAttributeCtor, s_IsReadOnlyAttributeBytes);

            ILGenerator il = method.GetILGenerator();

            for (int i = 0; i < fields.Count; i++)
            {
                il.EmitFieldSerialize(objType, fields[i].Field, fields[i].Field.FieldType, fields[i].Index);
            }

            il.Emit(OpCodes.Ret);

            builder.DefineMethodOverride(method, interfaceType.GetMethod(s_SerializeMethodName));
        }

        private static void DefineDeserializeMethod(TypeBuilder builder, Type objType, Type interfaceType, List<FieldData> fields)
        {
            MethodBuilder method = builder.DefineMethod(s_DeserializeMethodName,
                                                        s_OverrideMethodAttributes,
                                                        CallingConventions.Standard,
                                                        objType,
                                                        Type.EmptyTypes,
                                                        Type.EmptyTypes,
                                                        s_InputBufferPtrTypes,
                                                        s_InAttr1, s_EmptyTypes1);

            method.DefineParameter(1, ParameterAttributes.In, s_ParameterName_Buffer).SetCustomAttribute(s_IsReadOnlyAttributeCtor, s_IsReadOnlyAttributeBytes);

            ILGenerator il = method.GetILGenerator();

            if (objType.IsValueType)
            {
                il.DeclareLocal(objType);

                il.Emit(OpCodes.Ldloca_S, (byte)0);
                il.Emit(OpCodes.Initobj, objType);
            }
            else
            {
                il.Emit(OpCodes.Newobj, objType.GetConstructor(Type.EmptyTypes));
            }

            for (int i = 0; i < fields.Count; i++)
            {
                il.EmitFieldDeserialize(objType, fields[i].Field, fields[i].Field.FieldType, fields[i].Index);
            }

            if (objType.IsValueType)
            {
                il.Emit(OpCodes.Ldloc_0);
            }

            il.Emit(OpCodes.Ret);

            builder.DefineMethodOverride(method, interfaceType.GetMethod(s_DeserializeMethodName));
        }
    }
}
