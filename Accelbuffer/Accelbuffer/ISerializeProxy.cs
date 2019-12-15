namespace Accelbuffer
{
    /// <summary>
    /// 实现接口完成<typeparamref name="T"/>类型对象的序列化代理
    /// </summary>
    /// <typeparam name="T">指定序列化的类型</typeparam>
    public unsafe interface ISerializeProxy<T>
    {
        /// <summary>
        /// 方法用于实现<typeparamref name="T"/>类型对象的序列化
        /// </summary>
        /// <param name="obj">将被序列化的对象（RO）</param>
        /// <param name="buffer">序列化缓冲区</param>
        void Serialize(in T obj, in OutputBuffer* buffer);

        /// <summary>
        /// 方法用于实现<typeparamref name="T"/>类型对象的反序列化
        /// </summary>
        /// <param name="buffer">反序列化缓冲区</param>
        /// <returns>反序列化对象</returns>
        T Deserialize(in InputBuffer* buffer);
    }
}
