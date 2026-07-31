using DOTABATA_VRLand.Shared.Interfaces.StreamingHubs;
using DOTABATA_VRLand.Shared.Models.Entities;
using Steamworks;
using System;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ConnectionTest : MonoBehaviour {
    private void Start() {
        SteamVR_Fade.View(new Color(0, 0, 0, 0), 1.0f);

        Player player = Player.instance;
        player.GetComponent<SmoothLocomotion>().enabled = true;
    }
}
