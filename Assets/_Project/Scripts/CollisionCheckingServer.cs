using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheckingServer : MonoBehaviour
{
    private List<Car> cars = new List<Car>();

    private Dictionary<int, int> blockCar = new Dictionary<int, int>();

    private Action<int> onCarBlock;

    private Action<int> onCarUnblock;

    private void Update()
    {
        CollisionChecking();
    }

    private void CollisionChecking()
    {
        List<CollisionSegment> segments = new List<CollisionSegment>();

        foreach (var car in cars)
        {
            segments.Add(car.GetCollisionSegment());
        }

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = i + 1; j < segments.Count; j++)
            {
                var isIntersect = MathSupport.IsIntersect(segments[i].Head, segments[i].Tail, segments[j].Head, segments[j].Tail);
                if (isIntersect)
                {
                    CheckReturn(cars[i], j);
                    CheckReturn(cars[j], i);
                }
                else
                {
                    Debug.Log($"2 cars intersect: {cars[i].Id} and {cars[j].Id}");
                    var intersectPoint = MathSupport.GetIntersectionPoint(segments[i].Head, segments[i].Tail, segments[j].Head, segments[j].Tail);
                    var distanceToFirstCar = Vector2.Distance(intersectPoint, new Vector2(cars[i].gameObject.transform.position.x, cars[i].gameObject.transform.position.z));
                    var distanceToSecondCar = Vector2.Distance(intersectPoint, new Vector2(cars[j].gameObject.transform.position.x, cars[i].gameObject.transform.position.z));

                    if (distanceToFirstCar < distanceToSecondCar)
                    {
                        BlockCar(cars[j], i);
                        CheckReturn(cars[i], j);
                    }
                    else
                    {
                        BlockCar(cars[i], j);
                        CheckReturn(cars[j], i);
                    }
                }
            }
        }
    }

    private void CheckReturn(Car car, int paringCarId)
    {
        if (car.status != CarStatus.Blocking) return;

        var isCarInBlocking = blockCar.TryGetValue(car.Id, out int obstacleCar);

        if (!isCarInBlocking) return;

        if (paringCarId != obstacleCar) return;

        onCarUnblock?.Invoke(car.Id);
    }

    private void BlockCar(Car car, int paringCarId)
    {
        if (car.status == CarStatus.Blocking) return;

        if (blockCar.ContainsKey(car.Id)) return;

        blockCar.Add(car.Id, paringCarId);

        onCarBlock?.Invoke(car.Id);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Car>(out var car)) return;

        cars.Add(car);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Car>(out var car)) return;

        cars.Remove(car);
    }

    public void SubscribeOnCarBlock(Action<int> onCarBlock)
    {
        this.onCarBlock += onCarBlock;
    }

    public void UnsubscribeOnCarBlock(Action<int> onCarBlock)
    {
        this.onCarBlock -= onCarBlock;
    }

    public void SubscribeOnCarUnblock(Action<int> onCarUnblock)
    {
        this.onCarUnblock += onCarUnblock;
    }

    public void UnsubscribeOnCarUnblock(Action<int> onCarUnblock)
    {
        this.onCarUnblock -= onCarUnblock;
    }
}
