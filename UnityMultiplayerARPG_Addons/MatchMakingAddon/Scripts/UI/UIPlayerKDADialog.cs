using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class UIPlayerKDADialog : UIBase
    {
        [Header("UI Elements")]
        public TextWrapper uiEventTextTitle;
        public UITeamKDA[] uITeamKDA;
        public UIPlayerKDA uiplayerKDAPrefab;
        public Button LeaveButton;
        bool abletoOpen = false;


        private void Start()
        {
            if (!(BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo))
                return;


            MatchEventMapInfo matchEventMap = BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo;

            foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchEvents)
                if (matchEvents == matchEventMap.matchEvent)
                    uiEventTextTitle.text = matchEvents.title;

            foreach (UITeamKDA teams in uITeamKDA)
            {
                teams.uiTextTitle.text = teams.Team.Title;
            }
        }

        public override void Show()
        {
            if (abletoOpen)
                base.Show();
            GameInstance.PlayingCharacterEntity.CallServerPlayersInEvent();
        }

        public override void Hide()
        {
            if (abletoOpen)
                base.Hide();
        }
        public void EventEnd(int teamid)
        {
            Show();
            foreach (UITeamKDA teamuis in uITeamKDA)
            {
                if (teamuis.Team.DataId == teamid)
                    teamuis.uiWinText.gameObject.SetActive(true);
            }

            LeaveButton.gameObject.SetActive(true);
            abletoOpen = false;
        }

        public void OnClickLeave()
        {
            GameInstance.PlayingCharacterEntity.CallServerWarpPlayer();
        }

        public void FillKDAUI(PlayingCharacterDataMessage playingCharacters)
        {
            if (BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo)
            {
                MatchEventMapInfo matchEventMapInfo = BaseGameNetworkManager.CurrentMapInfo as MatchEventMapInfo;

                if (matchEventMapInfo.DisplayPlayerKDABoard)
                {

                    MatchEvents match = null;
                    foreach (MatchEvents matchEvents in GameInstance.Singleton.MatchEvents)
                        if (matchEvents == matchEventMapInfo)
                            match = matchEvents;

                    foreach (UITeamKDA teamKDA in uITeamKDA)
                    {
                        foreach (Transform child in teamKDA.Container.transform)
                        {
                            Destroy(child.gameObject);
                        }
                    }

                    foreach (PlayingCharacterData playerCharacter in playingCharacters.playingCharacterDatas)
                    {

                        foreach (UITeamKDA teamKDA in uITeamKDA)
                        {
                            if (teamKDA.Team.DataId == playerCharacter.teamID)
                            {
                                uiplayerKDAPrefab.SetUIPlayerKDA(playerCharacter.characterName, playerCharacter.kills, playerCharacter.Deaths);
                                Instantiate(uiplayerKDAPrefab, teamKDA.Container.transform);
                                if (match != null)
                                    teamKDA.uiTextTeamNumbers.text = (teamKDA.Container.transform.childCount + " / " + match.AmountPlayersPerInstance / 2).ToString();
                            }
                        }
                    }
                }
            }
        }
        public void updatePlayerKDAUI(string Killername, string Victimname)
        {
            GameInstance.PlayingCharacterEntity.CallServerPlayersInEvent();
        }

        public void canBeOpened()
        {
            abletoOpen = true;
        }

    }

    [System.Serializable]
    public struct UITeamKDA
    {
        public Team Team;
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextTeamNumbers;
        public TextWrapper uiWinText;
        public GameObject Container;
    }
}
