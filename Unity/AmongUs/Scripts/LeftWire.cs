using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Generators;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class LeftWire : MonoBehaviour
{
    public EWireColor WireColor { get; private set; }
    public bool isConnected { get; private set; }

    [SerializeField]
    private List<Image> mWireImages;

    [SerializeField]
    private Image mLightImage;

    [SerializeField]
    private RectTransform mWireBody;
    [SerializeField]
    private RightWire mConnectedWire;

    [SerializeField]
    private float offset = 15f;

    private Canvas mGameCanvas;
    // Start is called before the first frame update
    void Start()
    {
        mGameCanvas = FindObjectOfType<Canvas>();
    }

    // Update is called once per frame
    
    public void SetTarget(Vector3 targetPosition, float offset)
    {
        float angle = Vector2.SignedAngle(transform.position + Vector3.right - transform.position, 
        targetPosition - transform.position);

        float distance = Vector2.Distance(mWireBody.transform.position, targetPosition) + offset;
        mWireBody.localRotation = Quaternion.Euler(new Vector3(0f,0f,angle));
        mWireBody.sizeDelta = new Vector2(distance /** (1/mGameCanvas.transform.localScale.x)*/, mWireBody.sizeDelta.y);
        // distance * (1/mGameCanvas.transform.localScale.x) 
        //캔버스 크기의 역수를 곱해줘야 화면에 따른 비율에 문제가 없다고함 , 근데 이걸쓰면 와이어 클릭시
        // 길이가 솟구치는 현상이 발생함
    }

    public void ResetTarget()
    {
        mWireBody.localRotation = Quaternion.Euler(Vector3.zero);
        mWireBody.sizeDelta = new Vector2(0f, mWireBody.sizeDelta.y);
    }

    public void SetWireColor(EWireColor wirecolor)
    {
        WireColor = wirecolor;
        Color color = Color.black;
        switch (wirecolor)
        {
            case EWireColor.Red:
                color = Color.red;
                break;
            case EWireColor.Blue:
                color = Color.blue;
                break;
            case EWireColor.Yellow:
                color = Color.yellow;
                break;
            case EWireColor.Magenta:
                color = Color.magenta;
                break;
        }
        foreach(var image in mWireImages)
        {
            image.color = color;
        }
    }

    public void ConnectWire(RightWire rightWire)
    {
        if(mConnectedWire != null && mConnectedWire != rightWire)
        {
            mConnectedWire.DisconnectWire(this);
            mConnectedWire = null;
        }
        mConnectedWire = rightWire;
        if(mConnectedWire.WireColor == WireColor)
        {
            isConnected = true;
            mLightImage.color = Color.yellow;
        }
    }

    public void DisconnectWire()
    {
        if(mConnectedWire != null)
        {
            mConnectedWire.DisconnectWire(this);
            mConnectedWire = null;
        }
        mLightImage.color = Color.gray;
        isConnected = false;
    }
}
