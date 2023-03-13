using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public static partial class PlayerCharacterDataExtensions
    {

        [DevExtMethods("CloneTo")]
        public static IPlayerCharacterData CloneTournament(IPlayerCharacterData from, IPlayerCharacterData to)
        {
            to.TournamentGM = from.TournamentGM;
            return to;
        }
        [DevExtMethods("SerializeCharacterData")]
        public static void SerializeCharacterDataTournament(IPlayerCharacterData characterData, NetDataWriter writer)
        {
            writer.PutPackedInt(characterData.TournamentGM);
        }
        [DevExtMethods("DeserializeCharacterData")]
        public static void DeserializeCharacterDataTournament(IPlayerCharacterData characterData, NetDataReader reader)
        {
            characterData.TournamentGM = reader.GetPackedInt();
        }

        public static bool TournamentGM(this IPlayerCharacterData data)
        {
            return data.TournamentGM > 0;
        }
    }
}
