using GLTFast;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

using Random = UnityEngine.Random;
using Direction = Intersection.Direction;
using static Intersection;

[RequireComponent(typeof(ServerMQTTConnection))]
public class VehicleManager : MonoBehaviour
{
    private class CarParamter
    {
        public int id;
        public int startGate;
        public int endGate;
        public bool ready;

        public CarParamter(int id)
        {
            this.id = id;
            startGate = 0;
            endGate = 0;
            ready = false;
        }
    }

    [SerializeField] private Commands commands;

    [SerializeField] private MQTTPublishSetting publishSetting;

    [SerializeField] private MQTTSubscriptionSetting registrationSubscriptionSetting;

    [SerializeField] private MQTTSubscriptionSetting positionSubscriptionSetting;

    [SerializeField] private MQTTSubscriptionSetting disconnectSubscriptionSetting;

    [SerializeField] private MapSetting mapSetting;

    private Dictionary<int, MemoryStream> memoryStreamDict = new Dictionary<int, MemoryStream>();

    private Dictionary<int, Car> carDict = new Dictionary<int, Car>();

    private List<Car> carList = new List<Car>();

    private List<CarParamter> waitingCarList = new List<CarParamter>();

    private Dictionary<int, GateController> gateDict = new Dictionary<int, GateController>();

    private ServerMQTTConnection mqttConnection;

    private string carFilePath = Application.dataPath + "/_Project/Models/Car_";

    private void Awake()
    {
        mqttConnection = GetComponent<ServerMQTTConnection>();
    }

    private void Start()
    {
        mqttConnection.AddMessageReceivedCallback(OnMessageReceived);

        AddSubscriptionIntersections();

        AddSubscriptionEndPoint();

        AddSubscribeCarBlock();

        GetGateDict();
    }

    private void AddSubscriptionIntersections()
    {
        var intersections = FindObjectsOfType<Intersection>();
        foreach (var intersection in intersections)
        {
            intersection.SubscribeCarEnterIntersection(OnIntersectionEnter);
        }
    }

    private void AddSubscriptionEndPoint()
    {
        var endPoints = FindObjectsOfType<EndPoint>();
        foreach (var endPoint in endPoints)
        {
            endPoint.SubscribeOnCarReachTheEnd(OnCarReachTheEnd);
        }
    }

    private void AddSubscribeCarBlock()
    {
        var collisionCheckingServers = FindObjectsOfType<CollisionCheckingServer>();
        foreach (var collisionCheckingServer in collisionCheckingServers)
        {
            collisionCheckingServer.SubscribeOnCarBlock(OnCarBlock);
            collisionCheckingServer.SubscribeOnCarUnblock(OnCarUnblock);
        }
    }

    private void GetGateDict()
    {
        var gateList = FindObjectsOfType<GateController>();

        foreach (var gate in gateList)
        {
            if (gateDict.ContainsKey(gate.Gate.Id))
            {
                Debug.LogError($"Duplicate gate: {gate.Gate.Id}");
                continue;
            }

            gateDict.Add(gate.Gate.Id, gate);
        }
    }

    private void OnMessageReceived(object sender, MqttMsgPublishEventArgs message)
    {
        if (message.Topic == disconnectSubscriptionSetting.Topic)
        {
            ProcessDisconnect(message.Message);
            return;
        }

        ProcessTopic(message.Topic, out var preTopic, out var postTopic);

        if (registrationSubscriptionSetting.Topic.StartsWith(preTopic)) ProcessSubscription(postTopic, message.Message);

        if (positionSubscriptionSetting.Topic.StartsWith(preTopic)) ProcessPosition(postTopic, message.Message);
    }

    private void ProcessDisconnect(byte[] message)
    {
        int carId = int.Parse(Encoding.UTF8.GetString(message));
        var isCarExist = carDict.TryGetValue(carId, out var car);
        if (isCarExist) car.isAlive = false;
        else ClearWaitingData(carId);
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
        for (int i = carList.Count - 1; i >= 0; i--)
        {
            if (carList[i].isAlive)
            {
                carList[i].UpdateCarData();
                continue;
            }

            OnCarDisconnect(carList[i]);
            carList.RemoveAt(i);
        }
    }

    private void CheckForUninstantiatedCars()
    {
        if (waitingCarList.Count <= 0) return;

        for (int i = waitingCarList.Count - 1; i >= 0; i--)
        {
            if (!waitingCarList[i].ready) continue;

            if (!IsGateAvailable(waitingCarList[i].startGate)) continue;

            TryCreateCar(waitingCarList[i]);

            waitingCarList.RemoveAt(i);
        }
    }

    private bool IsGateAvailable(int gateId)
    {
        var isGateExist = gateDict.TryGetValue(gateId, out GateController gate);
        if (!isGateExist)
        {
            Debug.LogError($"Gate {gateId} doesn't exist.");
            return isGateExist;
        }

        if (gate.IsSlotAvailable)
        {
            gate.OccupySlot();
            return true;
        }


        return gate.IsSlotAvailable;
    }

    private void TryCreateCar(CarParamter carParameter)
    {
        bool isGettingGateSuccess = mapSetting.GetGateSetting(carParameter.startGate, out var gate);

        if (!isGettingGateSuccess)
        {
            Debug.LogError($"Fail to get gate from car: {carParameter.id}");
            return;
        }

        var newCar = InitCarObject(carParameter, gate.Position, gate.Direction, carParameter.startGate, carParameter.endGate);

        InstantiateCarModel(newCar);
    }

