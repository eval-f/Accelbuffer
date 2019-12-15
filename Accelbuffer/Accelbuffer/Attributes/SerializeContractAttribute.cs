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
        /// 获取/设置 默认的序列化缓冲区大小，以字节为单位
        /// </summary>
        public long DefaultBufferSize { get; set; }

        /// <summary>
        /// 获取/设置 是否忽略私有字段
        /// </summary>
        public bool IgnoreNonPublicField { get; set; }

        /// <summary>
        /// 获取 序列化代理类型
        /// </summary>
        public Type ProxyType { get; }

        public SerializeContractAttribute()
        {
            ProxyType = null;
        }

        public SerializeContractAttribute(Type proxyType)
        {
            ProxyType = proxyType;
        }
    }
}
