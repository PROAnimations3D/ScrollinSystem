using UnityEngine;
using UnityEngine.UI;
namespace MultiplayerARPG
{
    public class UITournamentRanksItem : UISelectionEntry<TournamentCharacter>
    {
        [Header("UI")]
        public TextWrapper textTitle;
        public TextWrapper textKills;
        public TextWrapper textClass;
        public TextWrapper textRank;
        public Image image;
        public Color defaultColor;
        public Color loserColor;
        protected override void UpdateData()
        {
            if (textTitle != null)
            {
                textTitle.text = Data.characterName;
            }
            if (textRank != null)
            {
                textRank.text = Data.rank.ToString();
            }

            if(textKills != null)
            {
                textKills.text = Data.kills.ToString();
            }
            if(image != null)
            {
                if (Data.death)
                    image.color = loserColor;
                else
                    image.color = defaultColor;
            }

            if(textClass != null)
            {
                PlayerCharacter player;
                if(GameInstance.PlayerCharacters.TryGetValue(Data.dataId, out player))
                {
                    textClass.text = player.Title;
                }
            }
        }
    }
}
