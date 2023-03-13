﻿using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public struct RequestKickMemberFromPartyMessage : INetSerializable
    {
        public string memberId;

        public void Deserialize(NetDataReader reader)
        {
            memberId = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(memberId);
        }
    }
}
