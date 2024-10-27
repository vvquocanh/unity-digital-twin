using System;
using UnityEngine;

public class Car : MonoBehaviour
{
    private int id;

    public int Id => id;

    public float velocity = 0f;

    public Vector2 direction;

    public Vector2 position;

    private float modelRotationOffset;

    public float ModelRotationOffset => modelRotationOffset;

    private int startGate;

    public int StartGate => startGate;

    private int endGate;

    public int EndGate => endGate;

    public bool isAlive;

    public CarStatus status = CarStatus.Blocking;

    private Renderer carRenderer;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Car");

        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        var rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;

        carRenderer = GetComponent<Renderer>();
    }

    public void InitializeCar(int id, float modelRotationOffset, Vector2 direction, Vector2 position, int startGate, int endGate)
    {
        this.id = id;
        this.direction = direction;
        this.position = position;
        this.modelRotationOffset = modelRotationOffset;
        this.startGate = startGate;
        this.endGate = endGate;
        isAlive = true;
        transform.position = new Vector3(position.x, 0.2f, position.y);
        transform.eulerAngles = new Vector3(0, GetDirectionAngle(), 0);
    }

    public void UpdateCarData()
    {
        float directionAngle = GetDirectionAngle();
        transform.position = new Vector3(position.x, transform.position.y, position.y);
        if (Mathf.Abs(transform.eulerAngles.y - directionAngle) < 0.001f) return;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, directionAngle, transform.eulerAngles.z);
    }

    public CollisionSegment GetCollisionSegment()
    {
        Vector2 head = new Vector2(carRenderer.bounds.max.x, carRenderer.bounds.max.z) * direction;

        Vector2 tail = new Vector2(carRenderer.bounds.min.x, carRenderer.bounds.min.z) * direction;

        return new CollisionSegment(head, tail);
    }

    private float GetDirectionAngle()
    {
        return (float)Math.Round(MathSupport.VectorToAngle(direction, modelRotationOffset), 1);
    }
}
