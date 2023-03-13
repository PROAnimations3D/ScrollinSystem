using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseGameNetworkManager
    {
        [Header("Kill notify")]
        public ushort killNotifyMessageId = 2000;
        /// <summary>
        /// Killer Name, Victim Name, Weapon ID, Skill ID, Skill Level
        /// </summary>
        public System.Action<string,int, string, int, int, short> onKillNotify;
        [DevExtMethods("RegisterClientMessages")]
        public void RegisterClientMessages_KillNotify()
        {
            RegisterClientMessage(killNotifyMessageId, (messageHandler) =>
            {
                var killerName = messageHandler.Reader.GetString();
                var teamid = messageHandler.Reader.GetInt();
                var victimName = messageHandler.Reader.GetString();
                var weaponId = messageHandler.Reader.GetInt();
                var skillId = messageHandler.Reader.GetInt();
                var skillLevel = messageHandler.Reader.GetShort();

                if (onKillNotify != null)
                    onKillNotify.Invoke(killerName, teamid, victimName, weaponId, skillId, skillLevel);
            });
        }

        public void SendKillNotify(string killerName,int teamid, string victimName, int weaponId, int skillId, int skillLevel)
        {
            if (!IsServer)
                return;

            //Increase Killer kill count by 1;
            PlayingCharacterData killerData = PlayersKDA[killerName];
            killerData.kills++;

            //Increase Victim death count by 1;
            PlayingCharacterData VictimData = PlayersKDA[victimName];
            VictimData.Deaths++;

            PlayersKDA[killerName] = killerData;
            PlayersKDA[victimName] = VictimData;

            Invoke(nameof(LastPlayerCheck), 2);

            UpdatePlayerKillsTable(killerName, victimName, weaponId);

            List<IPlayerCharacterData> playerCharacters = new List<IPlayerCharacterData>(ServerUserHandlers.GetPlayerCharacters());

            foreach (IPlayerCharacterData playerCharacterData in playerCharacters)
            {
                long connectionId;
                ServerUserHandlers.TryGetConnectionIdByName(playerCharacterData.CharacterName, out connectionId);

                ServerSendPacket(connectionId, 0, LiteNetLib.DeliveryMethod.Sequenced, killNotifyMessageId, (writer) =>
                {
                    writer.Put(killerName);
                    writer.Put(teamid);
                    writer.Put(victimName);
                    writer.Put(weaponId);
                    writer.Put(skillId);
                    writer.Put(skillLevel);
                });
            }
        }

        public virtual void UpdatePlayerKillsTable(string killerName, string victimName, int weaponId)
        {

        }
    }
}
