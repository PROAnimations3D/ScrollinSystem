using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIPlayerKDA : MonoBehaviour
    {
        public TextWrapper playerNameText;
        public TextWrapper uiTextKills;
        public TextWrapper uiTextDeaths;
        public TextWrapper uiTextKDA;

        public void SetUIPlayerKDA(string name, int kills, int deaths)
        {
            playerNameText.text = name;
            uiTextKills.text = kills.ToString();
            uiTextDeaths.text = deaths.ToString();
            if (kills > 0 && deaths > 0)
                uiTextKDA.text = ((float)kills / (float)deaths).ToString();
            else if (kills > 0 && deaths <= 0)
                uiTextKDA.text = (kills).ToString();
            else if (kills <= 0 && deaths > 0)
                uiTextKDA.text = "0";
            else if (kills <= 0 && deaths <=0)
                uiTextKDA.text = "0";
        }
    }
}
