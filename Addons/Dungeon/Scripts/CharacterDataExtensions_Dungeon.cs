using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static partial class CharacterDataExtensions
    {
        public static List<PlayerDungeon> Clone(this IList<PlayerDungeon> src)
        {
            List<PlayerDungeon> result = new List<PlayerDungeon>();
            for (int i = 0; i < src.Count; ++i)
            {
                result.Add(src[i].Clone());
            }
            return result;
        }
    }
}
