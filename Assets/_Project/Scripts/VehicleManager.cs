using GLTFast;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ModelConstructor), typeof(ServerMQTTConnection))]
public class VehicleManager : MonoBehaviour
{
    private enum CarStatus
    {
        Idle = 0,
        Runnning = 1,
        Finish = 2
    }

    [SerializeField] private Commands commands;

    [SerializeField] private MQTTPublishSetting publishSetting;

    private Dictionary<int, Transform> carDict = new Dictionary<int, Transform>();

    private List<int> waitingCarIds = new List<int>();

    private ModelConstructor modelConstructor;

    private ServerMQTTConnection mqttConnection;

    private void Awake()
    {
        modelConstructor = GetComponent<ModelConstructor>();
        mqttConnection = GetComponent<ServerMQTTConnection>();
    }

    private void Start()
    {
        modelConstructor.AddCarModelConstructedCallback(OnCarModelConstructed);
    }

    private void Update()
    {
        CheckForUninstantiatedCars();
    }

    private void CheckForUninstantiatedCars()
    {
        if (waitingCarIds.Count <= 0) return;

        foreach (var id in waitingCarIds)
        {
            var newCar = new GameObject($"Car_{id}");
            InstantiateCarModel(id, newCar);
        }
        waitingCarIds.Clear();
    }

    void OnCarModelConstructed(int id)
    {
        waitingCarIds.Add(id);
    }

    private async void InstantiateCarModel(int id, GameObject newCar)
    {
        var filePath = Application.dataPath + "/_Project/Models/Car_" + "_" + id + ".glb";
        var gltf = new GltfImport();

        if (!await gltf.Load(filePath))
        {
            Debug.LogError($"Fail to load car model file: {id}.");
            return;
        }

        if (!await gltf.InstantiateMainSceneAsync(newCar.transform)) Debug.LogError($"Fail to instantiate car instance: {id}.");

        if (carDict.ContainsKey(id)) carDict.Remove(id);

        carDict.Add(id, newCar.transform);

        InitCarState(id);
    }

    private void InitCarState(int id)
    {
        GiveChangeDirectionCommand(id, new Vector2(1, 0));
        GiveChangeVelocityCommand(id, 10);
        GiveChangeStatusCommand(id, CarStatus.Runnning);
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

        string message = ((int) status).ToString();

        mqttConnection.Publish(topic, message, publishSetting.Qos, publishSetting.Retain);
    }
}
