namespace Assets.Scripts.Network.DataCompressing
{
    public class BypassDataCompressor : IDataCompressor
    {
        public byte[] Compress(byte[] bytes)
        {
            return bytes;
        }

        public byte[] Decompress(byte[] bytes)
        {
            return bytes;
        }


        public string Compress(string data)
        {
            return data;
        }

        public string Decompress(string data)
        {
            return data;
        }
    }
}
