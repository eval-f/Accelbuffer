using System;

namespace Accelbuffer
{
    /// <summary>
    /// 标签不匹配错误
    /// </summary>
    public class TagDismatchException : Exception
    {
        public TagDismatchException() { }

        public TagDismatchException(string message) : base(message) { }

        public TagDismatchException(string message, Exception inner) : base(message, inner) { }

        internal TagDismatchException(IValueTag expectedTag)
            : base($"读取数据是出错，标签{expectedTag.ToErrorMessageString()}不是期待的标签") { }

    }
}
