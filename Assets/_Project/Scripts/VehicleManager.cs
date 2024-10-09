using GLTFast;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

[RequireComponent(typeof(ServerMQTTConnection))]
public class VehicleManager : MonoBehaviour
{
    private enum CarStatus
    {
        Idle = 0,
        Runnning = 1,
        Finish = 2
    }

    private class CarParamter
    {
        public int id;
        public float acceleration;
        public int gate;
        public bool ready;

        public CarParamter (int id)
        {
            this.id = id;
            acceleration = 0;
            gate = 0;
            ready = false;
        }
    }

    [SerializeField] private Commands commands;

    [SerializeField] private MQTTPublishSetting publishSetting;

    [SerializeField] private MQTTSubscriptionSetting registrationSubscriptionSetting;

    [SerializeField] private MQTTSubscriptionSetting positionSubscriptionSetting;

    private Dictionary<int, MemoryStream> memoryStreamDict = new Dictionary<int, MemoryStream>();

    private Dictionary<int, Car> carDict = new Dictionary<int, Car>();

    private List<Car> carList;

    private List<CarParamter> waitingCarList = new List<CarParamter>();

    private ServerMQTTConnection mqttConnection;

    private string carFilePath = Application.dataPath + "/_Project/Models/Car_";

    private void Awake()
    {
        mqttConnection = GetComponent<ServerMQTTConnection>();
    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs message)
    {
        ProcessTopic(message.Topic, out var preTopic, out var postTopic);

        if (preTopic == registrationSubscriptionSetting.Topic) ProcessSubscription(postTopic, message.Message);

        if (preTopic == positionSubscriptionSetting.Topic) ProcessPosition(postTopic, message.Message);
    }

    private void ProcessTopic(string topic, out string preTopic, out string postTopic)
    {
        var delimiterIndex = topic.IndexOf("/");
        preTopic = topic[..delimiterIndex];
        postTopic = topic[(delimiterIndex + 1)..];
    }

    private void ProcessSubscription(string topic, byte[] message)
    {
        ProcessTopic(topic, out var preTopic, out var postTopic);

        var carId = int.Parse(postTopic);

        var carParameter = GetCarParamter(carId);

        switch (preTopic)
        {
            case "Model":
                ConstructCarModelFile(carId, message); 
                break;
            case "Acceleration":
                SetAcceleration(carParameter, Encoding.UTF8.GetString(message));
                break;
            case "Gate":
                SetGate(carParameter, Encoding.UTF8.GetString(message));
                break;
            case "Ready":
                SetReady(carParameter, Encoding.UTF8.GetString(message));
                break;
        }
    }

    private CarParamter GetCarParamter(int carId)
    {
        var carParameter = waitingCarList.Find((car) => car.id == carId);

        if (carParameter == null)
        {
            carParameter = new CarParamter(carId);
            waitingCarList.Add(carParameter);
        }

        return carParameter;
    }

    private void ConstructCarModelFile(int id, byte[] message)
    {
        var filePath = carFilePath + id + ".glb";

        if (!memoryStreamDict.ContainsKey(id)) memoryStreamDict.Add(id, new MemoryStream());

        var memoryStream = memoryStreamDict.GetValueOrDefault(id);

        if (Encoding.UTF8.GetString(message) == "EOF")
        {
            if (File.Exists(filePath)) File.Delete(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                memoryStream.WriteTo(fileStream);
            }
            memoryStream.Close();
            memoryStreamDict.Remove(id);
        }
        else memoryStream.Write(message, 0, message.Length);
    }

    private void SetAcceleration(CarParamter carParamter, string message) 
    {
        var acceleration = float.Parse(message);
        carParamter.acceleration = acceleration;
    }

    private void SetGate(CarParamter carParamter, string message)
    {
        var gate = int.Parse(message);
        carParamter.gate = gate;
    }

    private void SetReady(CarParamter carParamter, string message)
    {
        var ready = int.Parse(message);
        carParamter.ready = ready != 0;
    }

    private void Update()
    {
        CheckForUninstantiatedCars();
    }

    private void CheckForUninstantiatedCars()
    {
        if (waitingCarList.Count <= 0) return;

        for (int i = waitingCarList.Count - 1; i >= 0; i--)
        {
            if (!waitingCarList[i].ready) continue;
            
            int id = waitingCarList[i].id;
            
            var newCar = InitCarObject(id);
            InstantiateCarModel(id, newCar);

            waitingCarList.RemoveAt(i);
        }
    }

    private Car InitCarObject(int id)
    {
        var newGameobject = new GameObject($"Car_{id}");
        var newCar = newGameobject.AddComponent<Car>();

        return newCar;
    }

    private async void InstantiateCarModel(int id, Car newCar)
    {
        var filePath = carFilePath + id + ".glb";
        var gltf = new GltfImport();

        if (!await gltf.Load(filePath))
        {
            Debug.LogError($"Fail to load car model file: {id}.");
            return;
        }

        if (!await gltf.InstantiateMainSceneAsync(newCar.transform)) Debug.LogError($"Fail to instantiate car instance: {id}.");

        AddCarToManagementData(id, newCar);

        InitCarState(id);
    }

    private void AddCarToManagementData(int id, Car newCar)
    {
        if (carDict.ContainsKey(id)) carDict.Remove(id);

        carDict.Add(id, newCar);

        var existedCarInTheList = carList.Find((car) => car.Id == id);

        if (existedCarInTheList != null) carList.Remove(existedCarInTheList);
        
        carList.Add(newCar);
    }

    private void InitCarState(int id)
    {
        GiveChangeDirectionCommand(id, new Vector2(1, 0));
        GiveChangeVelocityCommand(id, 10);
        GiveChangeStatusCommand(id, CarStatus.Runnning);
    }

    private void ProcessPosition(string postTopic, byte[] message)
    {

    }

    private void GiveChangeVelocityCommand(int carId, float desiredVelocity)
    {
        string topic = publishSetting.Topic + "/" + commands.ChangeVelocity + "/" + carId;

        string message = desiredVelocity.ToString();

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveChangeDirectionCommand(int carId, Vector2 desiredDirection)
    {
        string topic = publishSetting.Topic + "/" + commands.ChangeDirection + "/" + carId;

        string message = desiredDirection.x + "_" + desiredDirection.y;

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveChangeStatusCommand(int carId, CarStatus status)
    {
        string topic = publishSetting.Topic + "/" + commands.ChangeDirection + "/" + carId;

        string message = ((int)status).ToString();

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }
}
