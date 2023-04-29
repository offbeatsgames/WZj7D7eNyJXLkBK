using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;

public class EndCutscreenManager : MonoBehaviour
{
    public PlayableDirector endcutscreen;
    public GameObject[] Police_prefebs;

    [Header("Spawn number shuold be less than spawnpoint")]
    public GameObject[] spawnpoints;
    public int SpawnNumber;
    // Start is called before the first frame update
    private void Awake()
    {
        endcutscreen = transform.GetComponentInChildren<PlayableDirector>();

    }
    void Start()
    {
        spawnpoints = GameObject.FindGameObjectsWithTag("SPoint");
        


    }

    // Update is called once per frame
    void Update()
    {
        
    }


   public void playCutscreen()
    {
        endcutscreen.Play();
        var randomObjects = spawnpoints.OrderBy(x => Random.Range(0f, 1f)).Take(SpawnNumber).ToArray();
        var randomPolice_Prefabs = Police_prefebs.OrderBy(x => Random.Range(0f, 1f)).Take(SpawnNumber).ToArray();

        // Iterate over the selected objects and print their names

        for (int i = 0; i < randomObjects.Length; i++)
        {
            Instantiate(randomPolice_Prefabs[i], randomObjects[i].transform);
        }
    }
}
