using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentApplier : MonoBehaviour
{
    public SkinSet skin;
    SkinnedMeshRenderer renderer;
    void Start()
    {
        renderer = GetComponent<SkinnedMeshRenderer>();
        renderer.sharedMesh = skin.meshes[PlayerPrefs.GetInt("agentCode", 0)];
    }

    
  
}
