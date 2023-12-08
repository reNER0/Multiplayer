using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network
{
    public interface IDataCompressor
    {
        byte[] Compress(byte[] bytes);
        byte[] Decompress(byte[] bytes);
        string Compress(string data);
        string Decompress(string data);
    }
}
