using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/EndPoint/Gate", fileName = "NewGate")]
public class Gate : EndPoint
{
    [SerializeField] private Vector2 position;

    public Vector2 Position => position;

    [SerializeField] private float directionAngle;

    public float DirectionAngle => directionAngle;

    public override bool IsDestination(int checkId)
    {
        return id == checkId;
    }
}
