#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using Rewired;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MonoMod;

class patch_SystemManager : SystemManager {

    public extern Player orig_GetInput(PlayerType playerType);
    public override Player GetInput(PlayerType playerType) {
        if (YLModFreeCamera.IsEnabled) {
            if (PlayerManager.Instance?.GetPlayer(0) != null && playerType == PlayerType.MainPlayer)
                playerType = PlayerType.ArcadePlayer2;
        }

        return orig_GetInput(playerType);
    }

}
