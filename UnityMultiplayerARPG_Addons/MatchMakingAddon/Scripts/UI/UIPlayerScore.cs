using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIPlayerScore : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextWrapper PlayerRankText;
        public TextWrapper PlayerNameText;
        public TextWrapper PlayerScoreText;
        
        public void SetUIPlayerScore(int playerRank, string playername, int playerScore)
        {
            PlayerRankText.text = playerRank.ToString();
            PlayerNameText.text = playername;
            PlayerScoreText.text = playerScore.ToString();
        }
    }
}
