using System;

namespace Accelbuffer
{
    /// <summary>
    /// 指示字段参与序列化
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class SerializedValueAttribute : Attribute
    {
        /// <summary>
        /// 获取 值的序列化索引
        /// </summary>
        public byte SerializeIndex { get; }

        /// <summary>
        /// 初始化 SerializedValueAttribute 实例
        /// </summary>
        /// <param name="serializeIndex">值的序列化索引</param>
        public SerializedValueAttribute(byte serializeIndex)
        {
            SerializeIndex = serializeIndex;
        }
    }
}
