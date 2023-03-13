using System;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Dungeon Map Info", menuName = "Dungeon/Dungeon MapInfo", order = -5500)]
    public partial class DungeonMapInfo : BaseMapInfo
    {
        [Serializable]
        public struct EventTime
        {
            public bool isOn;
            [Range(0, 23)]
            public int startHour;
            [Range(0, 59)]
            public int startMinutes;
            [Range(0, 23)]
            public int endHour;
            [Range(0, 59)]
            public int endMinutes;
        }

        [Category("Dungeon Settings")]
        [Header("Event time settings")]
        public EventTime sunday;
        public EventTime monday;
        public EventTime tuesday;
        public EventTime wednesday;
        public EventTime thursday;
        public EventTime friday;
        public EventTime saturday;
        [Header("Generally Settings")]
        public string eventTitle;
        [Min(1)]
        [Tooltip("Daily login limit")]
        public int limitLogin;
        public int minLevel;
        public int maxLevel;
        [Min(1)]
        [Tooltip("battle duration miniutes")]
        public float battleDuration;
        [Tooltip("player teleporting town seconds")]
        public float warpToTown;
        [Tooltip("Event alert")]
        public int[] alerTimes;

        public MonsterCharacter dungeonBoss;

        public MonsterCharacter DungeonBoss
        {
            get { return dungeonBoss; }
        }

        [Header("Announce messages")]
        public string eventKilledBossReturnTown = "seconds to teleporting town";
        public string eventFinishBattleDuration = "duration finish players warping town";
        public string eventStartedMessage = "event started !!";
        public string eventEndedMessage = "event ended !!";
        public string eventAlertMessage = "event will begin {0}  {1}minutes";
        public string eventDoorDestroyed = "{0} destroyed";
        public string eventBossKilledParty = "<color=green>{0}</color> and party <color=white>{1}</color> succes boss killed";
        public string eventBossKilledSolo = "<color=green>{0}</color> <color=white>{1}</color> succes boss killed";

        public bool IsOn
        {
            get
            {
                EventTime eventTime;
                switch (DateTime.Now.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        eventTime = sunday;
                        break;
                    case DayOfWeek.Monday:
                        eventTime = monday;
                        break;
                    case DayOfWeek.Tuesday:
                        eventTime = tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        eventTime = wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        eventTime = thursday;
                        break;
                    case DayOfWeek.Friday:
                        eventTime = friday;
                        break;
                    case DayOfWeek.Saturday:
                        eventTime = saturday;
                        break;
                    default:
                        eventTime = sunday;
                        break;
                }
                return eventTime.isOn && (DateTime.Now > StartTime(DateTime.Now)) && (DateTime.Now < EndTime(DateTime.Now));
            }
        }

        public EventTime GetDateTime(DateTime time)
        {
            EventTime eventTime;
                switch (time.DayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        eventTime = sunday;
                        break;
                    case DayOfWeek.Monday:
                        eventTime = monday;
                        break;
                    case DayOfWeek.Tuesday:
                        eventTime = tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        eventTime = wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        eventTime = thursday;
                        break;
                    case DayOfWeek.Friday:
                        eventTime = friday;
                        break;
                    case DayOfWeek.Saturday:
                        eventTime = saturday;
                        break;
                    default:
                        eventTime = sunday;
                        break;
                }
                return eventTime;
        }

        public DateTime StartTime(DateTime dateTime)
        {
            EventTime eventTime = GetDateTime(dateTime);
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, eventTime.startHour, eventTime.startMinutes, 0);
        }

        public DateTime EndTime(DateTime dateTime)
        {
            EventTime eventTime = GetDateTime(dateTime);
            DateTime value = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, eventTime.endHour, eventTime.endMinutes, 0);
            return value;
        }

        public override bool AutoRespawnWhenDead { get { return false; } }
        public override bool SaveCurrentMapPosition { get { return false; } }

        public override void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out WarpPortalType portalType, out string mapName, out Vector3 position, out bool overrideRotation, out Vector3 rotation)
        {
            base.GetRespawnPoint(playerCharacterData, out portalType, out mapName, out position, out overrideRotation, out rotation);
            mapName = null;
            position = StartPosition;
        }

        protected override bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter.IsSummonedAndSummonerExisted)
            {
                // If summoned by someone, will have same allies with summoner
                return targetEntity.Id.Equals(monsterCharacter.Summoner.Id) || monsterCharacter.Summoner.IsAlly(targetEntity);
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If another monster has same allyId so it is ally
                if (targetEntity.HasSummoner)
                    return monsterCharacter.IsAlly(targetEntity.Summoner);
                return GameInstance.MonsterCharacters[targetEntity.DataId].AllyId == monsterCharacter.CharacterDatabase.AllyId;
            }

            return false;
        }

        protected override bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter.IsSummonedAndSummonerExisted)
            {
                // If summoned by someone, will have same enemies with summoner
                return targetEntity.Id.Equals(monsterCharacter.Summoner.Id) && monsterCharacter.Summoner.IsEnemy(targetEntity);
            }

            // Attack only player by default
            return targetEntity.Type == EntityTypes.Player;
        }

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntityInfo)
        {
            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntityInfo)
        {
            return false;
        }
    }
}
