using UnityEngine;

[CreateAssetMenu(fileName = "NewMQTTPublishSetting", menuName = "MQTT/MQTTPublishSetting")]
public class MQTTPublishSetting : ScriptableObject
{
    [SerializeField] private string topic;

    public string Topic => topic;

    [SerializeField] private byte qos;

    public byte Qos => qos;

    [SerializeField] private bool retain;

    public bool Retain => retain;
}
