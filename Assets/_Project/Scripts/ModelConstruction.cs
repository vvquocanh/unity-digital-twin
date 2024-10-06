using System.IO;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

[RequireComponent(typeof(ServerMQTTConnection))]
public class ModelConstruction : MonoBehaviour
{
    [SerializeField] private MQTTSubscriptionSetting setting;

    private ServerMQTTConnection mqttConnection;

    private MemoryStream memoryStream = new MemoryStream();

    private void Awake()
    {
        mqttConnection = GetComponent<ServerMQTTConnection>();
    }

    private void Start()
    {
        mqttConnection.AddMessageReceivedCallback(OnMessageReceived);
    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs msg)
    {
        if (!msg.Topic.StartsWith(setting.Topic)) return;

        ConstructCarModelFile(msg);
    }

    private void ConstructCarModelFile(MqttMsgPublishEventArgs msg)
    {
        var filePath = Application.dataPath + "/_Project/Models/Car.glb";

        if (Encoding.UTF8.GetString(msg.Message) == "EOF")
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(fileStream);
            }  
            memoryStream.SetLength(0);
            memoryStream.Position = 0;
        }
        else memoryStream.Write(msg.Message, 0, msg.Message.Length);
    }
}
