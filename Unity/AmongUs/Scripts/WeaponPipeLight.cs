using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPipeLight : MonoBehaviour
{
    private Animator animator;

    private WaitForSeconds wait = new WaitForSeconds(0.15f);

    private List<WeaponPipeLight> lights = new List<WeaponPipeLight>();
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        for(int i = 0; i<transform.childCount; i++)
        {
            var child = transform.GetChild(i).GetComponent<WeaponPipeLight>();
            if(child)
            {
                lights.Add(child);
            }
        }
    }

    public void TurnOnLight()
    {
        animator.SetTrigger("On");
        StartCoroutine(TurnOnLightAtChild());
    }

    private IEnumerator TurnOnLightAtChild()
    {
        yield return wait;

        foreach(var child in lights)
        {
            child.TurnOnLight();
        }
    }
}
