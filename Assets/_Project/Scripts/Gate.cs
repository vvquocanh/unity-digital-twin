using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/Gate", fileName = "NewGate")]
public class Gate : ScriptableObject
{
    [SerializeField] private int id;

    public int Id => id;

    [SerializeField] private Vector2 position;

    public Vector2 Position => position;

    [SerializeField] private float directionAngle;

    public float DirectionAngle => directionAngle;
}
