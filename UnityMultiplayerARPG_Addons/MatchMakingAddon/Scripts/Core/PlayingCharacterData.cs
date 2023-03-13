using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct PlayingCharacterData : INetSerializable
    {
        public int dataId;
        public string characterName;
        public int level;
        public int teamID;
        public int kills;
        public int Deaths;


        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            characterName = reader.GetString();
            level = reader.GetPackedInt();
            teamID = reader.GetPackedInt();
            kills = reader.GetPackedInt();
            Deaths = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            writer.Put(characterName);
            writer.PutPackedInt(level);
            writer.PutPackedInt(teamID);
            writer.PutPackedInt(kills);
            writer.PutPackedInt(Deaths);
        }

        public static PlayingCharacterData Create(IPlayerCharacterData character, int kills, int deaths)
        {
            return new PlayingCharacterData()
            {
                dataId = character.DataId,
                characterName = character.CharacterName,
                level = character.Level,
                teamID = character.TeamData.id,
                kills = kills,
                Deaths = deaths,
            };
        }

        public static PlayingCharacterData Create(BasePlayerCharacterEntity character, int kills, int deaths)
        {
            return new PlayingCharacterData()
            {
                dataId = character.DataId,
                characterName = character.CharacterName,
                level = character.Level,
                teamID = character.TeamData.id,
                kills = kills,
                Deaths = deaths,
            };
        }
    }
}
