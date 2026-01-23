using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static Define;

public class TitleScene : BaseScene
{
    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        SceneType = EScene.Title;
        Managers.Scene.SetCurrentScene(this);

        // Addressable PreLoad (Label: "PreLoad")
        Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalcount) =>
        {
            Debug.Log($"{key} {count}/{totalcount}");
            if (count == totalcount)
            {
                Managers.Data.Init();
                Object eventSystem = FindAnyObjectByType(typeof(EventSystem));
                if (eventSystem == null)
                {
                    eventSystem = Managers.Resource.Instantiate("EventSystem");
                    eventSystem.name = "EventSystem";
                    DontDestroyOnLoad(eventSystem);
                }
            }
        });

        return true;
    }

    public override void Clear()
    {
    }
}
