using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Tournament Condition", menuName = "Tournament/Tournament Condition", order = -5575)]
    public class TournamentConditions : ScriptableObject
    {
        public TournamentMapInfo mapInfo;

        public bool CheckEventTournament()
        {
            foreach (TournamentMapInfo tournament in GameInstance.Singleton.Tournaments.Keys)
            {
                if (tournament == mapInfo && !tournament.finished && mapInfo.IsOn)
                    return true;
            }
            return false;
        }
    }
}
