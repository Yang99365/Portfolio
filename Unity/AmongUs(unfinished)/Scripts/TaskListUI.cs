using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TaskListUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private float offset;
    [SerializeField]
    private RectTransform TaskListUITransform;
    
    private bool isOpen = true;
    private float timer;

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(OpenAndHideUI());
    }

   private IEnumerator OpenAndHideUI()
   {
    isOpen = !isOpen;
    if(timer != 0f) // 반전시킴으로써 열리거나 닫히는 도중에 클릭이 되었을 때 자연스럽게 처리
    {
        timer = 1f - timer;
    }
    while(timer <= 1f)
    {
        timer += Time.deltaTime * 2f;

        float start = isOpen ? -TaskListUITransform.sizeDelta.x : offset;
        float dest = isOpen ? offset : -TaskListUITransform.sizeDelta.x;
        TaskListUITransform.anchoredPosition = new Vector2(Mathf.Lerp(start,dest,timer),TaskListUITransform.anchoredPosition.y);
        yield return null;
    }

   }
   
    
}
