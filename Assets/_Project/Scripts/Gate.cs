using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/Gate", fileName = "NewGate")]
public class Gate : ScriptableObject
{
    [SerializeField] private int id;

    public int Id => id;

    [SerializeField] private Vector2 position;

    public Vector2 Position => position;

    [SerializeField] private Vector2 direction;

    public Vector2 Direction => direction;

    [SerializeField] private IntersectionPoint adjacentIntersectionPoint;

    public IntersectionPoint AdjacentIntersectionPoint => adjacentIntersectionPoint;
}
