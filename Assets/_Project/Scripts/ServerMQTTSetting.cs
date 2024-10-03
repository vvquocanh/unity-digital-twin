using UnityEngine;

[CreateAssetMenu(fileName = "NewServerMQTTSetting", menuName = "MQTT/ServerMQTTSetting")]
public class ServerMQTTSetting : ScriptableObject
{
    [SerializeField] private string brokerAddress;

    public string BrokerAddress => brokerAddress;

    [SerializeField] private int brokerPort;

    public int BrokerPort => brokerPort;

    [SerializeField] private string clientId;

    public string ClientId => clientId;

    [SerializeField] private string username;

    public string Username => username;

    [SerializeField] private string password;

    public string Password => password;

    [SerializeField] private bool cleanSession;

    public bool CleanSession => cleanSession;

    [SerializeField] private ushort aliveTime;

    public ushort AliveTime => aliveTime;
}
