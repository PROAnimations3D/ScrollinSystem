using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MultiplayerARPG
{
    public static partial class PlayerCharacterDataExtension
    {

        [DevExtMethods("CloneTo")]
        static void CloneTo(IPlayerCharacterData from, IPlayerCharacterData to)
        {
            to.TeamData = from.TeamData;
        }

    }
}
