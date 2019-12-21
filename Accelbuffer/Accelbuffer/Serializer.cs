using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Accelbuffer
{
    /// <summary>
    /// 公开序列化<typeparamref name="T"/>类型对象的接口
    /// </summary>
    /// <typeparam name="T">序列化类型</typeparam>
    public static unsafe class Serializer<T>
    {
        private static readonly ISerializeProxy<T> s_CachedProxy;

        private static OutputBuffer* s_CachedBuffer;

        private static readonly object s_Lock;

        /// <summary>
        /// 获取/设置 初始化缓冲区使用的字节大小，对于自定义类型，该值可能为<see cref="SerializeContractAttribute.InitialBufferSize"/>
        /// </summary>
        public static long InitialBufferSize { get; set; }

        /// <summary>
        /// 获取/设置 是否使用严格的序列化模式（开启对序列化索引不匹配的错误警告），对于自定义类型，该值可能为<see cref="SerializeContractAttribute.StrictMode"/>
        /// </summary>
        public static bool StrictMode { get; set; }

        /// <summary>
        /// 获取当前缓冲区使用的字节大小
        /// </summary>
        public static long CurrentBufferSize
        {
            get { return s_CachedBuffer == null ? 0L : s_CachedBuffer->Size; }
        }

        static Serializer()
        {
            Type objectType = typeof(T);
            Type proxyType;

            if (SerializeUtility.IsSerializablePrimitiveType(objectType))
            {
                proxyType = SerializeProxyUtility.GetPrimitiveProxyType(objectType);
                InitialBufferSize = SerializeUtility.GetPrimitiveTypeBufferSize(objectType);
                StrictMode = false;
            }
            else if (SerializeUtility.IsSerializablePrimitiveCollection(objectType, out Type elementType))
            {
                proxyType = SerializeProxyUtility.GetPrimitiveProxyType(elementType);
                InitialBufferSize = SerializeUtility.GetPrimitiveTypeBufferSize(elementType) << 3;
                StrictMode = false;
            }
            else
            {
                SerializeContractAttribute attr = objectType.GetCustomAttribute<SerializeContractAttribute>(true);

                if (attr == null)
                {
                    throw new SerializationException($"类型{objectType.Name}不能被序列化，因为没有被标记{typeof(SerializeContractAttribute).Name}特性");
                }

                proxyType = SerializeProxyUtility.GetProxyType(objectType, attr.ProxyType);

                InitialBufferSize = SerializeUtility.GetBufferSize(objectType, attr.InitialBufferSize);
                StrictMode = attr.StrictMode;
            }

            if (!typeof(ISerializeProxy<T>).IsAssignableFrom(proxyType))
            {
                throw new NullReferenceException("获取" + typeof(T).Name + "的序列化代理失败");
            }

            s_CachedProxy = (ISerializeProxy<T>)Activator.CreateInstance(proxyType);
            s_CachedBuffer = null;
            s_Lock = new object();
        }

        private static void InitializeBuffer()
        {
            s_CachedBuffer = (OutputBuffer*)Marshal.AllocHGlobal(sizeof(OutputBuffer)).ToPointer();
            *s_CachedBuffer = new OutputBuffer(InitialBufferSize);
            s_CachedBuffer->Reset();
        }

        private static void SerializePrivate(T obj, OutputBuffer* buffer)
        {
            if (obj is ISerializeMessageReceiver receiver)
            {
                receiver.OnBeforeSerialize();
            }

            s_CachedProxy.Serialize(in obj, buffer);
        }

        /// <summary>
        /// 释放当前序列化缓冲区使用的内存
        /// </summary>
        public static void FreeBufferMemory()
        {
            if (s_CachedBuffer != null)
            {
                s_CachedBuffer->Free();
                Marshal.FreeHGlobal(new IntPtr(s_CachedBuffer));
                s_CachedBuffer = null;
            }
        }

        /// <summary>
        /// 使用内部维护的缓冲区序列化对象，并返回序列化数据（线程安全）
        /// </summary>
        /// <param name="obj">被序列化的对象</param>
        /// <returns>对象的序列化结果</returns>
        public static byte[] Serialize(T obj)
        {
            lock (s_Lock)
            {
                if (s_CachedBuffer == null)
                {
                    InitializeBuffer();
                }

                SerializePrivate(obj, s_CachedBuffer);

                byte[] result = s_CachedBuffer->ToArray();

                s_CachedBuffer->Reset();

                return result;
            }
        }

        /// <summary>
        /// 使用内部维护的缓冲区序列化对象，并将序列化数据写入指定的缓冲区中（线程安全）
        /// </summary>
        /// <param name="obj">被序列化的对象</param>
        /// <param name="buffer">用于接受序列化数据的缓冲区</param>
        /// <returns>序列化数据的大小</returns>
        public static int Serialize(T obj, ArraySegment<byte> buffer)
        {
            lock (s_Lock)
            {
                if (s_CachedBuffer == null)
                {
                    InitializeBuffer();
                }

                SerializePrivate(obj, s_CachedBuffer);

                int result = (int)s_CachedBuffer->CopyToArray(buffer);

                s_CachedBuffer->Reset();

                return result;
            }
        }

        /// <summary>
        /// 使用指定的缓冲区序列化对象，并写入序列化数据中
        /// </summary>
        /// <param name="obj">被序列化的对象</param>
        /// <param name="buffer">用于序列化对象的缓冲区</param>
        /// <exception cref="ArgumentNullException">缓冲区指针为空</exception>
        public static void Serialize(T obj, OutputBuffer* buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer), "缓冲区指针不能为空");
            }

            SerializePrivate(obj, buffer);
        }

        /// <summary>
        /// 将指定的字节数组反序列化成<typeparamref name="T"/>类型对象实例
        /// </summary>
        /// <param name="bytes">被反序列化的字节数组</param>
        /// <returns>反序列化的对象实例</returns>
        public static T Deserialize(ArraySegment<byte> bytes)
        {
            if (bytes.Count == 0)
            {
                return default;
            }

            T result;

            fixed (byte* p = bytes.Array)
            {
                InputBuffer buffer = new InputBuffer(p + bytes.Offset, bytes.Count, StrictMode);

                result = s_CachedProxy.Deserialize(&buffer);

                if (result is ISerializeMessageReceiver receiver)
                {
                    receiver.OnAfterDeserialize();
                }
            }

            return result;
        }

        /// <summary>
        /// 将指定的缓冲区中的数据反序列化成<typeparamref name="T"/>类型对象实例
        /// </summary>
        /// <param name="buffer">反序列化缓冲区</param>
        /// <returns>反序列化的对象实例</returns>
        public static T Deserialize(InputBuffer* buffer)
        {
            if (buffer == null)
            {
                return default;
            }

            T result = s_CachedProxy.Deserialize(buffer);

            if (result is ISerializeMessageReceiver receiver)
            {
                receiver.OnAfterDeserialize();
            }

            return result;
        }
    }
}
