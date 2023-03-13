using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterData
    {
        private List<PlayerDungeon> dungeons = new List<PlayerDungeon>();
        public IList<PlayerDungeon> Dungeons
        {
            get { return dungeons; }
            set
            {
                dungeons = new List<PlayerDungeon>();
                dungeons.AddRange(value);
            }
        }
    }
}