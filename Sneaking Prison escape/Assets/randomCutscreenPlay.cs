using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomCutscreenPlay : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] Cutscreens_Gameonjects;
    public int randomint;
    public int a;
    void Awake()
    {
        randomint = Random.Range(0, 100);
        if(randomint <= 40)
        {
            a = 0;
        }else if(randomint <= 80 && randomint >= 40)
        {
            a = 1;
        }
        else
        {
            a = 2;
        }

        Cutscreens_Gameonjects[a].SetActive(true
            );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
