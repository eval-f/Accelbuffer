using System.Collections.Generic;

namespace Accelbuffer
{
    internal sealed class PrimitiveTypeSerializeProxy<T> : ISerializeProxy<T>, ISerializeProxy<T[]>, ISerializeProxy<List<T>> where T : unmanaged
    {
        unsafe T ISerializeProxy<T>.Deserialize(in InputBuffer* buffer)
        {
            T result;

            buffer->ReadBytes((byte*)&result, sizeof(T));

            return result;
        }

        unsafe void ISerializeProxy<T>.Serialize(in T obj, in OutputBuffer* buffer)
        {
            T value = obj;

            buffer->WriteBytes((byte*)&value, sizeof(T));
        }

        unsafe T[] ISerializeProxy<T[]>.Deserialize(in InputBuffer* buffer)
        {
            int len = buffer->ReadInt32();
            T[] result = new T[len];

            fixed (T* ptr = result)
            {
                T* p = ptr;

                while (len > 0)
                {
                    buffer->ReadBytes((byte*)p++, sizeof(T));
                    len--;
                }
            }

            return result;
        }

        unsafe void ISerializeProxy<T[]>.Serialize(in T[] obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(obj.Length);

            fixed (T* ptr = obj)
            {
                T* p = ptr;
                int count = obj.Length;

                while (count > 0)
                {
                    buffer->WriteBytes((byte*)p++, sizeof(T));
                    count--;
                }
            }
        }

        unsafe void ISerializeProxy<List<T>>.Serialize(in List<T> obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(obj.Count);

            for (int i = 0; i < obj.Count; i++)
            {
                T value = obj[i];
                buffer->WriteBytes((byte*)&value, sizeof(T));
            }
        }

        unsafe List<T> ISerializeProxy<List<T>>.Deserialize(in InputBuffer* buffer)
        {
            int len = buffer->ReadInt32();
            List<T> result = new List<T>(len);

            for (int i = 0; i < len; i++)
            {
                T value;
                buffer->ReadBytes((byte*)&value, sizeof(T));
                result[i] = value;
            }

            return result;
        }
    }
}
