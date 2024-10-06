using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMQTTSubscriptionSetting", menuName = "MQTT/MQTTSubscriptionSetting")]
public class MQTTSubscriptionSetting : ScriptableObject
{
    [SerializeField] private string topic;

    public string Topic => topic;

    [SerializeField] private byte qos;

    public byte Qos => qos;
}
