using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AmongUsRoomManager : NetworkRoomManager
{
    public GameRuleData gameRuleData;
    public int minPlayerCount;
    public int imposterCount;
    public override void OnRoomServerConnect(NetworkConnectionToClient conn)
    {
        // 서버에서 새로 접속한 클라이언트 감지 시 동작
        base.OnRoomServerConnect(conn);

        
    }
}
