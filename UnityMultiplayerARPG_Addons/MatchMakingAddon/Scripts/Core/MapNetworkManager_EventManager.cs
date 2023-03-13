using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public sealed partial class MapNetworkManager
    {
        /// <summary>
        /// Override Warp Character to Instance in MMO Mode.
        /// </summary>
        /// <param name="playerCharacterEntities"></param>
        /// <param name="instanceid"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        public override void WarpCharacterToEventInstance(List<IPlayerCharacterData> playerCharacterEntities, string instanceid, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if UNITY_EDITOR || UNITY_SERVER
            HashSet<uint> instanceMapWarpingCharacters = new HashSet<uint>();

            foreach (BasePlayerCharacterEntity basePlayerCharacter in playerCharacterEntities)
            {
                if (!CanWarpCharacter(basePlayerCharacter))
                    continue;

                instanceMapWarpingCharacters.Add(basePlayerCharacter.ObjectId);
                basePlayerCharacter.IsWarping = true;
            }

            instanceMapWarpingCharactersByInstanceId.TryAdd(instanceid, instanceMapWarpingCharacters);
            instanceMapWarpingLocations.TryAdd(instanceid, new InstanceMapWarpingLocation()
            {
                mapName = mapName,
                position = position,
                overrideRotation = overrideRotation,
                rotation = rotation,
            });
            ClusterClient.SendRequest(MMORequestTypes.RequestSpawnMap, new RequestSpawnMapMessage()
            {
                mapId = mapName,
                instanceId = instanceid,
                instanceWarpPosition = position,
                instanceWarpOverrideRotation = overrideRotation,
                instanceWarpRotation = rotation,
            }, responseDelegate: (responseHandler, responseCode, response) => OnRequestSpawnMap(responseHandler, responseCode, response, instanceid), millisecondsTimeout: mapSpawnMillisecondsTimeout);
#endif
        }
        
        /// <summary>
        /// Override Send SystemNotice in MMO Mode.
        /// </summary>
        /// <param name="systemNoticeType"></param>
        /// <param name="matchEvents"></param>
        /// <param name="Timer"></param>
        public override void SendSystemNotice(SystemNoticeType systemNoticeType, MatchEvents matchEvents, int Timer)
        {
#if UNITY_EDITOR || UNITY_SERVER
            switch (systemNoticeType)
            {
                case SystemNoticeType.Alert:

                    if (CurrentMapInfo is MatchEventHubMapInfo)
                    {
                        MatchEventHubMapInfo mapInfo = CurrentMapInfo as MatchEventHubMapInfo;
                        if (mapInfo.matchEvent == matchEvents)
                        {
                            ServerSendSystemAnnounce(matchEvents.title + " will begin in " + Timer + " minutes");
                        }
                    }
                    break;
                case SystemNoticeType.Started:

                    if (CurrentMapInfo is MatchEventHubMapInfo)
                    {
                        MatchEventHubMapInfo mapInfo = CurrentMapInfo as MatchEventHubMapInfo;
                        if (mapInfo.matchEvent == matchEvents)
                        {
                            ServerSendSystemAnnounce(matchEvents.eventStartedMessage);
                        }
                    }
                    break;
                case SystemNoticeType.Ended:

                    if (CurrentMapInfo is MatchEventHubMapInfo)
                    {
                        MatchEventHubMapInfo mapInfo = CurrentMapInfo as MatchEventHubMapInfo;
                        if (mapInfo.matchEvent == matchEvents)
                        {
                            ServerSendSystemAnnounce(matchEvents.eventEndedMessage);
                        }
                    }
                    break;
            }
#endif
        }

        public override void UpdatePlayerKillsTable(string killerName, string victimName, int weaponId)
        {
#if UNITY_EDITOR || UNITY_SERVER

#endif

        }
    }
}
