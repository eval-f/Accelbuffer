using System;

namespace Accelbuffer
{
    /// <summary>
    /// 指示字段作为动态长度的整数被序列化，该特性只对 <see cref="byte"/>, <see cref="sbyte"/>, <see cref="ushort"/>, <see cref="short"/>, <see cref="uint"/>, <see cref="int"/>, <see cref="ulong"/>, <see cref="long"/> 类型的字段有效
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class VariableIntegerAttribute : Attribute { }
}
