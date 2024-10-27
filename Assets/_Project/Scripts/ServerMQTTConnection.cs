using System;
using System.Collections.Generic;
using System.Text;
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
            topicList.Add(setting.Topic);
            qosList.Add(setting.Qos);
        }
    }
    // Start is called before the first frame update
    private void Start()
    {
        var rc = ConnectToBroker();

        if (rc != 0) return;

        mqttClient.MqttMsgPublishReceived += OnPublishReceived;

        mqttClient.ConnectionClosed += OnConnectionClosed;

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
        
        var rc = mqttClient.Subscribe(topicList.ToArray(), qosList.ToArray());
        if (rc != 0) return;
        Debug.LogError("Fail to subscribe to the broker.");

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

    public void Publish(string topic, string message, byte qos, bool retain)
    {
        var rc = mqttClient.Publish(topic, Encoding.ASCII.GetBytes(message), qos, retain);
        if (rc != 0) return;

        Debug.LogError($"Fail to publish with topic: {topic}");
    }

    private void OnConnectionClosed(object sender, EventArgs e)
    {
        int rc = ConnectToBroker();

        if (rc != 0) return;

        Subscribe();
    }
}
