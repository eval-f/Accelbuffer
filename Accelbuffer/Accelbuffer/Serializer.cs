using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization;

namespace Accelbuffer
{
    public static unsafe class Serializer<T>
    {
        private static readonly long DEFAULT_BUFFER_SIZE = 20L;

        public static ISerializeProxy<T> CachedProxy { get; }

        public static long DefaultBufferSize { get; }

        public static long CurrentBufferSize
        {
            get { return s_CachedBuffer == null ? 0L : s_CachedBuffer->Size; }
        }

        private static OutputBuffer* s_CachedBuffer;


        static Serializer()
        {
            Type objectType = typeof(T);
            Type proxyType;

            if (SerializeUtility.IsPrimitiveSerializableType(objectType))
            {
                proxyType = GetProxyType(objectType);
                DefaultBufferSize = GetPrimitiveTypeBufferSize(objectType);
            }
            else if (IsSerializablePrimitiveCollection(objectType, out Type elementType))
            {
                proxyType = GetProxyType(elementType);
                DefaultBufferSize = GetPrimitiveTypeBufferSize(elementType) << 5;
            }
            else
            {
                SerializeContractAttribute attr = objectType.GetCustomAttribute<SerializeContractAttribute>(true);

                if (attr == null)
                {
                    throw new SerializationException($"类型{objectType.Name}不能被序列化，因为没有{typeof(SerializeContractAttribute).Name}特性");
                }

                proxyType = GetProxyType(objectType, attr.ProxyType, attr.IgnoreNonPublicField);

                DefaultBufferSize = GetBufferSize(objectType, attr.DefaultBufferSize);
            }

            if (!typeof(ISerializeProxy<T>).IsAssignableFrom(proxyType))
            {
                throw new NullReferenceException("获取" + typeof(T).Name + "的序列化代理失败");
            }

            CachedProxy = (ISerializeProxy<T>)Activator.CreateInstance(proxyType);
            s_CachedBuffer = null;
        }


        private static bool IsSerializablePrimitiveCollection(Type objectType, out Type elementType)
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

            return SerializeUtility.IsPrimitiveSerializableType(elementType);
        }

        private static Type GetProxyType(Type objectType, Type proxyType, bool ignoreNonPublicField)
        {
            if (proxyType == null)
            {
                proxyType = SerializeProxyUtility.GenerateProxy(objectType, ignoreNonPublicField);
            }
            else
            {
                if (proxyType.IsGenericTypeDefinition)
                {
                    proxyType = proxyType.MakeGenericType(objectType.GenericTypeArguments);
                }
            }
            
            return proxyType;
        }

        private static Type GetProxyType(Type elementType)
        {
            if (elementType == typeof(string))
            {
                return typeof(StringSerializeProxy);
            }

            return typeof(PrimitiveTypeSerializeProxy<>).MakeGenericType(elementType);
        }

        private static long GetPrimitiveTypeBufferSize(Type objectType)
        {
            return objectType == typeof(string) ? DEFAULT_BUFFER_SIZE : Marshal.SizeOf(objectType);
        }

        private static long GetBufferSize(Type objectType, long size)
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
                    result = DEFAULT_BUFFER_SIZE;
                }

                return result;
            }

            return size;
        }

        private static void InitializeBuffer()
        {
            s_CachedBuffer = (OutputBuffer*)Marshal.AllocHGlobal(sizeof(OutputBuffer)).ToPointer();
            *s_CachedBuffer = new OutputBuffer(DefaultBufferSize);
            s_CachedBuffer->Reset();
        }

        private static void SerializePrivate(T obj)
        {
            if (s_CachedBuffer == null)
            {
                InitializeBuffer();
            }

            if (obj is ISerializeMessageReceiver receiver)
            {
                receiver.OnBeforeSerialize();
            }

            CachedProxy.Serialize(in obj, s_CachedBuffer);
        }

        
        public static void FreeBufferMemory()
        {
            if (s_CachedBuffer != null)
            {
                s_CachedBuffer->Free();
                Marshal.FreeHGlobal(new IntPtr(s_CachedBuffer));
                s_CachedBuffer = null;
            }
        }

        public static byte[] Serialize(T obj)
        {
            SerializePrivate(obj);

            byte[] result = s_CachedBuffer->ToArray();

            s_CachedBuffer->Reset();

            return result;
        }

        public static void Serialize(T obj, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("序列化输出流不能为null");
            }

            if (!stream.CanWrite)
            {
                throw new ArgumentException("序列化输出流不能为只读流");
            }

            SerializePrivate(obj);

            s_CachedBuffer->WriteToStream(stream);

            s_CachedBuffer->Reset();
        }

        public static T Deserialize(ArraySegment<byte> bytes)
        {
            if (bytes.Count == 0)
            {
                return default;
            }

            T result;

            fixed (byte* p = bytes.Array)
            {
                InputBuffer buffer = new InputBuffer(p + bytes.Offset, bytes.Count);

                result = CachedProxy.Deserialize(&buffer);

                if (result is ISerializeMessageReceiver receiver)
                {
                    receiver.OnAfterDeserialize();
                }
            }

            return result;
        }

        public static T Deserialize(Stream stream)
        {
            if (stream.Length == 0 || !stream.CanRead || !stream.CanSeek)
            {
                return default;
            }

            stream.Seek(0, SeekOrigin.Begin);

            InputBuffer buffer = new InputBuffer(stream.ReadByte, stream.Length);

            T result = CachedProxy.Deserialize(&buffer);

            if (result is ISerializeMessageReceiver receiver)
            {
                receiver.OnAfterDeserialize();
            }

            return result;
        }
    }
}
