using UnityEngine;

[CreateAssetMenu(fileName = "New Car", menuName = "Scriptable Objects/Car")]
public class Car : ScriptableObject
{
    public string carName;
    public string carDescription;
    public int carPrice;
   
    public GameObject carModel;
}