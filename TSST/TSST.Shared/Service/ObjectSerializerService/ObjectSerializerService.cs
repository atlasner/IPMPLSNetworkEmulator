using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TSST.Shared.Service.ObjectSerializerService
{
    public class ObjectSerializerService : IObjectSerializerService
    {
        public object Deserialize(byte[] arrBytes)
        {
            var memStream = new MemoryStream();
            var binForm = new BinaryFormatter();

            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);

            var obj = binForm.Deserialize(memStream);

            return obj;
        }

        public byte[] Serialize(object package)
        {
            if (package == null)
                return null;

            var bf = new BinaryFormatter();
            var ms = new MemoryStream();

            bf.Serialize(ms, package);

            return ms.ToArray();
        }
    }
}