using System;
using UnityEngine;

public class Car : MonoBehaviour
{
    private int id;

    public int Id => id;

    public float velocity = 0f;

    public Vector2 direction;

    public Vector2 position;

    private int startGate;

    public int StartGate => startGate;

    private int endGate;

    public int EndGate => endGate;

    public bool isAlive;

    public CarStatus status = CarStatus.Blocking;

    private MeshRenderer carRenderer;

    private float safeTime = 0.7f;

    public int blockCount = 0;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Car");

        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        var rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
    }

    public void StartLate()
    {
        carRenderer = GetComponentInChildren<MeshRenderer>();
    }

    public void InitializeCar(int id, Vector2 direction, Vector2 position, int startGate, int endGate)
    {
        this.id = id;
        this.direction = direction;
        this.position = position;
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

        var localMax = carRenderer.localBounds.max;
        var localMin = carRenderer.localBounds.min;

        var localCenterZ = (localMax.z + localMin.z) / 2;
        var localCenterMax = new Vector3(carRenderer.localBounds.max.x + GetSafeDistance(), 0, localCenterZ);
        var localCenterMin = new Vector3(carRenderer.localBounds.min.x, 0, localCenterZ);

        var worldCenterMax = transform.TransformPoint(localCenterMax);
        var worldCenterMin = transform.TransformPoint(localCenterMin);

        return new CollisionSegment(new Vector2(worldCenterMax.x, worldCenterMax.z), new Vector2(worldCenterMin.x, worldCenterMin.z));
    }

    public float GetSafeDistance()
    {
        return velocity * safeTime;
    }

    public Vector2 GetEndWorldCenterMin()
    {
        var localMax = carRenderer.localBounds.max;
        var localMin = carRenderer.localBounds.min;

        var localCenterZ = (localMax.z + localMin.z) / 2;
        var localCenterMin = new Vector3(carRenderer.localBounds.min.x, 0, localCenterZ);

        var worldCenterMin = transform.TransformPoint(localCenterMin);

        return new Vector2(worldCenterMin.x, worldCenterMin.z);
    }

    public Vector3 GetEndWorldCenterMax()
    {
        var localMax = carRenderer.localBounds.max;
        var localMin = carRenderer.localBounds.min;

        var localCenterZ = (localMax.z + localMin.z) / 2;

        var localCenterMax = new Vector3(carRenderer.localBounds.max.x, 0, localCenterZ);


        Debug.DrawRay(transform.TransformPoint(localCenterMax), new Vector3(direction.x, 0.5f, direction.y), Color.red);

        return transform.TransformPoint(localCenterMax);
    }

    public bool IsTouchingCar(Ray ray, float distance)
    {
        bool isIntersect = carRenderer.bounds.IntersectRay(ray, out float intersectDistance);

        if (!isIntersect || (isIntersect && intersectDistance > distance)) return false;

        return true;
    }

    private float GetDirectionAngle()
    {
        return (float)Math.Round(MathSupport.VectorToAngle(direction), 1);
    }
}