    private Car InitCarObject(CarParamter carParameter, Vector2 position, Vector2 direction, int startGate, int endGate)
    {
        var newGameobject = new GameObject($"Car_{carParameter.id}");

        var newCar = newGameobject.AddComponent<Car>();
        newCar.InitializeCar(carParameter.id, direction, position, startGate, endGate);

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

        newCar.StartLate();

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
        bool isGettingGateSuccess = mapSetting.GetGateSetting(car.StartGate, out var gate);

        if (!isGettingGateSuccess)
        {
            Debug.LogError($"Fail to get gate from car: {car.Id}");
            return;
        }

        SetPositionCommand(car.Id, new Vector2(car.transform.position.x, car.transform.position.z));
        GiveSetNextIntersectionCommand(car.Id, gate.AdjacentIntersectionPoint.Coordination);
        GiveSetDirectionCommand(car.Id, MathSupport.AngleToVector(car.transform.eulerAngles.y));
        GiveChangeStatusCommand(car, CarStatus.Running);
    }

    private void SetPositionCommand(int carId, Vector2 position)
    {
        string topic = publishSetting.Topic + "/" + commands.SetPosition + "/" + carId;

        string message = position.x + "/" + position.y;

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

    private void GiveSetNextIntersectionCommand(int carId, Vector2 nextIntersection)
    {
        string topic = publishSetting.Topic + "/" + commands.SetNextIntersection + "/" + carId;

        string message = nextIntersection.x + "/" + nextIntersection.y;

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }

    private void GiveChangeStatusCommand(Car car, CarStatus status)
    {
        car.status = status;
        string topic = publishSetting.Topic + "/" + commands.ChangeStatus + "/" + car.Id;

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

        car.position = new Vector2(x, z);
    }

    private void SetDirection(Car car, string message)
    {
        var delimiterIndex = message.IndexOf("/");
        float x = float.Parse(message[..delimiterIndex]);
        float z = float.Parse(message[(delimiterIndex + 1)..]);

        car.direction = new Vector2(x, z); ;
    }

    private void SetVelocity(Car car, string message)
    {
        var velocity = float.Parse(message);
        car.velocity = velocity;
    }

    private void OnIntersectionEnter(int carId, HashSet<Direction> availableDirections, List<AdjacentIntersectionPoint> adjacentIntersectionPoints)
    {
        var isCarExist = carDict.TryGetValue(carId, out var car);
        if (!isCarExist)
        {
            Debug.LogError($"Car {carId} does not exist.");
            return;
        }

        int random = Random.Range(0, availableDirections.Count);

        var direction = availableDirections.ToArray()[random];
        var newRotation = direction switch
        {
            Direction.Left => car.transform.localEulerAngles.y - 90,
            Direction.Right => car.transform.localEulerAngles.y + 90,
            Direction.Straight => car.transform.localEulerAngles.y,
            _ => car.transform.localEulerAngles.y,
        };
        var nextIntersectionPoint = adjacentIntersectionPoints.Find((point) => point.Direction == direction).IntersectionPoint.Coordination;
        
        GiveSetNextIntersectionCommand(carId, nextIntersectionPoint);
        GiveChangeDirectionCommand(carId, MathSupport.AngleToVector(newRotation));
        
        if (car.status != CarStatus.Blocking) GiveChangeStatusCommand(car, CarStatus.Running);
    }

    private void OnCarReachTheEnd(Car car)
    {
        carList.Remove(car);
        carDict.Remove(car.Id);

        Destroy(car.gameObject);

        var filePath = carFilePath + car.Id + ".glb";
        if (File.Exists(filePath)) File.Delete(filePath);

        GiveChangeStatusCommand(car, CarStatus.Finish);
    }

    private void OnCarDisconnect(Car car)
    {
        ClearWaitingData(car.Id);

        carDict.Remove(car.Id);

        Destroy(car.gameObject);

        var filePath = carFilePath + car.Id + ".glb";
        if (File.Exists(filePath)) File.Delete(filePath);
    }


    private void ClearWaitingData(int carId)
    {
        if (memoryStreamDict.ContainsKey(carId)) memoryStreamDict.Remove(carId);

        var carParameter = waitingCarList.Find((carParameter) => carParameter.id == carId);
        if (carParameter != null) waitingCarList.Remove(carParameter);
    }

    private void OnCarBlock(int carId)
    {
        if (!carDict.TryGetValue(carId, out var car)) return;

        car.blockCount++;

        GiveChangeStatusCommand(car, CarStatus.Blocking);
    }

    private void OnCarUnblock(int carId)
    {
        if (!carDict.TryGetValue(carId, out var car)) return;

        car.blockCount--;

        car.blockCount = Mathf.Max(car.blockCount, 0);

        if (car.blockCount == 0) GiveChangeStatusCommand(car, CarStatus.Running);
    }
}
public enum CarStatus
{
    Idle = 0,
    Running = 1,
    Waiting = 2,
    Blocking = 3,
    Finish = 4
}
public struct CollisionSegment
{
    private Vector2 tail;

    public Vector2 Tail => tail;

    private Vector3 head;

    public Vector2 Head => head;

    public CollisionSegment(Vector2 head, Vector2 tail)
    {
        this.head = head;
        this.tail = tail;
    }
}
