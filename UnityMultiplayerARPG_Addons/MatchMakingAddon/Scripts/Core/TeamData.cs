using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MultiplayerARPG
{
    [System.Serializable]
    public struct TeamData : INetSerializable
    {
        public int id;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
        }
    }

    [System.Serializable]
    public class SyncFieldTeamData : LiteNetLibSyncField<TeamData>
    {
        protected override bool IsValueChanged(TeamData newValue)
        {
            return true;
        }
    }
}
