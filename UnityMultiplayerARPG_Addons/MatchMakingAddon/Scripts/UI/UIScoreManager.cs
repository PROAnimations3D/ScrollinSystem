using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIScoreManager : UIBase
    {

        [Header("Player Leader Board")]
        public UIBase PlayerScoreUI;
        public GameObject Container;
        public UIPlayerScore uIPlayerScore;

        [Header("Team Score")]
        public UITeamScore TeamScoreUI;

        [Header("Player KDA Dialog")]
        public UIPlayerKDADialog uiPlayerKDADialog;

        private void OnEnable()
        {
            BaseGameNetworkManager.Singleton.onReceivePlayersList += uiPlayerKDADialog.FillKDAUI;
            BaseGameNetworkManager.Singleton.onEventMatchFinished += uiPlayerKDADialog.EventEnd;
        }

        private void OnDisable()
        {
            BaseGameNetworkManager.Singleton.onReceivePlayersList -= uiPlayerKDADialog.FillKDAUI;
            BaseGameNetworkManager.Singleton.onEventMatchFinished -= uiPlayerKDADialog.EventEnd;
        }
        public void UpdateLeaderBoard(Dictionary<string, int> LeaderBoard)
        {
            var sortedDict = from entry in LeaderBoard orderby entry.Value descending select entry;

            int counter = 1;
            foreach (var value in sortedDict)
            {
                Container.transform.GetChild(counter - 1).GetComponent<UIPlayerScore>().SetUIPlayerScore(counter, value.Key, value.Value);
                counter++;

                if (counter > 5)
                    break;
            }
        }

        public void updateTeamScoreBoard(int teamid)
        {
            TeamScoreUI.SetUITeamScore(teamid);
        }

        public void updatePlayerKDABoard(string Killername, string Victimname)
        {
            uiPlayerKDADialog.updatePlayerKDAUI(Killername, Victimname);
        }
    }
}
