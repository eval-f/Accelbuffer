namespace Accelbuffer
{
    /// <summary>
    /// 字符数据的类型（1位）
    /// </summary>
    internal enum CharValueType : byte
    {
        /// <summary>
        /// 表示字符类型是单个字符
        /// </summary>
        SingleChar = 0,

        /// <summary>
        /// 表示字符类型是字符串
        /// </summary>
        String = 1
    }
}
