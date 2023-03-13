using System.Runtime.Serialization;

namespace MultiplayerARPG
{
    public class PlayerDungeonSerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context)
        {
            PlayerDungeon data = (PlayerDungeon)obj;
            info.AddValue("characterId", data.CharacterId);
            info.AddValue("dataId", data.DataId);
            info.AddValue("loginTime", data.LoginTime);
        }

        public object SetObjectData(
            object obj,
            SerializationInfo info,
            StreamingContext context,
            ISurrogateSelector selector)
        {
            PlayerDungeon data = (PlayerDungeon)obj;
            data.CharacterId = info.GetString("characterId");
            data.DataId = info.GetInt32("dataId");
            data.LoginTime = info.GetInt64("loginTime");
            obj = data;
            return obj;
        }
    }
}
