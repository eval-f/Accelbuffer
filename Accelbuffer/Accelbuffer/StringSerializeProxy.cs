using System.Collections.Generic;

namespace Accelbuffer
{
    internal sealed class StringSerializeProxy : ISerializeProxy<string>, ISerializeProxy<string[]>, ISerializeProxy<List<string>>
    {
        unsafe string ISerializeProxy<string>.Deserialize(in InputBuffer* buffer)
        {
            return buffer->ReadString(0, CharEncoding.Unicode);
        }

        unsafe string[] ISerializeProxy<string[]>.Deserialize(in InputBuffer* buffer)
        {
            int len = buffer->ReadVariableInt32(0);
            string[] result = new string[len];

            for (int i = 0; i < len; i++)
            {
                result[i] = buffer->ReadString(0, CharEncoding.Unicode);
            }

            return result;
        }

        unsafe List<string> ISerializeProxy<List<string>>.Deserialize(in InputBuffer* buffer)
        {
            int len = buffer->ReadVariableInt32(0);
            List<string> result = new List<string>(len);

            for (int i = 0; i < len; i++)
            {
                result[i] = buffer->ReadString(0, CharEncoding.Unicode);
            }

            return result;
        }

        unsafe void ISerializeProxy<string>.Serialize(in string obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(0, obj, CharEncoding.Unicode);
        }

        unsafe void ISerializeProxy<string[]>.Serialize(in string[] obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(0, obj.Length, false);

            for (int i = 0; i < obj.Length; i++)
            {
                buffer->WriteValue(0, obj[i], CharEncoding.Unicode);
            }
        }

        unsafe void ISerializeProxy<List<string>>.Serialize(in List<string> obj, in OutputBuffer* buffer)
        {
            buffer->WriteValue(0, obj.Count, false);

            for (int i = 0; i < obj.Count; i++)
            {
                buffer->WriteValue(0, obj[i], CharEncoding.Unicode);
            }
        }
    }
}
