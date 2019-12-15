namespace Accelbuffer
{
    /// <summary>
    /// 实现接口以获取序列化消息回调，对于值类型，该操作会导致装箱
    /// </summary>
    public interface ISerializeMessageReceiver
    {
        /// <summary>
        /// 方法会在序列化前调用
        /// </summary>
        void OnBeforeSerialize();

        /// <summary>
        /// 方法会在反序列化后调用
        /// </summary>
        void OnAfterDeserialize();
    }
}
