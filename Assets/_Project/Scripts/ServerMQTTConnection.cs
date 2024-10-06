using System;
using System.Collections.Generic;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

public class ServerMQTTConnection : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private ServerMQTTSetting serverMQTTSetting;

    [SerializeField] private List<MQTTSubscriptionSetting> subscriptionSettings;

    [Header("Attribute")]
    [SerializeField] private int id;

    private MqttClient mqttClient = null;

    private List<string> topicList = new List<string>();

    private List<byte> qosList = new List<byte>();

    private Action<object, MqttMsgPublishEventArgs> onMessageReceived;

    private void Awake()
    {
        InitMqttClient();

        InitSubscriptionSetting();
    }

    private void InitMqttClient()
    {
        mqttClient = new MqttClient(serverMQTTSetting.BrokerAddress, serverMQTTSetting.BrokerPort, false, null, null, MqttSslProtocols.None);
    }

    private void InitSubscriptionSetting()
    {
        foreach (var setting in subscriptionSettings)
        {
            topicList.Add(string.Concat(setting.Topic, "/#"));
            qosList.Add(setting.Qos);
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        var rc = ConnectToBroker();

        if (rc != 0) return;

        Subscribe();
    }

    private int ConnectToBroker()
    {
        var clientId = string.Concat(serverMQTTSetting.ClientId, ":", id);
        var rc = mqttClient.Connect(clientId, serverMQTTSetting.Username, serverMQTTSetting.Password, serverMQTTSetting.CleanSession, serverMQTTSetting.AliveTime);

        if (rc == 0) Debug.Log("Connected to the broker.");
        else Debug.LogError($"Fail to connect to the broker: {rc}.");

        return rc;
    }

    private void Subscribe()
    {
        mqttClient.MqttMsgPublishReceived += OnPublishReceived;
        var rc = mqttClient.Subscribe(topicList.ToArray(), qosList.ToArray());
        if (rc != 0) return;
        Debug.LogError("Failt to subscribe");

    }

    private void OnPublishReceived(object sender, MqttMsgPublishEventArgs msg)
    {
        onMessageReceived?.Invoke(sender, msg);
    }

    private void OnDestroy()
    {
        mqttClient.Unsubscribe(topicList.ToArray());
        mqttClient.MqttMsgPublishReceived -= OnPublishReceived;
        mqttClient.Disconnect();
        Debug.Log("Disconnected from the broker.");
    }

    public void AddMessageReceivedCallback(Action<object, MqttMsgPublishEventArgs> callback)
    {
        onMessageReceived += callback;
    }

    public void RemoveMessageReceivedCallback(Action<object, MqttMsgPublishEventArgs> callback)
    {
        onMessageReceived -= callback;
    }
}
