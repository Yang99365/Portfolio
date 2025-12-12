using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypeEffect : MonoBehaviour
{
    public int charsPerSecond;
    public GameObject TalkCursor;
    public bool isAnim;

    Text msgText;
    AudioSource audioSource;
    string targetMsg;
    int index;
    float interval;
    

    private void Awake()
    {
        msgText= GetComponent<Text>();
        audioSource = GetComponent<AudioSource>();
    }

    public void SetMsg(string msg)
    {
        if (isAnim) //interrupt
        {
            msgText.text = targetMsg;
            CancelInvoke();
            EffectEnd();
        }
        else
        {
            targetMsg = msg;
            EffectStart();
        }
        
    }

    // Update is called once per frame
    void EffectStart()
    {
        msgText.text = "";
        index = 0;
        TalkCursor.SetActive(false);

        interval = 1.0f / charsPerSecond;
        Debug.Log(interval);

        isAnim = true;

        Invoke("Effecting",interval);
    }

    void Effecting()
    {
        if(msgText.text == targetMsg)
        {
            EffectEnd();
            return;
        }

        msgText.text += targetMsg[index];
        

        //sound
        if (targetMsg[index] != ' '|| targetMsg[index] != '.')
            audioSource.Play();

        index++;
        Invoke("Effecting",interval);
    }
    void EffectEnd()
    {
        isAnim = false;
        TalkCursor.SetActive(true);
    }
}
