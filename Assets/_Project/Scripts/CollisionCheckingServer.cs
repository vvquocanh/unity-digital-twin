using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCheckingServer : MonoBehaviour
{
    private List<Car> cars = new List<Car>();

    private Dictionary<int, Car> blockCar = new Dictionary<int, Car>();

    private Action<int> onCarBlock;

    private Action<int> onCarUnblock;

    private void Update()
    {
        CollisionChecking();
    }

    private void CollisionChecking()
    {
        List<CollisionSegment> segments = new List<CollisionSegment>();

        for (int i = cars.Count - 1; i >= 0; i--) {
            if (cars[i] != null) continue;

            if (blockCar.ContainsKey(cars[i].Id)) blockCar.Remove(cars[i].Id);

            cars.RemoveAt(i);
        }

        foreach (var car in cars)
        {
            segments.Add(car.GetCollisionSegment());
        }

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = i + 1; j < segments.Count; j++)
            {
                var firstCarRay = new Ray(cars[i].GetEndWorldCenterMax(), new Vector3(cars[i].direction.x, 0, cars[i].direction.y));
                var secondCarRay = new Ray(cars[j].GetEndWorldCenterMax(), new Vector3(cars[j].direction.x, 0, cars[j].direction.y));

                if (cars[j].IsTouchingCar(firstCarRay, cars[i].GetSafeDistance()))
                {
                    BlockCar(cars[i], cars[j]);
                    CheckReturn(cars[j], cars[i]);
                }
                else if (cars[i].IsTouchingCar(secondCarRay, cars[j].GetSafeDistance()))
                {
                    BlockCar(cars[j], cars[i]);
                    CheckReturn(cars[i], cars[j]);
                }
                else
                {
                    var isIntersect = MathSupport.IsIntersect(segments[i].Head, segments[i].Tail, segments[j].Head, segments[j].Tail);
                    if (!isIntersect)
                    {
                        CheckReturn(cars[i], cars[j]);
                        CheckReturn(cars[j], cars[i]);
                    }
                    else
                    {
                        var intersectPoint = MathSupport.GetIntersectionPoint(segments[i].Head, segments[i].Tail, segments[j].Head, segments[j].Tail);
                        var distanceToFirstCar = Vector2.Distance(intersectPoint, cars[i].GetEndWorldCenterMin());
                        var distanceToSecondCar = Vector2.Distance(intersectPoint, cars[j].GetEndWorldCenterMin());

                        if (distanceToFirstCar < distanceToSecondCar)
                        {
                            BlockCar(cars[j], cars[i]);
                            CheckReturn(cars[i], cars[j]);
                        }
                        else
                        {
                            BlockCar(cars[i], cars[j]);
                            CheckReturn(cars[j], cars[i]);
                        }
                    }
                }          
            }
        }
    }

    private void CheckReturn(Car car, Car paringCar)
    {
        if (car.status != CarStatus.Blocking) return;

        var isCarInBlocking = blockCar.TryGetValue(car.Id, out var blockingCar);

        if (!isCarInBlocking) return;

        if (paringCar != blockingCar) return;

        onCarUnblock?.Invoke(car.Id);

        blockCar.Remove(car.Id);
    }

    private void BlockCar(Car car, Car paringCar)
    {
        if (car.status == CarStatus.Blocking) return;

        if (blockCar.ContainsKey(car.Id)) return;

        blockCar.Add(car.Id, paringCar);

        onCarBlock?.Invoke(car.Id);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Car>(out var car)) return;

        cars.Add(car);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<Car>(out var exitCar)) return;

        cars.Remove(exitCar);

        foreach (var car in cars)
        {
            if (!blockCar.ContainsKey(car.Id)) continue;

            var blockingCar = blockCar[car.Id];

            if (exitCar == blockingCar)
            {
                onCarUnblock?.Invoke(car.Id);

                blockCar.Remove(car.Id);
            } 
        }
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
