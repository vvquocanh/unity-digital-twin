using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

[RequireComponent(typeof(ServerMQTTConnection))]
public class ModelConstructor : MonoBehaviour
{
    [SerializeField] private MQTTSubscriptionSetting setting;

    private ServerMQTTConnection mqttConnection;

    private Dictionary<int, MemoryStream> memoryStreamDict = new Dictionary<int, MemoryStream>();

    private Action<int> onCarModelConstructed;

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
        var topic = msg.Topic;

        int id = int.Parse(topic.Substring(topic.IndexOf("/") + 1));

        var filePath = Application.dataPath + "/_Project/Models/Car_"+ "_" + id + ".glb";

        if (!memoryStreamDict.ContainsKey(id)) memoryStreamDict.Add(id, new MemoryStream());

        var memoryStream = memoryStreamDict.GetValueOrDefault(id);

        if (Encoding.UTF8.GetString(msg.Message) == "EOF")
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(fileStream);
            }
            memoryStream.Close();
            memoryStreamDict.Remove(id);

            onCarModelConstructed?.Invoke(id);
        }
        else memoryStream.Write(msg.Message, 0, msg.Message.Length);
    }

    public void AddCarModelConstructedCallback(Action<int> callback)
    {
        onCarModelConstructed += callback;
    }

    public void RemoveCarModelContructedCallback(Action<int> callback)
    {
        onCarModelConstructed -= callback;
    }
}
