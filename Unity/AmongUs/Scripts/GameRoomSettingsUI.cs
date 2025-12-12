using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRoomSettingsUI : SettingsUI
{
    public void Open()
    {
        AmongUsRoomPlayer.MyRoomPlayer.myCharacter.IsMovable = false;
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        base.Close();
        AmongUsRoomPlayer.MyRoomPlayer.myCharacter.IsMovable = true;
    }
    public void ExitGameRoom()
    {
        var manager = AmongUsRoomManager.singleton;
        if(manager.mode == Mirror.NetworkManagerMode.Host)
        {
            manager.StopHost();
        }
        else if(manager.mode == Mirror.NetworkManagerMode.ClientOnly)
        {
            manager.StopClient();
        }
    }
    
}
