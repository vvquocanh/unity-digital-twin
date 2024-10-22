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

    [SerializeField] private List<AdjacentGate> adjacentGates = new List<AdjacentGate>();

    private Action<HashSet<Direction>> onCarEnterIntersection;
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
                onCarEnterIntersection?.Invoke(new HashSet<Direction> { adjacentGate.Direction });
                return;
            }
        }

        onCarEnterIntersection?.Invoke(availableDirections);
    }

    public void SubscribeCarEnterIntersection(Action<HashSet<Direction>> onCarEnterIntersection)
    {
        this.onCarEnterIntersection += onCarEnterIntersection;
    }

    public void UnsubscribeCarEnterIntersection(Action<HashSet<Direction>> onCarEnterIntersection)
    {
        this.onCarEnterIntersection -= onCarEnterIntersection;
    }
}


