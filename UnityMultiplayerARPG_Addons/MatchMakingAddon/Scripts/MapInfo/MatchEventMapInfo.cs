using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Mach Event Map Info", menuName = "Create GameData/CalleGaming/Event/Match Map Info/Match Event Map Info", order = -4799)]
    public partial class MatchEventMapInfo : BaseMapInfo
    {
        [Header("Event Settingss")]
        public MatchEvents matchEvent;
        public bool DisplayLeaderBoard;
        public bool DisplayTeamLeaderBoard;
        public bool DisplayPlayerKDABoard;
        public bool AutomaticTeleportOnDeath;
        public bool RespawnInEvent;
        public bool OverrideStartPosition;
        public EventOverrideRespawnPoints[] eventOverrideStartPoints;

        [System.NonSerialized]
        private Dictionary<int, List<EventOverrideRespawnPoints>> cacheEventRespawnPoints;
        public Dictionary<int, List<EventOverrideRespawnPoints>> CacheEventRespawnPoints
        {
            get
            {
                if (cacheEventRespawnPoints == null)
                {
                    cacheEventRespawnPoints = new Dictionary<int, List<EventOverrideRespawnPoints>>();
                    int factionDataId;
                    foreach (EventOverrideRespawnPoints overrideRespawnPoint in eventOverrideStartPoints)
                    {
                        factionDataId = 0;
                        if (overrideRespawnPoint.forTeam != null)
                            factionDataId = overrideRespawnPoint.forTeam.DataId;
                        if (!cacheEventRespawnPoints.ContainsKey(factionDataId))
                            cacheEventRespawnPoints.Add(factionDataId, new List<EventOverrideRespawnPoints>());
                        cacheEventRespawnPoints[factionDataId].Add(overrideRespawnPoint);
                    }
                }
                return cacheEventRespawnPoints;
            }
        }
        public override bool SaveCurrentMapPosition { get { return false; } }

        public override bool AutoRespawnWhenDead { get { return AutomaticTeleportOnDeath; } }

        public override void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out WarpPortalType portalType, out string mapName, out Vector3 position, out bool overrideRotation, out Vector3 rotation)
        {
            base.GetRespawnPoint(playerCharacterData, out portalType, out mapName, out position, out overrideRotation, out rotation);
            if (RespawnInEvent)
            {
                List<EventOverrideRespawnPoints> overrideRespawnPoints;
                if (CacheEventRespawnPoints.TryGetValue(playerCharacterData.TeamData.id, out overrideRespawnPoints) ||
                    CacheEventRespawnPoints.TryGetValue(0, out overrideRespawnPoints))
                {
                    EventOverrideRespawnPoints overrideRespawnPoint = overrideRespawnPoints[Random.Range(0, overrideRespawnPoints.Count)];
                    mapName = null;
                    position = overrideRespawnPoint.respawnPosition;
                }
            }
        }

        public Vector3 GetStartMapPosition(Team team)
        {
            List<EventOverrideRespawnPoints> overrideRespawnPoints;
            if (CacheEventRespawnPoints.TryGetValue(team.DataId, out overrideRespawnPoints) ||
                CacheEventRespawnPoints.TryGetValue(0, out overrideRespawnPoints))
            {
                EventOverrideRespawnPoints overrideRespawnPoint = overrideRespawnPoints[Random.Range(0, overrideRespawnPoints.Count)];
                return overrideRespawnPoint.respawnPosition;
            }
            return StartPosition;
        }

        public Vector3 GetStartMapPosition(int teamId)
        {
            List<EventOverrideRespawnPoints> overrideRespawnPoints;
            if (CacheEventRespawnPoints.TryGetValue(teamId, out overrideRespawnPoints) ||
                CacheEventRespawnPoints.TryGetValue(0, out overrideRespawnPoints))
            {
                EventOverrideRespawnPoints overrideRespawnPoint = overrideRespawnPoints[Random.Range(0, overrideRespawnPoints.Count)];
                return overrideRespawnPoint.respawnPosition;
            }
            return StartPosition;
        }

        protected override bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntity)
        {
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            if (monsterCharacter.IsSummoned)
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

            if (monsterCharacter.IsSummoned)
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

            ///If any of the given match types, return false. this matches are solo
            if (matchEvent.matchType == MatchType.LastManStanding 
                || matchEvent.matchType == MatchType.BattleArena
                || matchEvent.matchType == MatchType.KillConfirmed)
                return false;

            ///Try get if player is team.
            if (targetEntity.Type == EntityTypes.Player)
            {
                BasePlayerCharacterEntity Target = null;
                if (targetEntity.TryGetEntity(out Target))
                    return playerCharacter.TeamData.id == Target.TeamData.id;
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                /// If this character is summoner so it is ally
                if (targetEntity.HasSummoner)
                {
                    ///If summoned by someone, will have same allies with summoner
                    return playerCharacter.IsAlly(targetEntity.Summoner);
                }
                else
                {
                    /// Monster always not player's ally
                    return false;
                }
            }

            return false;
        }

        protected override bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntity)
        {   
            if (string.IsNullOrEmpty(targetEntity.Id))
                return false;

            ///If any of the given match types, return True. this matches are solo
            if (matchEvent.matchType == MatchType.LastManStanding
                || matchEvent.matchType == MatchType.BattleArena
                || matchEvent.matchType == MatchType.KillConfirmed)
                return true;

            ///Try get if player is Enemy.
            if (targetEntity.Type == EntityTypes.Player)
            {
                BasePlayerCharacterEntity Target = null;
                if (targetEntity.TryGetEntity(out Target))
                    return playerCharacter.TeamData.id != Target.TeamData.id;
            }

            if (targetEntity.Type == EntityTypes.Monster)
            {
                /// If this character is summoner so it is ally
                if (targetEntity.HasSummoner)
                {
                    /// If summoned by someone, will have same allies with summoner
                    return playerCharacter.IsEnemy(targetEntity.Summoner);
                }
                else
                {
                    /// Monster always not player's ally
                    return true;
                }
            }

            return false;
        }
    }

    [System.Serializable]
    public struct EventOverrideRespawnPoints
    {
        [Tooltip("If this is not empty, character who have the same faction will respawn to this point")]
        public Team forTeam;
        public Vector3 respawnPosition;
    }
}
