using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Mach Event Hub Map Info", menuName = "Create GameData/CalleGaming/Event/Match Map Info/Match Event Hub Map Info", order = -4799)]
    public partial class MatchEventHubMapInfo : MapInfo
    {
        public MatchEvents matchEvent;

        [Header("Event Settingss")]
        public bool ConsistentEvent;
        public override bool SaveCurrentMapPosition { get { return false; } }

        public override bool AutoRespawnWhenDead { get { return true; } }
    }
}
