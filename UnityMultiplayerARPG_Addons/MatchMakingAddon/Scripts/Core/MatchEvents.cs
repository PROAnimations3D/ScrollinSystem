using MarkupAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Match Event", menuName = "Create GameData/CalleGaming/Event/Match Event", order = -4795)]
    public partial class MatchEvents : ScriptableObject
    {
        [Foldout("General Information")]
        [Header("Title")]
        public string title;
        public string Description;

        [Foldout("Match General Settings")]
        [Header("Event Type")]
        public MatchType matchType;
        public bool Consistent;
        public WarpPortalType warpPortalType = WarpPortalType.EnterInstance;
        public bool SplitInTeams;
        public int AmountPlayersPerInstance;
        public int MinimumPlayersInInstance;
        [Header("Expel Players Delay in seconds")]
        public int ExpelDelay = 15;
        public Team[] Teams;

        [Foldout("Reward options")]
        [TabScope("Reward options/Tabscope", "On Kill|On Win",true)]
        [Tab("./On Kill")]
        public Reward rewardsPerKill;
        [Tab("../On Win")]
        public Reward rewardsOnWin;

        [Foldout("Categories")]
        [Header("Event Level Restrictions")]
        public List<LevelRange> levelRanges;

        [Foldout("Days of the Week")]
        [TabScope("Days of the Week/Tabscope2", "Sunday|Monday|Tuesday|Wednesday|Thursday|Friday|Saturday",true)]
        [Tab("./Sunday")]
        public EventManagerTime sunday;
        [Tab("../Monday")]
        public EventManagerTime monday;
        [Tab("../Tuesday")]
        public EventManagerTime tuesday;
        [Tab("../Wednesday")]
        public EventManagerTime wednesday;
        [Tab("../Thursday")]
        public EventManagerTime thursday;
        [Tab("../Friday")]
        public EventManagerTime friday;
        [Tab("../Saturday")]
        public EventManagerTime saturday;


        

        [Foldout("Announcements Settings")]
        [Header("Announce messages")]
        public bool alertAheadOfTime;
        public int[] alertTimer;
        public string eventStartedMessage = "Event has started !!";
        public bool sendEndingMessage;
        public string eventEndedMessage = "Event has ended !!";

        [Header("Global Event Winner Announcement")]
        public bool GlobalAnnouncement;
        [Tooltip("Winner Name / Team will come infront of the Msg")]
        public string GlobalAnnounceMsg = "Has Won the match";

        [Foldout("Debug + Partials")]
        [Header("Debugger")]
        public bool RunTestEvents;
        [Tooltip("Set Minutes Interval between each Test Event")]
        public int Interval;
        /// <summary>
        /// Used on Client side to check if event should be displayed in npc dialog
        /// </summary>
        /// Server sends Message to client with Event status, Status will update this variable.
        /// The Update variable will in npc conditions.
        [HideInInspector]
        public bool starting = false;

        /// <summary>
        /// Checks which date there is a event scheduled and returns the date.
        /// </summary>
        /// Cycles through the Weekdays to check if there an event active on a specified day.
        /// If doesnt find on that day, checks the next one till there is one.
        /// If none is found returns -1, this means there is no date for this event and will be skipped.
        public int GetNextEventDate(DateTime dateTime)
        {
            int AddDays = -1;
            DateTime dateNow = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
            EventManagerTime eventTime = GetEventDate(dateNow);

            for (int i = 0; i < 8; i++)
            {
                if (eventTime.Active)
                {
                    if (TimeOfEvent(dateNow) > DateTime.Now)
                    {
                        AddDays = i;
                        break;
                    }
                }
                dateNow = dateNow.AddDays(1);
                eventTime = GetEventDate(dateNow);
            }
            return AddDays;
        }
        /// <summary>
        /// Checks which time there is a event scheduled and returns the time.
        /// </summary>
        /// Checks the given date of the event, and checks if the event is a one time event or a repeatable event.
        /// if event is onetime event, it will check for the time.
        /// If event is repeatable. it will check the time of day, and return when the next event time will be.
        public DateTime TimeOfEvent(DateTime dateTime)
        {
            EventManagerTime eventTime = GetEventDate(dateTime);

            if (eventTime.OneTime)
                return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, eventTime.startHour, eventTime.starMinutes, 0);
            else
            {
                    DateTime DateEvent = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, eventTime.DelayedStart, 0, 0);

                    while (DateEvent < DateTime.Now && GetEventDate(DateEvent).Active)
                        DateEvent = DateEvent.AddHours(eventTime.RepeatRateHour);

                    return DateEvent;
            }
        }
        /// <summary>
        /// Returns eventData of a specified date.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public EventManagerTime GetEventDate(DateTime dateTime)
        {
            EventManagerTime eventTime;
            switch (dateTime.DayOfWeek)
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
    }
    [Serializable]
    public struct EventManagerTime
    {
        [Header("Active Event")]
        public bool Active;

        [Header("One Time Event")]
        public bool OneTime;
        [Tooltip("-1 = every hour.")]
        [Range(0, 23)]
        public int startHour;
        [Range(0, 59)]
        public int starMinutes;

        [Header("Repeatable Event")]
        [Tooltip("To Delay event at start of day")]
        [Range(0, 12)]
        public int DelayedStart;
        [Tooltip("Number Indicates The amount of Hours between events")]
        [Range(1, 12)]
        public int RepeatRateHour;

        [Header("Event Duration")]
        public int EventDuration;
    }

    [Serializable]
    public struct LevelRange
    {
        public MatchEventMapInfo matchEventMapInfo;
        public float MinimumLevel;
        public float MaximumLevel;
    }
}
