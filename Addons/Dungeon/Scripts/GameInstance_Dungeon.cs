using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Dungeon Addons")]
        public List<DungeonMapInfo> dungeons;

        public Dictionary<DungeonMapInfo, DateTime> Dungeons = new Dictionary<DungeonMapInfo, DateTime>();


        [DevExtMethods("Awake")]
        private void InitDungeons()
        {
            foreach (DungeonMapInfo info in dungeons)
            {
                if(!Dungeons.ContainsKey(info))
                Dungeons[info] = info.StartTime(DateTime.Now);
            }
        }
    }
}
