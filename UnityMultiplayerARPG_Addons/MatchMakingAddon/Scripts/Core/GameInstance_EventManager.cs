using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance
    {
        [Header("Gameplay Events")]
        ///Events list.
        [SerializeField]
        [Header("Timed Events")]
        private List<MatchEvents> matchEvents;

        [SerializeField]
        [Header("Always active Events")]
        private List<MatchEvents> runningEvents;
        /// <summary>
        /// Registered Events + start times.
        /// </summary>
        public Dictionary<MatchEvents, DateTime> MatchTimers = new Dictionary<MatchEvents, DateTime>();

        /// <summary>
        /// Events list Getter.
        /// </summary>
        public List<MatchEvents> MatchEvents
        {
            get { return matchEvents; }
        }

        public List<MatchEvents> RunningEvents
        {
            get { return runningEvents; }
        }
        /// <summary>
        /// Awake Extentions
        /// </summary>
        /// DevExtMethod that is called when on Gameinstance Awake.
        /// These setups the MatchTimers List.
        [DevExtMethods("Awake")]
        public virtual void SetEventsList()
        {
            foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchEvents)
            {
                ///if RunTestEvents is t rue, run Debug mode for event times.
                if (matchEvents.RunTestEvents)
                {
                    DateTime time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                    MatchTimers.Add(matchEvents, time.AddMinutes(matchEvents.Interval));
                }
                else
                    SetEventTimer(matchEvents);

            }
        }

        /// <summary>
        /// Event Time Setter.
        /// </summary>
        /// Update the current event with the time of the next match.
        /// <param name="matchEvents"></param>
        public virtual void SetEventTimer(MatchEvents matchEvents)
        {
            if (matchEvents.GetNextEventDate(DateTime.Now) != -1)
            {
                DateTime dateTime = DateTime.Now.AddDays(matchEvents.GetNextEventDate(DateTime.Now));

                if (matchEvents.TimeOfEvent(dateTime) > DateTime.Now)
                    if (!MatchTimers.ContainsKey(matchEvents))
                        MatchTimers.Add(matchEvents, matchEvents.TimeOfEvent(dateTime));
                    else
                        MatchTimers[matchEvents] = matchEvents.TimeOfEvent(dateTime);
            }
        }
    }
}
