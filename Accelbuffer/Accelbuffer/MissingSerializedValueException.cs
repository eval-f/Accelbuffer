using System;

namespace Accelbuffer
{
    /// <summary>
    /// 指定序列化索引值未找到错误
    /// </summary>
    public class MissingSerializedValueException : Exception
    {
        public MissingSerializedValueException() { }

        public MissingSerializedValueException(string message) : base(message) { }

        public MissingSerializedValueException(string message, Exception inner) : base(message, inner) { }

        public MissingSerializedValueException(byte index) : base($"序列化错误，未找到序列化索引为{index.ToString()}的值") { }
    }
}
