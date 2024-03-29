﻿namespace Accelbuffer
{
    /// <summary>
    /// 数据类型码（3位）
    /// </summary>
    internal enum ValueTypeCode : byte
    {
        /// <summary>
        /// 动态长度的整数(0-64b)
        /// </summary>
        VariableInteger = 0,

        /// <summary>
        /// 固定长度的整数(0b, 8b, 16b, 32b, 64b)
        /// </summary>
        FixedInteger = 1,

        /// <summary>
        /// 动态长度的浮点数(0-64b)
        /// </summary>
        VariableFloat = 2,

        /// <summary>
        /// 固定长度的浮点数(0b, 32b, 64b)
        /// </summary>
        FixedFloat = 3,

        /// <summary>
        /// 布尔值(true or false)
        /// </summary>
        Boolean = 4,

        /// <summary>
        /// 字符值(char or string)
        /// </summary>
        Char = 5
    }
}
