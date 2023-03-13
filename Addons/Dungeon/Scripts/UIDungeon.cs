using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cysharp.Text;

namespace MultiplayerARPG
{
    public class UIDungeon : UIBase
    {
        public GameObject[] uiObjs;
        public GameObject[] hiddingObjs;
        public GameObject bgBossTitle;
        public TextWrapper textBossTitle;
        public TextWrapper textCountDown;
        public TextWrapper textTarget;
        public GameObject uiStatus;
        public UILocaleKeySetting formatKeyDungeonBoss = new UILocaleKeySetting(UIFormatKeys.UI_CUSTOM);
        public UILocaleKeySetting formatKeyDungeonMonster = new UILocaleKeySetting(UIFormatKeys.UI_CUSTOM);


        private Dictionary<MonsterCharacter, int> target = new Dictionary<MonsterCharacter, int>();

        private float countDownBoss;
        private bool bossKilled;
        private void Start()
        {
            BaseMapInfo CurrentMapInfo = GameInstance.PlayingCharacterEntity.CurrentMapInfo;
            bool isDungeon = CurrentMapInfo != null && (CurrentMapInfo is DungeonMapInfo);

            if (uiObjs != null && uiObjs.Length > 0)
            {
                foreach (var obj in uiObjs)
                {
                    obj.SetActive(isDungeon);
                }
            }
            textCountDown.SetGameObjectActive(isDungeon);
            target.Clear();
            
        }

        public void BossFightUI(float duration)
        {
            BaseMapInfo CurrentMapInfo = GameInstance.PlayingCharacterEntity.CurrentMapInfo;
            DungeonMapInfo dungeonMap = CurrentMapInfo as DungeonMapInfo;
            countDownBoss = duration;
            bossKilled = true;
            textBossTitle.text = dungeonMap.DungeonBoss.Title;
            textBossTitle.SetGameObjectActive(true);
            bgBossTitle.SetActive(true);
            HideUIS(false);
            DungeonBossCamera bossCamera = FindObjectOfType<DungeonBossCamera>();
            bossCamera.uiCamera.gameObject.SetActive(true);
        }

        private void Update()
        {
            BaseMapInfo CurrentMapInfo = GameInstance.PlayingCharacterEntity.CurrentMapInfo;
            if(CurrentMapInfo != null && (CurrentMapInfo is DungeonMapInfo))
            {
                float timer = GameInstance.PlayingCharacterEntity.CurrentGameManager.BossTime;
                int minutes = Mathf.FloorToInt(timer / 60F);
                int seconds = Mathf.FloorToInt(timer - minutes * 60);
                string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
                textCountDown.text = niceTime;
                countDownBoss -= Time.unscaledDeltaTime;
                if(bossKilled && countDownBoss <= 0f)
                {
                    bossKilled = false;
                    textBossTitle.SetGameObjectActive(false);
                    bgBossTitle.SetActive(false);
                    HideUIS(true);
                    DungeonBossCamera bossCamera = FindObjectOfType<DungeonBossCamera>();
                    if(bossCamera != null)
                    bossCamera.uiCamera.gameObject.SetActive(false);
                }
                Status();
            }
        }

        public void Status()
        {
            DungeonDoorEntity[] doors = FindObjectsOfType<DungeonDoorEntity>();
            string doorId = GameInstance.PlayingCharacterEntity.dungeonStatus;
            using(Utf16ValueStringBuilder output = ZString.CreateStringBuilder(false))
            {
                if(doorId == "BOSS")
                {
                    DungeonMapInfo mapInfo = GameInstance.PlayingCharacterEntity.CurrentMapInfo as DungeonMapInfo;
                    output.AppendFormat(LanguageManager.GetText(formatKeyDungeonBoss), mapInfo.DungeonBoss.Title);
                }
                else
                {
                    foreach (var item in doors)
                    {
                        if (item == null)
                            continue;
                        if (item.doorId == doorId)
                        {
                            foreach (var itm in item.targets)
                            {
                                if (output.Length > 0)
                                    output.Append('\n');
                                output.AppendFormat(
                                    LanguageManager.GetText(formatKeyDungeonMonster),
                                    itm.monster.Title,
                                    itm.currentMonster,
                                    itm.targetMonster);
                            }
                        }
                    }
                }
                if (textTarget != null)
                {
                    textTarget.text = output.ToString();
                }
                if(uiStatus != null)
                {
                    uiStatus.SetActive(!string.IsNullOrEmpty(output.ToString()));
                }
            }
        }

        private void HideUIS(bool hidding)
        {
            foreach (GameObject obj in hiddingObjs)
            {
                if (obj == null)
                    continue;
                obj.SetActive(hidding);
            }
        }
    }
}
