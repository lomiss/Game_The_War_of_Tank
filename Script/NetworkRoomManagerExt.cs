using UnityEngine;

public class NetworkRoomManagerExt : CustomNetworkRoomManager
{
    bool showStartButton;

    public override void OnRoomServerPlayersReady()
    {
        // calling the base method calls ServerChangeScene as soon as all players are in Ready state.
        #if UNITY_SERVER
        base.OnRoomServerPlayersReady();
        #else
        showStartButton = true;
        #endif
    }
    
    public override void OnGUI()
    {
        base.OnGUI();
        if (allPlayersReady && showStartButton && GUI.Button(new Rect(150, 300, 120, 20), "START GAME"))
        {
            // set to false to hide it in the game scene
            showStartButton = false;

            ServerChangeScene(GameplayScene);
        }
    }
}

