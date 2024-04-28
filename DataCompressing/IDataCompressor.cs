namespace Assets.Scripts.Network.DataCompressing
{
    public interface IDataCompressor
    {
        byte[] Compress(byte[] bytes);
        byte[] Decompress(byte[] bytes);
        string Compress(string data);
        string Decompress(string data);
    }
}
