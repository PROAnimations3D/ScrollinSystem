using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public partial class PlayerDungeon : INetSerializable
    {
        public static readonly PlayerDungeon Empty = new PlayerDungeon();
        public string CharacterId { get; set; }
        public int DataId { get; set; }
        public long LoginTime { get; set; }

        public PlayerDungeon Clone()
        {
            PlayerDungeon clone = new PlayerDungeon();
            clone.CharacterId = CharacterId;
            clone.DataId = DataId;
            clone.LoginTime = LoginTime;
            return clone;
        }

        public static PlayerDungeon Create(string characterId, int dataId)
        {
            System.DateTime currentDate = System.DateTime.UtcNow.Date;
            return new PlayerDungeon()
            {
                CharacterId = characterId,
                DataId = dataId,
                LoginTime = ((System.DateTimeOffset)currentDate).ToUnixTimeSeconds(),
            };
        }
        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);
            writer.Put(DataId);
            writer.PutPackedLong(LoginTime);
        }

        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();
            DataId = reader.GetInt();
            LoginTime = reader.GetPackedLong();
        }
    }
    [System.Serializable]
    public class SyncListCharacterDungeon : LiteNetLibSyncList<PlayerDungeon>
    {
    }
}
