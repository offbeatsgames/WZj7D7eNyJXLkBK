using UnityEngine;
using UnityEngine.UI;

public class CarDisplay : MonoBehaviour
{
    [Header("Description")]
    [SerializeField] private Text carName;
    [SerializeField] private Text carDescription;
    [SerializeField] private Text carPrice;

  

    [Header("Car Model")]
    [SerializeField] private GameObject carModel;

    public void UpdateCar(Car _newCar)
    {
        carName.text = _newCar.carName;
        carDescription.text = _newCar.carDescription;
        carPrice.text = _newCar.carPrice + "$";

       

        if (carModel.transform.childCount > 0)
            Destroy(carModel.transform.GetChild(0).gameObject);
        Instantiate(_newCar.carModel, carModel.transform.position, carModel.transform.rotation, carModel.transform);
    }
}