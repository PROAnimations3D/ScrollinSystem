using UnityEngine;

namespace MultiplayerARPG
{
    public class UITournamentManager : MonoBehaviour
    {
        public TextWrapper uiRegisterCount;
        public TextWrapper uiParticipantCount;
        public TextWrapper uiFightCountdown;
        public GameObject[] uiMapObj;
        public GameObject[] uiGMObjs;

        private void Update()
        {
            BasePlayerCharacterEntity owningCharacter = GameInstance.PlayingCharacterEntity;

            if(owningCharacter != null)
            {
                if (!(owningCharacter.CurrentMapInfo as TournamentMapInfo))
                    return;

                TournamentMapInfo mapInfo = owningCharacter.CurrentMapInfo as TournamentMapInfo;

                foreach(var item in uiMapObj)
                {
                    if (item == null)
                        continue;
                    item.SetActive(mapInfo != null);
                }

                if(uiFightCountdown != null)
                {
                    float countDown = BaseGameNetworkManager.Singleton.TournamentFightCountDown;
                    uiFightCountdown.text = countDown.ToString();
                    uiFightCountdown.SetGameObjectActive(countDown > 0);
                }

                foreach(var item in uiGMObjs)
                {
                    if (item == null)
                        continue;

                    item.SetActive(owningCharacter.TournamentGM());
                }
                if(uiRegisterCount != null)
                {
                    uiRegisterCount.SetGameObjectActive(owningCharacter.TournamentGM());
                    uiRegisterCount.text = owningCharacter.CurrentGameManager.RegisteredCount + "/" + mapInfo.registerLimit;
                }

                if(uiParticipantCount != null)
                {
                    uiParticipantCount.SetGameObjectActive(owningCharacter.TournamentGM());
                    uiParticipantCount.text = owningCharacter.CurrentGameManager.LastPlayersTournament().ToString();
                }
            }
        }
    }
}
