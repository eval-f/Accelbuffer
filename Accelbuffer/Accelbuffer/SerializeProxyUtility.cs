using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Accelbuffer
{
    internal static class SerializeProxyUtility
    {
        private static readonly ModuleBuilder s_ModuleBuilder;
        private static readonly AssemblyBuilder s_Builder;

        private static readonly TypeAttributes s_TypeAttributes;

        private static readonly ConstructorInfo s_IsReadOnlyAttrCtorInfo;
        private static readonly byte[] s_IsReadOnlyAttrBytes;

        private static readonly string s_ParameterName_Obj;
        private static readonly string s_ParameterName_Buffer;

        private static readonly string s_SerializeMethodName;
        private static readonly string s_DeserializeMethodName;

        private static readonly Type[] s_CharEncodingTypes;
        private static readonly Type[] s_InputBufferPtrTypes;
        private static readonly Type[][] s_EmptyParameterTypeCustomModifiers;
        private static readonly Type[][] s_ParameterTypeCustomModifiersWith1InAttr;
        private static readonly Type[][] s_ParameterTypeCustomModifiersWith2InAttr;

        private static readonly MethodAttributes s_OverrideMethodAttributes;

        static SerializeProxyUtility()
        {
            AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SerializeProxies.dll"), AssemblyBuilderAccess.RunAndSave);
            s_ModuleBuilder = builder.DefineDynamicModule("SerializeProxies", "SerializeProxies.dll");//测试结束，移除
            s_Builder = builder;

            //AssemblyBuilder builder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("SerializeProxies.dll"), AssemblyBuilderAccess.Run);
            //s_ModuleBuilder = builder.DefineDynamicModule("SerializeProxies");
            //s_Builder = builder;

            s_TypeAttributes = TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.Public | TypeAttributes.BeforeFieldInit;

            s_IsReadOnlyAttrCtorInfo = typeof(IsReadOnlyAttribute).GetConstructor(Type.EmptyTypes);
            s_IsReadOnlyAttrBytes = new byte[4] { 1, 0, 0, 0 };

            s_ParameterName_Obj = "obj";
            s_ParameterName_Buffer = "buffer";

            s_SerializeMethodName = "Serialize";
            s_DeserializeMethodName = "Deserialize";

            Type[] inAttrs = new Type[] { typeof(InAttribute) };

            s_CharEncodingTypes = new Type[] { typeof(CharEncoding) };
            s_InputBufferPtrTypes = new Type[] { typeof(InputBuffer*) };
            s_EmptyParameterTypeCustomModifiers = new Type[][] { };
            s_ParameterTypeCustomModifiersWith1InAttr = new Type[][] { inAttrs };
            s_ParameterTypeCustomModifiersWith2InAttr = new Type[][] { inAttrs, inAttrs };

            s_OverrideMethodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Final | MethodAttributes.NewSlot | MethodAttributes.Virtual;
        }

        public static Type GenerateProxy(Type objType, bool ignoreNonPublicField)
        {
            string typeName = objType.Name + "SerializeProxy";

            Type interfaceType = typeof(ISerializeProxy<>).MakeGenericType(objType);

            TypeBuilder builder = s_ModuleBuilder.DefineType(typeName, s_TypeAttributes, typeof(object), new Type[] { interfaceType });

            builder.DefineDefaultConstructor(MethodAttributes.Public);

            objType.GetSerializedFields(ignoreNonPublicField, out List <FieldInfo> fields);

            DefineSerializeMethod(builder, objType, interfaceType, fields);

            DefineDeserializeMethod(builder, objType, interfaceType, fields);

            return builder.CreateType();
        }

        public static void Save()
        {
            s_Builder.Save("temp.dll");
        }

        private static void GetSerializedFields(this Type objType, bool ignoreNonPublicField, out List<FieldInfo> fields)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;

            if (!ignoreNonPublicField)
            {
                flags |= BindingFlags.NonPublic;
            }

            FieldInfo[] allFields = objType.GetFields(flags);
            fields = new List<FieldInfo>(allFields.Length);

            for (int i = 0; i < allFields.Length; i++)
            {
                FieldInfo field = allFields[i];

                if (field.GetCustomAttribute<CompilerGeneratedAttribute>() != null || 
                    field.GetCustomAttribute<IgnoreSerializeAttribute>() != null ||
                    field.IsInitOnly)
                {
                    continue;
                }

                fields.Add(field);
            }
        }

        private static void EmitEncoding(this ILGenerator il, FieldInfo field)
        {
            EncodingAttribute attribute = field.GetCustomAttribute<EncodingAttribute>();
            CharEncoding encoding = attribute == null ? CharEncoding.Unicode : attribute.Encoding;
            il.Emit(OpCodes.Ldc_I4, (int)encoding);
        }

        private static void DefineParameter(this MethodBuilder method, int position, string name)
        {
            method.DefineParameter(position, ParameterAttributes.In, name)
                .SetCustomAttribute(s_IsReadOnlyAttrCtorInfo, s_IsReadOnlyAttrBytes);
        }

        private static bool IsPrimitiveType(this Type type, out bool needEncoding)
        {
            return SerializeUtility.IsPrimitiveSerializableTypeInternal(type, out needEncoding);
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

        private static void EmitFieldSerialize(this ILGenerator il, Type objType, FieldInfo field, Type fieldType)
        {
            if (fieldType.IsPrimitiveType(out bool needEncoding))
            {
                il.Emit(OpCodes.Ldarg_2);
                il.Emit(OpCodes.Ldind_I);

                il.Emit(OpCodes.Ldarg_1);

                if (!objType.IsValueType)
                {
                    il.Emit(OpCodes.Ldind_Ref);
                }

                il.Emit(OpCodes.Ldfld, field);

                if (needEncoding)
                {
                    il.EmitEncoding(field);
                }

                Type[] argList = needEncoding ? new Type[] { fieldType, typeof(CharEncoding) } : new Type[] { fieldType };
                il.Emit(OpCodes.Call, typeof(OutputBuffer).GetMethod("WriteValue", argList));
            }
            else
            {

            }
        }

        private static void EmitFieldDeserialize(this ILGenerator il, Type objType, FieldInfo field, Type fieldType)
        {
            if (fieldType.IsPrimitiveType(out bool needEncoding))
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

                if (needEncoding)
                {
                    il.EmitEncoding(field);
                }

                Type[] argList = needEncoding ? s_CharEncodingTypes : Type.EmptyTypes;
                il.Emit(OpCodes.Call, typeof(InputBuffer).GetMethod("Read" + fieldType.GetPrimitiveTypeName(), argList));

                il.Emit(OpCodes.Stfld, field);
            }
            else
            {

            }
        }

        private static void DefineSerializeMethod(TypeBuilder builder, Type objType, Type interfaceType, List<FieldInfo> fields)
        {
            MethodBuilder method = builder.DefineMethod(s_SerializeMethodName,
                                                        s_OverrideMethodAttributes,
                                                        CallingConventions.Standard,
                                                        typeof(void),
                                                        Type.EmptyTypes,
                                                        Type.EmptyTypes,
                                                        new Type[] { objType, typeof(OutputBuffer*) },
                                                        s_ParameterTypeCustomModifiersWith2InAttr,
                                                        s_EmptyParameterTypeCustomModifiers);

            method.DefineParameter(1, s_ParameterName_Obj);
            method.DefineParameter(2, s_ParameterName_Buffer);

            ILGenerator il = method.GetILGenerator();

            for (int i = 0; i < fields.Count; i++)
            {
                il.EmitFieldSerialize(objType, fields[i], fields[i].FieldType);
            }

            il.Emit(OpCodes.Ret);

            builder.DefineMethodOverride(method, interfaceType.GetMethod(s_SerializeMethodName));
        }

        private static void DefineDeserializeMethod(TypeBuilder builder, Type objType, Type interfaceType, List<FieldInfo> fields)
        {
            MethodBuilder method = builder.DefineMethod(s_DeserializeMethodName,
                                                        s_OverrideMethodAttributes,
                                                        CallingConventions.Standard,
                                                        objType,
                                                        Type.EmptyTypes,
                                                        Type.EmptyTypes,
                                                        s_InputBufferPtrTypes,
                                                        s_ParameterTypeCustomModifiersWith1InAttr,
                                                        s_EmptyParameterTypeCustomModifiers);

            method.DefineParameter(1, s_ParameterName_Buffer);

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
                il.EmitFieldDeserialize(objType, fields[i], fields[i].FieldType);
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
