using System;

namespace Accelbuffer
{
    /// <summary>
    /// 标签不匹配错误
    /// </summary>
    public sealed class TagDismatchException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        public TagDismatchException() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public TagDismatchException(string message) : base(message) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="inner"></param>
        public TagDismatchException(string message, Exception inner) : base(message, inner) { }

        internal TagDismatchException(ValueTypeCode expected, ValueTypeCode serialized)
            : base($"数据类型匹配错误，无法将{serialized.ToString()}转换为{expected.ToString()}") { }

        internal TagDismatchException(CharEncoding expected, CharEncoding serialized)
            : base($"字符编码匹配错误，无法将{serialized.ToString()}转换为{expected.ToString()}") { }

        internal TagDismatchException(CharValueType expected, CharValueType serialized)
            : base($"字符类型匹配错误，无法将{serialized.ToString()}转换为{expected.ToString()}") { }

        internal TagDismatchException(int expectedByteCount, int serializedByteCount)
            : base($"整数长度匹配错误，无法将{(serializedByteCount << 3).ToString()}位整数转换为{(expectedByteCount << 3).ToString()}位整数") { }

        internal TagDismatchException(IntegerSign expected, IntegerSign serialized)
            : base($"整数符号匹配错误，无法将{serialized.ToString()}的整数转换为{expected.ToString()}的整数") { }
    }
}
