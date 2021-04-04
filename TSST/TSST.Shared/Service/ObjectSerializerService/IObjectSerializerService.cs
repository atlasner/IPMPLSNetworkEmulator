namespace TSST.Shared.Service.ObjectSerializerService
{
    public interface IObjectSerializerService
    {
        byte[] Serialize(object package);
        object Deserialize(byte[] arrBytes);
    }
}