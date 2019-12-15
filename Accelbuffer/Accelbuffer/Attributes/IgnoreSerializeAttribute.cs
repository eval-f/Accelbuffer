using System;

namespace Accelbuffer
{
    /// <summary>
    /// 指示字段不参与序列化
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class IgnoreSerializeAttribute : Attribute { }
}
