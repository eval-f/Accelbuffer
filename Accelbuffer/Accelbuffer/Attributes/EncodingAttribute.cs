using System;

namespace Accelbuffer
{
    /// <summary>
    /// 指示字段的编码，该特性只对 <see cref="char"/> 和 <see cref="string"/> 类型的字段有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class EncodingAttribute : Attribute
    {
        /// <summary>
        /// 获取字符的编码
        /// </summary>
        public CharEncoding Encoding { get; }

        public EncodingAttribute(CharEncoding encoding)
        {
            Encoding = encoding;
        }
    }
}
