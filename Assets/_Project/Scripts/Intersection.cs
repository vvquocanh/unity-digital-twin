using System;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
        Straight
    }

    [Serializable]
    private struct AdjacentGate
    {
        [SerializeField] private Direction direction;

        public Direction Direction => direction;

        [SerializeField] private Gate gate;

        public Gate Gate => gate;
    }

    [Serializable]
    public struct AdjacentIntersectionPoint
    {
        [SerializeField] private Direction direction;

        public Direction Direction => direction;

        [SerializeField] private IntersectionPoint intersectionPoint;

        public IntersectionPoint IntersectionPoint => intersectionPoint;
    }

    [SerializeField] private List<AdjacentGate> adjacentGates = new List<AdjacentGate>();

    [SerializeField] private List<AdjacentIntersectionPoint> adjacentIntersectionPoints = new List<AdjacentIntersectionPoint>();

    private Action<int, HashSet<Direction>, List<AdjacentIntersectionPoint>> onCarEnterIntersection;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Car>(out var car))
        {
            Debug.LogError("Fail to get car.");
            return;
        }

        var availableDirections = new HashSet<Direction>() { Direction.Left, Direction.Right, Direction.Straight };

        foreach (AdjacentGate adjacentGate in adjacentGates)
        {
            if (adjacentGate.Gate.Id != car.Id) availableDirections.Remove(adjacentGate.Direction);
            else
            {
                onCarEnterIntersection?.Invoke(car.Id, new HashSet<Direction> { adjacentGate.Direction }, adjacentIntersectionPoints);
                return;
            }
        }

        onCarEnterIntersection?.Invoke(car.Id, availableDirections, adjacentIntersectionPoints);
    }

    public void SubscribeCarEnterIntersection(Action<int, HashSet<Direction>, List<AdjacentIntersectionPoint>> onCarEnterIntersection)
    {
        this.onCarEnterIntersection += onCarEnterIntersection;
    }

    public void UnsubscribeCarEnterIntersection(Action<int, HashSet<Direction>, List<AdjacentIntersectionPoint>> onCarEnterIntersection)
    {
        this.onCarEnterIntersection -= onCarEnterIntersection;
    }
}


