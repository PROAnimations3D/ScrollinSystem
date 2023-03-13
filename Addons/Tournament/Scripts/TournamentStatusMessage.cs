using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct TournamentStatusMessage : INetSerializable
    {
        public TournamentCharacter[] participants;

        public void Deserialize(NetDataReader reader)
        {
            participants = reader.GetArray<TournamentCharacter>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(participants);
        }
    }

    [System.Serializable]
    public struct TournamentFinishMessage : INetSerializable
    {
        public TournamentFinished[] finishs;

        public void Deserialize(NetDataReader reader)
        {
            finishs = reader.GetArray<TournamentFinished>();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutArray(finishs);
        }
    }
    [System.Serializable]
    public struct TournamentResetMessage : INetSerializable
    {
        public int tournament;

        public void Deserialize(NetDataReader reader)
        {
            tournament = reader.GetPackedInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(tournament);
        }
    }
    [System.Serializable]
    public struct TournamentFinished : INetSerializable
    {
        public int dataId;
        public bool finish;

        public void Deserialize(NetDataReader reader)
        {
            dataId = reader.GetPackedInt();
            finish = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedInt(dataId);
            writer.Put(finish);
        }
        public static TournamentFinished Create(int dataId, bool finish)
        {
            return new TournamentFinished()
            {
                dataId = dataId,
                finish = finish,
            };
        }
    }

    [System.Serializable]
    public struct TournamentCharacter : INetSerializable
    {
        public string userId;
        public string characterId;
        public string characterName;
        public long connectionId;
        public int kills;
        public bool death;
        public int level;
        public int dataId;
        public int rank;
        public bool tournamentIn;

        public void Deserialize(NetDataReader reader)
        {
            userId = reader.GetString();
            characterId = reader.GetString();
            characterName = reader.GetString();
            connectionId = reader.GetPackedLong();
            kills = reader.GetPackedInt();
            death = reader.GetBool();
            rank = reader.GetPackedInt();
            level = reader.GetPackedInt();
            dataId = reader.GetPackedInt();
            tournamentIn = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(userId);
            writer.Put(characterId);
            writer.Put(characterName);
            writer.PutPackedLong(connectionId);
            writer.PutPackedInt(kills);
            writer.Put(death);
            writer.PutPackedInt(rank);
            writer.PutPackedInt(level);
            writer.PutPackedInt(dataId);
            writer.Put(tournamentIn);
        }

        public static TournamentCharacter Create(IPlayerCharacterData character, long connectionId, int kills, bool tournamentIn, int level, int dataId)
        {
            return new TournamentCharacter()
            {
                userId = character.UserId,
                characterId = character.Id,
                characterName = character.CharacterName,
                kills = kills,
                death = false,
                connectionId = connectionId,
                rank = 0,
                level = level,
                dataId = dataId,
                tournamentIn = tournamentIn,
            };
        }
    }
}
