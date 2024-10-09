using System;
using UnityEngine;

public class Car : MonoBehaviour
{
    private int id;

    public int Id => id;

    private float acceleration = 0f;

    public float Acceleration
    {
        get => acceleration;
        set => acceleration = Math.Max(0f, value);
    }

    private float velocity = 0f;
    
    public float Velocity
    {
        get => velocity;
        set => velocity = Math.Max(0f, value);
    }

    private Vector2 direction;

    public Vector2 Direction => direction;

    public Car(int id, float velocity, Vector2 direction)
    {
        this.id = id;
        this.Acceleration = velocity;
        this.Velocity = velocity;
        SetDirection(direction);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection;

        var angle = transform.eulerAngles;
        transform.eulerAngles = new Vector3(angle.x, ConvertDirectionToAngle(), angle.z);
    }

    private float ConvertDirectionToAngle()
    {
        return Mathf.Atan2(direction.x, direction.y) * 180 / Mathf.PI;
    }

}
