using UnityEngine;
using UnityEngine.UIElements;

public class Car : MonoBehaviour
{
    private int id;

    public int Id
    {
        get => id;
        set => id = value;
    }

    private float acceleration = 0f;

    public float Acceleration 
    {
        get => acceleration;
        set => acceleration = Mathf.Max(0f, value);
    }

    private float velocity = 0f;

    public float Velocity
    {
        get => velocity;
        set => velocity = Mathf.Max(0f, value);
    }

    float directionAngle;

    public float DirectionAngle
    {
        set => directionAngle = value;
    }

    private Vector2 position;

    public Vector2 Position
    {
        set => position = value;
    }

    private float modelRotationOffset;

    public float ModelRotationOffset
    {
        set => modelRotationOffset = value;
        get => modelRotationOffset;
    }

    public void UpdateCarData()
    {
        transform.position = new Vector3(position.x, transform.position.y, position.y);
        if (Mathf.Abs(transform.eulerAngles.y - directionAngle) < 0.001f) return;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, directionAngle, transform.eulerAngles.z);
    }
}
