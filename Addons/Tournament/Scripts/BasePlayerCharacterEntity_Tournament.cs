using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity
    {
        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            tournamentGM.deliveryMethod = DeliveryMethod.ReliableOrdered;
            tournamentGM.syncMode = LiteNetLibSyncField.SyncMode.ServerToOwnerClient;
        }

        public override bool CanUseSkill()
        {
            if ((CurrentMapInfo as TournamentMapInfo))
                return CurrentGameManager.CheckTournamentFighting(Id) && CurrentGameManager.TournamentFightReady();

            return base.CanUseSkill();
        }
        public override bool CanJump()
        {
            if ((CurrentMapInfo as TournamentMapInfo))
                return false;

            return base.CanJump();
        }

        public override bool CanUseItem()
        {
            if ((CurrentMapInfo as TournamentMapInfo))
                return CurrentGameManager.CheckTournamentFighting(Id) && CurrentGameManager.TournamentFightReady();

            return base.CanUseItem();
        }

        public override bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if ((CurrentMapInfo as TournamentMapInfo))
                return CurrentGameManager.CheckTournamentFighting(Id) && CurrentGameManager.TournamentFightReady();

            return base.CanReceiveDamageFrom(instigator);
        }
    }
    public partial class BasePlayerCharacterEntity
    {
        protected SyncFieldInt tournamentGM = new SyncFieldInt();

        [DevExtMethods("Awake")]
        protected void Awake_Tournament()
        {
            onReceivedDamage += OnReceivedDamage_Tournament;
        }

        [DevExtMethods("OnDestroy")]
        protected void OnDestroy_Tournament()
        {
            onReceivedDamage -= OnReceivedDamage_Tournament;
        }

        public void TournamentTargetClear()
        {
            RPC(ServerTournamentTargetClear);
        }
        [AllRpc]
        private void ServerTournamentTargetClear()
        {
            PlayerCharacterController controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
            if(controller != null)
            controller.ClearTournament();
        }
        private void OnReceivedDamage_Tournament(HitBoxPosition position, Vector3 fromPosition, IGameEntity attacker, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime)
        {
            if (!this.IsDead() && !(CurrentMapInfo as TournamentMapInfo))
                return;

            BasePlayerCharacterEntity playerAttacker = null;
            BaseMonsterCharacterEntity monsterCharacterEntity;
            if (attacker.Entity is BasePlayerCharacterEntity)
                playerAttacker = attacker.Entity as BasePlayerCharacterEntity;

            if (attacker.Entity is BaseMonsterCharacterEntity)
            {
                monsterCharacterEntity = attacker.Entity as BaseMonsterCharacterEntity;
                if (monsterCharacterEntity.IsSummoned)
                    playerAttacker = monsterCharacterEntity.Summoner as BasePlayerCharacterEntity;
            }

            BaseGameNetworkManager.Singleton.LosePlayerTournament(this, playerAttacker);
        }
    }
}
