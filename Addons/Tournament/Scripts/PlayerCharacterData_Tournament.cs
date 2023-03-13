using LiteNetLib.Utils;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        public int TournamentGM
        {
            get { return tournamentGM.Value; }
            set { tournamentGM.Value = value; }
        }
    }
    public partial class PlayerCharacterData
    {
        public int TournamentGM { get; set; }
    }
    public partial interface IPlayerCharacterData : ICharacterData
    {
        int TournamentGM { get; set; }
    }
}
