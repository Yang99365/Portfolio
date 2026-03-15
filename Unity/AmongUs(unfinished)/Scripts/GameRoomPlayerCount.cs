using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class GameRoomPlayerCount : NetworkBehaviour
{
    [SyncVar]
    private int minPlayer;
    [SyncVar]
    private int maxPlayer;
    [SerializeField]
    private Text playerCountText;

    public void UpdatePlayerCount()
    {
        var players = FindObjectsOfType<AmongUsRoomPlayer>();
        bool isStartable = players.Length >= minPlayer;
        playerCountText.color = isStartable ? Color.white : Color.red;
        playerCountText.text = string.Format("{0}/{1}", players.Length ,maxPlayer);
        LobbyUIManager.instance.SetInteractableStartButton(isStartable);
    }
    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
        {
            var manager = NetworkManager.singleton as AmongUsRoomManager;
            minPlayer = manager.minPlayerCount;
            maxPlayer = manager.maxConnections;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
