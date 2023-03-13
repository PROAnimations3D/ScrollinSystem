namespace MultiplayerARPG
{
    public class DungeonBossEntity : MonsterCharacterEntity
    {
        public override void Killed(EntityInfo lastAttacker)
        {
            base.Killed(lastAttacker);
            bool isParty = lastAttacker.PartyId > 0;
            BasePlayerCharacterEntity lastPlayer = null;
            BaseCharacterEntity attackerCharacter;
            lastAttacker.TryGetEntity(out attackerCharacter);
            lastPlayer = attackerCharacter as BasePlayerCharacterEntity;
            BaseGameNetworkManager.Singleton.BossKilled(isParty, lastPlayer.CharacterName);
        }

        public override bool CanReceiveDamageFrom(EntityInfo instigator)
        {
            if (!base.CanReceiveDamageFrom(instigator))
                return false;
            return CurrentGameManager.DungeonRunning;
        }
    }
}
