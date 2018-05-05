using System.Text;
using Newtonsoft.Json;
using PurePusher.Interfaces;

namespace PurePusherClientTest
{
    public class MySerializer : ISerializer
    {
	    public T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);

	    // this is slow and the default serializer does not require the conversion to byte array
	    // but to make this work with newtonsoft we sacrafice some speed
		// json is always UTF8
	    public byte[] Serialize(object obj) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
    }
}
