using System.Collections.Generic;

namespace Accelbuffer
{
    internal sealed class StringSerializeProxy : ISerializeProxy<string>, ISerializeProxy<string[]>, ISerializeProxy<List<string>>
    {
        unsafe string ISerializeProxy<string>.Deserialize(in InputBuffer* buffer)
        {
            return buffer->ReadString(CharEncoding.Unicode);
        }

        unsafe string[] ISerializeProxy<string[]>.Deserialize(in InputBuffer* buffer)
        {
            int len = buffer->ReadInt32();
            string[] result = new string[len];

            for (int i = 0; i < len; i++)
            {
                result[i] = buffer->ReadString(CharEncoding.Unicode);
            }

            return result;
        }

        unsafe List<string> ISerializeProxy<List<string>>.Deserialize(in InputBuffer* buffer)
        {
            int len = buffer->ReadInt32();
            List<string> result = new List<string>(len);

            for (int i = 0; i < len; i++)
            {
                result[i] = buffer->ReadString(CharEncoding.Unicode);
            }

            return result;
        }

        unsafe void ISerializeProxy<string>.Serialize(in string obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(obj, CharEncoding.Unicode);
        }

        unsafe void ISerializeProxy<string[]>.Serialize(in string[] obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(obj.Length);

            for (int i = 0; i < obj.Length; i++)
            {
                buffer->WriteValue(obj[i], CharEncoding.Unicode);
            }
        }

        unsafe void ISerializeProxy<List<string>>.Serialize(in List<string> obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(obj.Count);

            for (int i = 0; i < obj.Count; i++)
            {
                buffer->WriteValue(obj[i], CharEncoding.Unicode);
            }
        }
    }
}
