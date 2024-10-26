using System;
using UnityEngine;

public class EndPoint : MonoBehaviour
{
    [SerializeField] private Gate gate;

    private Action<Car> onCarReachTheEnd;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Car>(out var car))
        {
            Debug.LogError("Fail to get car");
            return;
        }

        if (car.EndGate != gate.Id)
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
