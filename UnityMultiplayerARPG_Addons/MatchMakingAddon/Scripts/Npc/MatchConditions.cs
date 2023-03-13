using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Match Condition", menuName = "Create GameData/CalleGaming/Event/Match Condition", order = -4792)]
    public class MatchConditions : ScriptableObject
    {
        [Header("Event Type")]
        public MatchEvents matchEvent;
        public bool CheckEvent()
        {
            switch (matchEvent.matchType)
            {
                case MatchType.LastManStanding:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
                case MatchType.BattleArena:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
                case MatchType.TeamDeathMatch:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
                case MatchType.KillConfirmed:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
                case MatchType.Domination:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
                case MatchType.Survival:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
                case MatchType.CaptureTheFlag:
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchTimers.Keys)
                        if (matchEvents.starting && matchEvents == matchEvent)
                            return true;
                    return false;
            }
            return false;
        }

        public void UsedOnlyForAOTCodeGeneration()
        {
            // YOUR CODE HERE
            CheckEvent();
            // Include an exception so we can be sure to know if this method is ever called.
            throw new InvalidOperationException(@"This method is used for AOT code generation only. 
    Do not call it at runtime.");
        }
    }
}
