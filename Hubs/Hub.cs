using Assets.Scripts.Network.Commands;
using Assets.Scripts.Network.DataCompressing;
using System;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class Hub : MonoBehaviour
    {
        protected static readonly Type[] publicCommandTypes = {
            typeof(SpawnCmd),
            typeof(DestroyCmd),
            typeof(ChatMessageCmd),
        };


        private const char PlainMessagePrefix = 'J';
        private const char CompressedMessagePrefix = 'G';
        private const int CompressionThresholdBytes = 256;

        private readonly IDataCompressor compressor = new GZipDataCompressor();


        protected string CommandToString(ICommand cmd)
        {
            var data = JsonUtility.ToJson(cmd);
            var dataSize = Encoding.UTF8.GetByteCount(data);

            if (dataSize < CompressionThresholdBytes)
                return PlainMessagePrefix + data;

            var compressedData = compressor.Compress(data);

            // Base64 can make small or poorly compressible messages larger.
            if (Encoding.UTF8.GetByteCount(compressedData) >= dataSize)
                return PlainMessagePrefix + data;

            return CompressedMessagePrefix + compressedData;
        }

        protected ICommand StringToCommand(string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg))
                    return null;

                string decompressedData;
                switch (msg[0])
                {
                    case CompressedMessagePrefix:
                        decompressedData = compressor.Decompress(msg.Substring(1));
                        break;
                    case PlainMessagePrefix:
                        decompressedData = msg.Substring(1);
                        break;
                    default:
                        // Allows local tools and older unsuffixed JSON messages.
                        decompressedData = msg;
                        break;
                }

                SerializableClass ctype = JsonUtility.FromJson<SerializableClass>(decompressedData);
                Type t = Type.GetType(ctype.ClassName);
                ICommand gc = (ICommand)JsonUtility.FromJson(decompressedData, t);
                return gc;
            }
            catch (Exception e)
            {
                Debug.LogError("Error while parsing command! " + e.Message);
                return null;
            }
        }
    }
}
