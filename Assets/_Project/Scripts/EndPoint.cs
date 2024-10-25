using System;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    [SerializeField] private Gate gate;

    private Action<Car> onCarReachTheEnd;

    private void OnTriggerEnter(Collider other)
    {
        var car = other.GetComponent<Car>();
        if (car == null)
        {
            Debug.LogError("Fail to get car");
            return;
        }

        if (car.Id != gate.Id)
        {
            Debug.LogError("Car enter the wrong end point.");
            return;
        }

        onCarReachTheEnd?.Invoke(car);
    }

    public void SubscribeOnCarReachTheEnd(Action<Car> onCarReachTheEnd)
    {
        this.onCarReachTheEnd = onCarReachTheEnd;
    }

    public void UnsubscribeOnCarReachTheEnd(Action<Car> onCarReachTheEnd)
    {
        this.onCarReachTheEnd -= onCarReachTheEnd;
    }
}
