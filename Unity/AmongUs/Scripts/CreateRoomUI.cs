using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField]
    private List<Image> crewImgs;
    [SerializeField]
    private List<Button> imposterCountButtons;
    [SerializeField]
    private List<Button> maxPlayerCountButtons;

    private CreateGameRoomData roomData;
    // Start is called before the first frame update
    void Start()
    {
        // 인스턴스화 시킨 이미지를 가져와서 색상을 변경해야함
        // 이미지를 인스턴스화 시키는 이유는 이미지를 복사해서 사용하기 때문에 원본 이미지의 색상이 변경되면 복사된 이미지의 색상도 변경되기 때문
        for (int i = 0; i < crewImgs.Count; i++)
        {
            Material materialInstance = Instantiate(crewImgs[i].material);
            crewImgs[i].material = materialInstance;
        }
        roomData = new CreateGameRoomData() { imposterCount = 1, maxPlayerCount = 10 };
        UpdateCrewImage();
        
    }

    public void UpdateMaxPlayerCount(int count)
    {
        roomData.maxPlayerCount = count;
        for(int i = 0; i < maxPlayerCountButtons.Count; i++)
        {
            if(i==count -4)
            {
                maxPlayerCountButtons[i].image.color = new Color(1f,1f,1f,1f);
            }
            else
            {
                maxPlayerCountButtons[i].image.color = new Color(1f,1f,1f,0f);
            }
        }
        UpdateCrewImage();
    }
    public void UpdateImposterCount(int count)
    {
        roomData.imposterCount = count;
        for(int i = 0; i < imposterCountButtons.Count; i++)
        {
            if(i==count -1)
            {
                imposterCountButtons[i].image.color = new Color(1f,1f,1f,1f);
            }
            else
            {
                imposterCountButtons[i].image.color = new Color(1f,1f,1f,0f);
            }
        }
        //인원에 따른 제약사항
        int limitMaxPlayer = count == 1 ? 4 : count == 2 ? 7 : 9;
        if(roomData.maxPlayerCount < limitMaxPlayer)
        {
            UpdateMaxPlayerCount(limitMaxPlayer);
        }
        else
        {
            UpdateMaxPlayerCount(roomData.maxPlayerCount);
        }
        for (int i = 0; i < maxPlayerCountButtons.Count; i++)
        {
            var text = maxPlayerCountButtons[i].GetComponentInChildren<Text>();
            if(i < limitMaxPlayer - 4)
            {
                maxPlayerCountButtons[i].interactable = false;
                text.color = Color.gray;
            }
            else
            {
                maxPlayerCountButtons[i].interactable = true;
                text.color = Color.white;
            }
        }
    }

    private void UpdateCrewImage()
    {
        for (int i = 0; i < crewImgs.Count; i++)
        {
            crewImgs[i].material.SetColor("_PlayerColor", Color.white);
        }

        int imposterCount = roomData.imposterCount;
        int idx = 0;
        while (imposterCount != 0)
        {
            if(idx >= roomData.maxPlayerCount)
            {
                idx=0;
            }
            if(crewImgs[idx].material.GetColor("_PlayerColor") != Color.red && Random.Range(0,5)==0)
            {
                crewImgs[idx].material.SetColor("_PlayerColor", Color.red);
                imposterCount--;
                //roomData에 저장된 임포스터 수를 가져와 0이 될떄까지 이미지를 랜덤으로 뽑아 빨간색으로 만듬
                //임포스터 수 확인
            }
            idx++;
        }
        //위 과정이 끝난후 설정한 플레이어 수만큼 크루원 이미지 활성화, 나머지는 비활성화
        for (int i=0; i<crewImgs.Count; i++)
        {
            if (i < roomData.maxPlayerCount)
            {
                crewImgs[i].gameObject.SetActive(true);
            }
            else
            {
                crewImgs[i].gameObject.SetActive(false);
            }
        }

    }

    public void CreateRoom()
    {
        var manager = NetworkManager.singleton as AmongUsRoomManager;
        // 방 설정 작업처리 할 구간
        manager.minPlayerCount = roomData.imposterCount == 1 ? 4 : roomData.imposterCount == 2 ? 7 : 9;
        manager.imposterCount = roomData.imposterCount;
        manager.maxConnections = roomData.maxPlayerCount;

        // 방 설정 작업 구간
        
        // 서버를 여는 동시에 클라이언트로써 참가하게 해주는 함수
        manager.StartHost();
    }
    

}

public class CreateGameRoomData
{
    public int imposterCount;
    public int maxPlayerCount;
}