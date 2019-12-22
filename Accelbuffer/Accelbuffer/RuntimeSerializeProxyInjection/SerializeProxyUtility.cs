using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accelbuffer.Experimental.RuntimeSerializeProxyInjection
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
        [Obsolete("此属性用于测试，当代理注入功能完成时，将移除该属性", false)]
        public static AssemblyBuilder AssemblyBuilder => s_AssemblyBuilder;

        private static readonly AssemblyBuilder s_AssemblyBuilder;

        private static readonly TypeAttributes s_TypeAttributes;

        private static readonly string s_ParameterName_Obj;
        private static readonly string s_ParameterName_Buffer;

        private static readonly string s_SerializeMethodName;
        private static readonly string s_DeserializeMethodName;

        private static readonly Type s_IsReadOnlyAttributeType;
        private static readonly ConstructorInfo s_IsReadOnlyAttributeCtor;
        private static readonly byte[] s_IsReadOnlyAttributeBytes;

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
            s_AssemblyBuilder = builder;

            //AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SerializeProxies.dll"), AssemblyBuilderAccess.Run);
            //s_ModuleBuilder = builder.DefineDynamicModule("SerializeProxies");
            //s_Builder = builder;

            s_TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

            s_ParameterName_Obj = "obj";
            s_ParameterName_Buffer = "buffer";

            s_SerializeMethodName = "Serialize";
            s_DeserializeMethodName = "Deserialize";

            s_IsReadOnlyAttributeType = Type.GetType("System.Runtime.CompilerServices.IsReadOnlyAttribute");
            s_IsReadOnlyAttributeCtor = s_IsReadOnlyAttributeType.GetConstructor(Type.EmptyTypes);
            s_IsReadOnlyAttributeBytes = new byte[] { 1, 0, 0, 0 };

            s_InputBufferPtrTypes = new Type[] { typeof(InputBuffer*).MakeByRefType() };

            s_EmptyTypes1 = new Type[][] { Type.EmptyTypes };
            s_EmptyTypes2 = new Type[][] { Type.EmptyTypes, Type.EmptyTypes };
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

            objType.GetSerializedFields(out List<FieldData> fields);

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

                SerializeIndexAttribute attribute = field.GetCustomAttribute<SerializeIndexAttribute>(true);

                if (attribute != null)
                {
                    fields.Add(new FieldData(field, attribute.SerializeIndex));
                }
            }

            fields.Sort((f1, f2) => f1.Index - f2.Index);
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
