namespace Accelbuffer
{
    /// <summary>
    /// 数据类型码（3位）
    /// </summary>
    internal enum ValueTypeCode : byte
    {
        /// <summary>
        /// 8位整数
        /// </summary>
        Integer_8b = 0,

        /// <summary>
        /// 16位整数
        /// </summary>
        Integer_16b = 1,

        /// <summary>
        /// 32位整数
        /// </summary>
        Integer_32b = 2,

        /// <summary>
        /// 64位整数
        /// </summary>
        Integer_64b = 3,

        /// <summary>
        /// 布尔值(true or false)
        /// </summary>
        Boolean = 4,

        /// <summary>
        /// 字符值
        /// </summary>
        Char = 5,

        /// <summary>
        /// 32位浮点数
        /// </summary>
        Float_32b = 6,

        /// <summary>
        /// 64位浮点数
        /// </summary>
        Float_64b = 7
    }
}
