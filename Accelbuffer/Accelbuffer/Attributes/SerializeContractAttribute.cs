using System;

namespace Accelbuffer
{
    /// <summary>
    /// 表示 类型/结构/接口 序列化的协议
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class SerializeContractAttribute : Attribute
    {
        /// <summary>
        /// 获取/设置 初始的序列化缓冲区大小，以字节为单位
        /// </summary>
        public long InitialBufferSize { get; set; }

        /// <summary>
        /// 获取/设置 是否使用严格序列化模式（开启对序列化索引不匹配的错误警告）
        /// </summary>
        public bool StrictMode { get; set; }

        /// <summary>
        /// 获取 序列化代理类型
        /// </summary>
        public Type ProxyType { get; }

        /// <summary>
        /// 初始化 SerializeContractAttribute 实例，并指示运行时自动注入序列化代理
        /// </summary>
        public SerializeContractAttribute()
        {
            ProxyType = null;
        }

        /// <summary>
        /// 初始化 SerializeContractAttribute 实例，并指示运行时使用<paramref name="proxyType"/>类型作为序列化代理
        /// </summary>
        /// <param name="proxyType">序列化代理类型</param>
        public SerializeContractAttribute(Type proxyType)
        {
            ProxyType = proxyType;
        }
    }
}
