namespace MultiplayerARPG
{
    public enum MatchType : byte
    {
        None,
        //Solo Types, no Objective
        LastManStanding,
        BattleArena,
        KillConfirmed,
        BattleRoyal,
        //Team Types, No Objective
        TeamDeathMatch,
        //Team Types, Objective
        CaptureTheFlag,
        Domination,
        Survival,
    }
}
