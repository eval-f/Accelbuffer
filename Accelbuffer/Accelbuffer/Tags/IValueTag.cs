namespace Accelbuffer
{
    /// <summary>
    /// 表示一个数据的标签（1个字节）
    /// </summary>
    internal interface IValueTag 
    {
        /// <summary>
        /// 获取数据的类型
        /// </summary>
        ValueTypeCode TypeCode { get; }

        /// <summary>
        /// 获取标签是否合法
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 获取标签的一个字节表示形式
        /// </summary>
        /// <returns></returns>
        byte GetTagByte();

        /// <summary>
        /// 获取标签的错误信息字符串
        /// </summary>
        /// <returns></returns>
        string ToErrorMessageString();
    }
}
