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

        // Resource Load
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

                // 그냥 이건 씬에다 배치해서 써도 될듯하니 일단 주석처리
                //UI_TitleScene sceneUI = Managers.UI.ShowSceneUI<UI_TitleScene>();

                // Backend (아직 백앤드 미구현 + 안할수도있음. 즉 이 부분은 안쓸수있음)
                //Managers.Backend.Init(() =>
                //{
                //    Managers.Data.Init(() =>
                //    {
                //        OnBackendInitSuccess();
                //    });
                //});
            }
        });

        return true;
    }

    private void OnBackendInitSuccess()
    {
		// UI
		//UI_TitleScene sceneUI = Managers.UI.ShowSceneUI<UI_TitleScene>();
        //sceneUI.SetInfo();
    }

    public override void Clear()
    {
    }
}
