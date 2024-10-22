using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/EndPoint/Intersection", fileName = "NewIntersection")]
public class Intersection : EndPoint
{

    public override bool IsDestination(int checkId)
    {
        return false;
    }
}


