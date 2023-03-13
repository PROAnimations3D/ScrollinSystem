using System;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Tournament Map Info", menuName = "Tournament/Tournament MapInfo", order = -5501)]
    public partial class TournamentMapInfo : BaseMapInfo
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

        [Category("Tournament Settings")]
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
        public int minLevel;
        public int maxLevel;
        public int registerLimit;
        public float readyDuration;
        [Header("Ring Position")]
        public TournamentPosition[] positions;
        [Tooltip("Event alert")]
        public int[] alerTimes;

        [Header("Rewarding")]
        public string winMailTitle;
        public string winMailContent;
        public string killRewardMailTitle;
        public string killRewardMailContent;
        public TournamentWinner winnerReward;
        public TournamentReward[] killRewards;
        [HideInInspector]
        public bool finished = false;

        [Header("Announce messages")]
        public string eventStartedMessage = "{0} started !!";
        public string eventEndedMessage = "{0} ended !!";
        public string eventAlertMessage = "{0} will begin {1} minutes";

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
            mapName = Id;
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

        protected override bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If this character is summoner so it is ally
                if (targetEntity.HasSummoner)
                {
                    // If summoned by someone, will have same allies with summoner
                    return playerCharacter.IsAlly(targetEntity.Summoner);
                }
                else
                {
                    // Monster always not player's ally
                    return false;
                }
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (targetEntity.Type == EntityTypes.Player)
            {
                return true;
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                // If this character is not summoner so it is enemy
                if (targetEntity.HasSummoner)
                {
                    // If summoned by someone, will have same enemies with summoner
                    return playerCharacter.IsEnemy(targetEntity.Summoner);
                }
                else
                {
                    // Monster always be player's enemy
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public struct TournamentReward
    {
        public int minRank;
        public int maxRank;
        public int winRewardGold;
        public CurrencyAmount[] winRewardCurrencies;
        public ItemAmount[] winRewardItems;
    }
    [Serializable]
    public struct TournamentWinner
    {
        public int winRewardGold;
        public CurrencyAmount[] winRewardCurrencies;
        public ItemAmount[] winRewardItems;
    }
}
