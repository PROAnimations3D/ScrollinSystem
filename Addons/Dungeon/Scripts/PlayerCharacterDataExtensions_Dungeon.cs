using LiteNetLib.Utils;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MultiplayerARPG
{
    public static partial class PlayerCharacterDataExtensions
    {
        [DevExtMethods("AddAllCharacterRelatesDataSurrogate")]
        public static void AddAllCharacterRelatesDataSurrogateDungeon(SurrogateSelector surrogateSelector)
        {
            PlayerDungeonSerializationSurrogate dungeonSS = new PlayerDungeonSerializationSurrogate();
            surrogateSelector.AddSurrogate(typeof(PlayerDungeon), new StreamingContext(StreamingContextStates.All), dungeonSS);
        }

        [DevExtMethods("CloneTo")]
        public static IPlayerCharacterData CloneDungeon(IPlayerCharacterData from, IPlayerCharacterData to)
        {
            to.Dungeons = from.Dungeons.Clone();
            return to;
        }
        [DevExtMethods("SerializeCharacterData")]
        public static void SerializeCharacterDataDungeon(IPlayerCharacterData characterData, NetDataWriter writer)
        {
            writer.Put((short)characterData.Dungeons.Count);
            foreach (PlayerDungeon entry in characterData.Dungeons)
            {
                writer.Put(entry);
            }
        }
        [DevExtMethods("DeserializeCharacterData")]
        public static void DeserializeCharacterDataDungeon(IPlayerCharacterData characterData, NetDataReader reader)
        {
            int count = reader.GetShort();
            for (int i = 0; i < count; ++i)
            {
                characterData.Dungeons.Add(reader.Get<PlayerDungeon>());
            }
        }
    }
}
