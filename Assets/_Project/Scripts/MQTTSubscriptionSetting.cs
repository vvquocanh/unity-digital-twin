using UnityEngine;

[CreateAssetMenu(fileName = "NewMQTTSubscriptionSetting", menuName = "MQTT/MQTTSubscriptionSetting")]
public class MQTTSubscriptionSetting : ScriptableObject
{
    [SerializeField] private string[] topics;

    public string[] Topics => topics;

    [SerializeField] private byte[] qosLevel;

    public byte[] QosLevel => qosLevel;
}
