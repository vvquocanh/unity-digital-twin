using UnityEngine;

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

    private Vector2 direction;

    public Vector2 Direction {
        get => direction;
        set => direction = value;
    }
}
