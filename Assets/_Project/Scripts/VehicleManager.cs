using GLTFast;
using System;
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
        Idle,
        Running,
        Finish
    }

    private class CarParamter
    {
        public int id;
        public float acceleration;
        public float modelRotationOffset;
        public int startGate;
        public int endGate;
        public bool ready;

        public CarParamter (int id)
        {
            this.id = id;
            acceleration = 0;
            modelRotationOffset = 0;
            startGate = 0;
            endGate = 0;
            ready = false;
        }
    }

    [SerializeField] private Commands commands;

    [SerializeField] private MQTTPublishSetting publishSetting;

    [SerializeField] private MQTTSubscriptionSetting registrationSubscriptionSetting;

    [SerializeField] private MQTTSubscriptionSetting positionSubscriptionSetting;

    [SerializeField] private MapSetting mapSetting;

    private Dictionary<int, MemoryStream> memoryStreamDict = new Dictionary<int, MemoryStream>();

    private Dictionary<int, Car> carDict = new Dictionary<int, Car>();

    private List<Car> carList = new List<Car>() ;

    private List<CarParamter> waitingCarList = new List<CarParamter>();

    private ServerMQTTConnection mqttConnection;

    private string carFilePath = Application.dataPath + "/_Project/Models/Car_";

    private void Awake()
    {
        mqttConnection = GetComponent<ServerMQTTConnection>();
    }

    private void Start()
    {
        mqttConnection.AddMessageReceivedCallback(OnMessageReceived);
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

        var messageString = Encoding.UTF8.GetString(message);

        switch (preTopic)
        {
            case "model":
                ConstructCarModelFile(carId, message); 
                break;
            case "rotation_offset":
                SetModelRotationOffset(carParameter, messageString);
                break;
            case "acceleration":
                SetAcceleration(carParameter, messageString);
                break;
            case "start_gate":
                SetStartGate(carParameter, messageString);
                break;
            case "end_gate":
                SetEndGate(carParameter, messageString);
                break;
            case "ready":
                SetReady(carParameter, messageString);
                break;
            default:
                Debug.LogError($"Unrecognize topic: {preTopic}");
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

    private void SetModelRotationOffset(CarParamter carParamter, string message) 
    {
        var rotationOffset = float.Parse(message);
        carParamter.modelRotationOffset = rotationOffset;
    }

    private void SetAcceleration(CarParamter carParamter, string message) 
    {
        var acceleration = float.Parse(message);
        carParamter.acceleration = acceleration;
    }

    private void SetStartGate(CarParamter carParamter, string message)
    {
        var gate = int.Parse(message);
        carParamter.startGate = gate;
    }

    private void SetEndGate(CarParamter carParamter, string message)
    {
        var gate = int.Parse(message);
        carParamter.endGate = gate;
    }

    private void SetReady(CarParamter carParamter, string message)
    {
        var ready = int.Parse(message);
        carParamter.ready = ready != 0;
    }

    private void Update()
    {
        CheckForUninstantiatedCars();

        UpdateCar();
    }

    private void UpdateCar()
    {
        foreach (var car in carList)
        {
            car.UpdateCarData();
        }
    }

    private void CheckForUninstantiatedCars()
    {
        if (waitingCarList.Count <= 0) return;

        for (int i = waitingCarList.Count - 1; i >= 0; i--)
        {
            if (!waitingCarList[i].ready) continue;

            TryCreateCar(waitingCarList[i]);

            waitingCarList.RemoveAt(i);
        }
    }

    private void TryCreateCar(CarParamter carParameter)
    {
        bool isGettingGateSuccess = mapSetting.GetGateSetting(carParameter.startGate, out var position, out var directionAngle);

        if (!isGettingGateSuccess)
        {
            Debug.LogError($"Fail to get gate from car: {carParameter.id}");
            return;
        }

        var newCar = InitCarObject(carParameter, position, directionAngle);

        InstantiateCarModel(newCar);
    }

    private Car InitCarObject(CarParamter carParameter, Vector2 position, float directionAngle)
    {
        var newGameobject = new GameObject($"Car_{carParameter.id}");

        var newCar = newGameobject.AddComponent<Car>();
        newCar.Id = carParameter.id;
        newCar.Acceleration = carParameter.acceleration;
        newCar.DirectionAngle = directionAngle;
        newCar.Position = position;
        newCar.ModelRotationOffset = carParameter.modelRotationOffset;
        newCar.transform.position = new Vector3(position.x, 0.2f, position.y);
        newCar.transform.eulerAngles = new Vector3(0, directionAngle, 0);

        return newCar;
    }

    private async void InstantiateCarModel(Car newCar)
    {
        var filePath = carFilePath + newCar.Id + ".glb";
        var gltf = new GltfImport();

        if (!await gltf.Load(filePath))
        {
            Debug.LogError($"Fail to load car model file: {newCar.Id}.");
            return;
        }

        if (!await gltf.InstantiateMainSceneAsync(newCar.transform)) Debug.LogError($"Fail to instantiate car instance: {newCar.Id}.");

        AddCarToManagementData(newCar);

        InitCarState(newCar);
    }

    private void AddCarToManagementData(Car newCar)
    {
        if (carDict.ContainsKey(newCar.Id)) carDict.Remove(newCar.Id);

        carDict.Add(newCar.Id, newCar);

        var existedCarInTheList = carList.Find((car) => car.Id == newCar.Id);

        if (existedCarInTheList != null) carList.Remove(existedCarInTheList);
        
        carList.Add(newCar);
    }

    private void InitCarState(Car car)
    {
        SetPositionCommand(car.Id, new Vector2(car.transform.position.x, car.transform.position.z));
        GiveSetDirectionCommand(car.Id, AngleToVector(car.transform.eulerAngles.y, car.ModelRotationOffset));
        GiveChangeVelocityCommand(car.Id, 10);
        GiveChangeStatusCommand(car.Id, CarStatus.Running);
    }

    private void SetPositionCommand(int carId, Vector2 position)
    {
        string topic = publishSetting.Topic + "/" + commands.SetPosition + "/" + carId;

        string message = position.x + "/" + position.y;

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveChangeVelocityCommand(int carId, float desiredVelocity)
    {
        string topic = publishSetting.Topic + "/" + commands.ChangeVelocity + "/" + carId;

        string message = desiredVelocity.ToString();

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveSetDirectionCommand(int carId, Vector2 desiredDirection)
    {
        string topic = publishSetting.Topic + "/" + commands.SetDirection + "/" + carId;

        string message = desiredDirection.x + "/" + desiredDirection.y;

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveChangeDirectionCommand(int carId, Vector2 desiredDirection)
    {
        string topic = publishSetting.Topic + "/" + commands.ChangeDirection + "/" + carId;

        string message = desiredDirection.x + "/" + desiredDirection.y;

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveChangeStatusCommand(int carId, CarStatus status)
    {
        string topic = publishSetting.Topic + "/" + commands.ChangeStatus + "/" + carId;

        int enumInt = (int)status;
        string message = enumInt.ToString();

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void ProcessPosition(string topic, byte[] message)
    {
        ProcessTopic(topic, out var preTopic, out var postTopic);

        var carId = int.Parse(postTopic);

        bool isCarExist = carDict.TryGetValue(carId, out var car);
        if (!isCarExist) return;

        var messageString = Encoding.UTF8.GetString(message);

        switch (preTopic)
        {
            case "position":
                SetPosition(car, messageString);
                break;
            case "direction":
                SetDirection(car, messageString);
                break;
            case "velocity":
                SetVelocity(car, messageString);
                break;
            default:
                Debug.LogError($"Unrecognize topic: {preTopic}");
                break;

        }
    }

    private void SetPosition(Car car, string message)
    {
        var delimiterIndex = message.IndexOf("/");
        float x = float.Parse(message[..delimiterIndex]);
        float z = float.Parse(message[(delimiterIndex + 1)..]);

        car.Position = new Vector2(x, z);
    }

    private void SetDirection(Car car, string message)
    {
        var delimiterIndex = message.IndexOf("/");
        float x = float.Parse(message[..delimiterIndex]);
        float z = float.Parse(message[(delimiterIndex + 1)..]);

        float angle = VectorToAngle(new Vector2(x, z), car.ModelRotationOffset);

        car.DirectionAngle = angle;
    }

    private void SetVelocity(Car car, string message)
    {
        var velocity = float.Parse(message);
        car.Velocity = velocity;
    }

    private Vector2 AngleToVector(float angle, float rotationOffset)
    {
        angle += rotationOffset;
        
        float x = (float)Math.Round(Mathf.Cos(angle * Mathf.Deg2Rad), 3);
        float y = (float)Math.Round(Mathf.Sin(angle * Mathf.Deg2Rad), 3);

        return new Vector2(x, y);
    }

    private float VectorToAngle(Vector2 vector, float rotationOffset)
    {
        float radians = (float)Mathf.Atan2(vector.y, vector.x);

        return radians * Mathf.Rad2Deg - rotationOffset;
    }
}
