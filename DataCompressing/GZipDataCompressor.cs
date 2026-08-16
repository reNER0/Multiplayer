using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Assets.Scripts.Network.DataCompressing
{
    public class GZipDataCompressor : IDataCompressor
    {
        public byte[] Compress(byte[] bytes)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress, true))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }

                return output.ToArray();
            }
        }

        public byte[] Decompress(byte[] bytes)
        {
            using (var input = new MemoryStream(bytes))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }

        public string Compress(string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(Compress(bytes));
        }

        public string Decompress(string data)
        {
            var bytes = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(Decompress(bytes));
        }
    }
}
