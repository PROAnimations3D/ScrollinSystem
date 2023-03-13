namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {
        public virtual void ClearTournament()
        {
            targetPosition = null;
            PlayingCharacterEntity.StopMove();
            PlayingCharacterEntity.SetTargetEntity(null);
            TargetEntity = null;
            SelectedEntity = null;
            HideNpcDialog();
        }
    }
}
