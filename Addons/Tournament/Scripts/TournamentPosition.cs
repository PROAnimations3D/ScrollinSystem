using UnityEngine;


namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Tournament Position", menuName = "Tournament/Tournament Position", order = -5501)]
    public class TournamentPosition : BaseGameData
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
