using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AmongUsRoomPlayer : NetworkRoomPlayer
{
    private static AmongUsRoomPlayer myRoomPlayer;
    public static AmongUsRoomPlayer MyRoomPlayer
    {
        get
        {
            if(myRoomPlayer == null)
            {
                var players = FindObjectsOfType<AmongUsRoomPlayer>();
                foreach(var player in players)
                {
                    if(player.isOwned)
                    {
                        myRoomPlayer = player;
                    }
                }
            }
            return myRoomPlayer;
        }
    }
    [SyncVar(hook = nameof(SetPlayerColor_Hook))]
    public EPlayerColor playerColor;

    public void SetPlayerColor_Hook(EPlayerColor oldColor, EPlayerColor newColor)
    {
        LobbyUIManager.instance.CustomizeUI.UpdateUnselectColorButton(oldColor);
        LobbyUIManager.instance.CustomizeUI.UpdateSelectColorButton(newColor);
    }

    [SyncVar]
    public string nickname;

    public CharacterMover myCharacter;

    public void Start()
    {
        base.Start(); // 부모 클래스의 Start()를 호출

        if(isServer)
        {
            SpawnLobbyPlayerCharacter();
            LobbyUIManager.instance.ActiveStartButton();
        }
        if(isLocalPlayer)
        {
            CmdSetNickname(PlayerSettings.nickname);
        }
        LobbyUIManager.instance.GameRoomPlayerCount.UpdatePlayerCount();
    }

    private void OnDestroy()
    {
        if(LobbyUIManager.instance != null)
        {
            LobbyUIManager.instance.GameRoomPlayerCount.UpdatePlayerCount();
            LobbyUIManager.instance.CustomizeUI.UpdateUnselectColorButton(playerColor);
        }
    }
    [Command]
    public void CmdSetNickname(string nick)
    {
        nickname = nick;
        myCharacter.nickname = nick;
    }    


    [Command]
    public void CmdSetPlayerColor(EPlayerColor color)
    {
        playerColor = color;
        myCharacter.playerColor = color;
    }

    private void SpawnLobbyPlayerCharacter()
    {
        var roomSlots = (NetworkManager.singleton as AmongUsRoomManager).roomSlots;
        EPlayerColor color = EPlayerColor.Red;
        for (int i = 0; i < (int)EPlayerColor.Lime + 1; i++)
        {
            bool isFindSameColor = false;
            foreach (var roomPlayer in roomSlots)
            {
                var amongUsRoomPlayer = roomPlayer as AmongUsRoomPlayer;
                if(amongUsRoomPlayer.playerColor == (EPlayerColor)i && roomPlayer.netId != netId)
                {
                    isFindSameColor = true;
                    break;
                }
            }
            if(!isFindSameColor)
            {
                color = (EPlayerColor)i;
                break;
            }
        }
        playerColor = color;

        var spawnPositions = FindObjectOfType<SpawnPositions>();
        int index = spawnPositions.Index;

        Vector3 spawnPos = FindObjectOfType<SpawnPositions>().GetSpawnPosition();
        
        var playerCharacter = Instantiate(AmongUsRoomManager.singleton.spawnPrefabs[0], spawnPos, Quaternion.identity).GetComponent<LobbyCharacterMover>();
        playerCharacter.transform.localScale = index < 5 ? new Vector3(0.5f,0.5f,1f) : new Vector3(-0.5f,0.5f,1f);
        NetworkServer.Spawn(playerCharacter.gameObject, connectionToClient);
        playerCharacter.ownerNetId = netId;
        playerCharacter.playerColor = color;
    }
}
