using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UITournamentRanks : UIBase
    {
        public int maxShow;
        public float repaetUI;
        public UITournamentRanksItem uiPrefab;
        public Transform uiContainer;

        private float countDown;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiPrefab.gameObject;
                    cacheList.uiContainer = uiContainer;
                }
                return cacheList;
            }
        }

        private void Update()
        {
            countDown -= Time.unscaledDeltaTime;
            if (countDown <= 0)
            {
                Setup();
                countDown = repaetUI;
            }
        }

        private void Setup()
        {
            List<TournamentCharacter> ranks = BaseGameNetworkManager.Singleton.GetTournamentRanks();
            List<TournamentCharacter> list = new List<TournamentCharacter>();

            int rank = 0;
            foreach(TournamentCharacter item in ranks)
            {
                rank++;
                var tempData = item;
                tempData.rank = rank;
                if(tempData.rank <= maxShow)
                {
                    list.Add(tempData);
                }
            }
            UITournamentRanksItem tempUI;
            CacheList.Generate(list, (index, resp, ui) =>
            {
                tempUI = ui.GetComponent<UITournamentRanksItem>();
                tempUI.Data = resp;
                tempUI.Show();
            });
        }

    }
}
