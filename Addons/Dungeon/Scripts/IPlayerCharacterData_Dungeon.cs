using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IPlayerCharacterData
    {
        IList<PlayerDungeon> Dungeons { get; set; }
    }
}
