using UnityEngine;
using uPLibrary.Networking.M2Mqtt;

public class ServerMQTTConnection : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private ServerMQTTSetting serverMQTTSetting;

    [SerializeField] private MQTTSubscriptionSetting subscriptionSetting;

    [Header("Attribute")]
    [SerializeField] private int id;

    private MqttClient mqttClient = null;
    private void Awake()
    {
        mqttClient = new MqttClient(serverMQTTSetting.BrokerAddress, serverMQTTSetting.BrokerPort, false, null, null, MqttSslProtocols.None);   
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
        var rc = mqttClient.Subscribe(subscriptionSetting.Topics, subscriptionSetting.QosLevel);

    }

    private void OnDestroy()
    {
        mqttClient.Unsubscribe(subscriptionSetting.Topics);
        mqttClient.Disconnect();
        Debug.Log("Disconnected from the broker.");
    }
}
