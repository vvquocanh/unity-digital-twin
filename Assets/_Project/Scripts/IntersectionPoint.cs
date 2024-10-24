using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/IntersectionPoint", fileName = "NewIntersectionPoint")]
public class IntersectionPoint : ScriptableObject
{
    [SerializeField] private Vector2 coordination;

    public Vector2 Coordination => coordination;
}
