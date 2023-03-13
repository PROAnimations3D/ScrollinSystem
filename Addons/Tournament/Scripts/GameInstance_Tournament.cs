using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Tournaments Addons")]
        public List<TournamentMapInfo> tournaments;

        public Dictionary<TournamentMapInfo, DateTime> Tournaments = new Dictionary<TournamentMapInfo, DateTime>();


        [DevExtMethods("Awake")]
        private void InitTournaments()
        {
            foreach (TournamentMapInfo info in tournaments)
            {
                if (!Tournaments.ContainsKey(info))
                    Tournaments[info] = info.StartTime(DateTime.Now);
            }
        }
    }
}
