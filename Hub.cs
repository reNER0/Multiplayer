using Assets.Scripts.Network.Commands;
using Assets.Scripts.Network.DataCompressing;
using System;
using UnityEngine;

namespace Assets.Scripts.Network
{
    public class Hub : MonoBehaviour
    {
        [SerializeField]
        protected int _port = 25565;


        protected static readonly Type[] publicCommandTypes = {
            typeof(SpawnCmd),
        };


        private IDataCompressor compressor = new BypassDataCompressor();


        protected string CommandToString(ICommand cmd)
        {
            var data = JsonUtility.ToJson(cmd);

            var compressedData = compressor.Compress(data);

            return compressedData;
        }

        protected ICommand StringToCommand(string msg)
        {
            var decompressedData = compressor.Decompress(msg);

            SerializableClass ctype = JsonUtility.FromJson<SerializableClass>(decompressedData);
            Type t = Type.GetType(ctype.ClassName);
            ICommand gc = (ICommand)JsonUtility.FromJson(decompressedData, t);
            return gc;
        }
    }
}
