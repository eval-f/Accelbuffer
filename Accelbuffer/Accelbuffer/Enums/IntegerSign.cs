namespace Accelbuffer
{
    /// <summary>
    /// 整数符号（1位）
    /// </summary>
    internal enum IntegerSign : byte
    {
        /// <summary>
        /// 表示整数为正数或者无符号
        /// </summary>
        PositiveOrUnsigned = 0,

        /// <summary>
        /// 表示整数为负数
        /// </summary>
        Negative = 1
    }
}
