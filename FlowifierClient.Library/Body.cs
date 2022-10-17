using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowifierClient
{
    internal class Body
    {
        public enum ContentTypes
        {
            JSON,
            BSON
        }

        public ContentTypes ContentType { get; private set; }
        public string JSON { get; private set; }
        public byte[] BSON { get; private set; }

        private Body(string jsonString)
        {
            ContentType = ContentTypes.JSON;
            JSON = jsonString;
            BSON = new byte[0];
        }

        private Body(byte[] bsonBytes)
        {
            ContentType = ContentTypes.BSON;
            JSON = "";
            BSON = bsonBytes;
        }

        public static Body ObjectToJSONString(JObject jObj)
        {
            return new Body(jObj.ToString(Formatting.None));
        }

        public static Body ObjectToBSONBody(JObject jObj)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonDataWriter writer = new BsonDataWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, jObj);
            }

            return new Body(ms.ToArray());
        }
    }
}
