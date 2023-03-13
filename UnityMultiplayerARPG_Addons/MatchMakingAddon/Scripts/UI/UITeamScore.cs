using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UITeamScore : UIBase
    {
        [Header("UI Elements")]
        public TextWrapper EventNameText;

        [Header("Team Elements")]
        public UITeams[] uITeams;

        public static Dictionary<Team, int> TeamScores = new Dictionary<Team, int>();

        private void Start()
        {
            if (!(BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo))
                return;


            MatchEventMapInfo matchEventMap = BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo;

            foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchEvents)
                if (matchEvents == matchEventMap.matchEvent)
                    EventNameText.text = matchEvents.title;

            foreach (UITeams teams in uITeams)
            {
                TeamScores.Add(teams.Team, 0);
                teams.TeamNameText.text = teams.Team.Title;
                teams.TeamScoreText.text = TeamScores[teams.Team].ToString();
                
            }
        }

        public void SetUITeamScore(int teamid)
        {
            foreach (UITeams teams in uITeams)
            {
                if (teams.Team.DataId == teamid)
                {
                    TeamScores[teams.Team]++;

                    teams.TeamScoreText.text = TeamScores[teams.Team].ToString();
                }
            }
        }
    }


    [System.Serializable]
    public struct UITeams
    {
        public Team Team;
        public TextWrapper TeamNameText;
        public TextWrapper TeamScoreText;
    }
}
