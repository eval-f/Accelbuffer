namespace Accelbuffer
{
    /// <summary>
    /// 数字符号（1位）
    /// </summary>
    internal enum NumberSign : byte
    {
        /// <summary>
        /// 表示数字为正数或者无符号
        /// </summary>
        PositiveOrUnsigned = 0,

        /// <summary>
        /// 表示数字为负数
        /// </summary>
        Negative = 1
    }
}
