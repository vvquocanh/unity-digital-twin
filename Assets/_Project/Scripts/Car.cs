using UnityEngine;

public class Car : MonoBehaviour
{
    private int id;

    public int Id => id;

    private float velocity = 0f;

    public float Velocity
    {
        get => velocity;
        set => velocity = Mathf.Max(0f, value);
    }

    private float directionAngle;

    public float DirectionAngle
    {
        set => directionAngle = value;
    }

    private Vector2 position;

    public Vector2 Position
    {
        get => position;
        set => position = value;
    }

    private float modelRotationOffset;

    public float ModelRotationOffset => modelRotationOffset;

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Car");

        var collider = gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        var rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
    }

    public void InitializeCar(int id, float modelRotationOffset, float directionAngle, Vector2 position)
    {
        this.id = id;
        this.directionAngle = directionAngle;
        this.position = position;
        this.modelRotationOffset = modelRotationOffset;
        transform.position = new Vector3(position.x, 0.2f, position.y);
        transform.eulerAngles = new Vector3(0, directionAngle, 0);
    }

    public void UpdateCarData()
    {
        transform.position = new Vector3(position.x, transform.position.y, position.y);
        if (Mathf.Abs(transform.eulerAngles.y - directionAngle) < 0.001f) return;

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, directionAngle, transform.eulerAngles.z);
    }
}
