using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIKillNotify : MonoBehaviour
    {
        public TextWrapper textKillNotify;
        public UIScoreManager uiScoreManager;
        public string formatKillNotify = "{0} kill {1} ({2})";
        public float showDuration = 3f;
        private float timeCount;
        public static Dictionary<string, int> LeaderBoard = new Dictionary<string, int>();
        MatchEventMapInfo matchEventMapInfo;


        private void Awake()
        {
            //Hides Leaderboard if not Event Arena or Event doesnt use leaderboard;
            if (BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo)
            {
                matchEventMapInfo = BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo;

                if (matchEventMapInfo.DisplayLeaderBoard)
                    uiScoreManager.PlayerScoreUI.Show();

                if (matchEventMapInfo.DisplayTeamLeaderBoard)
                    uiScoreManager.TeamScoreUI.Show();

                if (matchEventMapInfo.DisplayPlayerKDABoard)
                    uiScoreManager.uiPlayerKDADialog.canBeOpened();
            }
            else
            {
                uiScoreManager.PlayerScoreUI.Hide();
                uiScoreManager.TeamScoreUI.Hide();
            }
            textKillNotify.gameObject.SetActive(false);
        }

        private void Start()
        {
            BaseGameNetworkManager.Singleton.onKillNotify += KillNotify;
        }

        private void OnDestroy()
        {
            BaseGameNetworkManager.Singleton.onKillNotify -= KillNotify;
        }

        private void Update()
        {
            timeCount += Time.deltaTime;
            if (timeCount >= showDuration)
                textKillNotify.gameObject.SetActive(false);
        }

        public void KillNotify(string killerName, int teamid, string victimName, int weaponId, int skillId, short skillLevel)
        {
            if (!GameInstance.Items.ContainsKey(weaponId) || !(BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo))
                return;

            timeCount = 0;
            textKillNotify.text = string.Format(formatKillNotify, killerName, victimName, GameInstance.Items[weaponId].Title);
            textKillNotify.gameObject.SetActive(true);

            if (LeaderBoard.ContainsKey(killerName))
                LeaderBoard[killerName]++;
            else
                LeaderBoard.Add(killerName, 1);

            if (uiScoreManager && matchEventMapInfo.DisplayLeaderBoard)
                uiScoreManager.UpdateLeaderBoard(LeaderBoard);

            if (uiScoreManager && matchEventMapInfo.DisplayTeamLeaderBoard)
                uiScoreManager.updateTeamScoreBoard(teamid);

            if (uiScoreManager && matchEventMapInfo.DisplayPlayerKDABoard)
                uiScoreManager.updatePlayerKDABoard(killerName, victimName);
        }
    }
}
