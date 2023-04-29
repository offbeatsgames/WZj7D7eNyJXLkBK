using UnityEngine;

[CreateAssetMenu(fileName = "SkinSet", menuName = "Scriptable Objects/SkinSet")]
public class SkinSet : ScriptableObject
{
    public Mesh[] meshes;
}