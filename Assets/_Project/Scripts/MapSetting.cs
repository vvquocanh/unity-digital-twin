using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/MapSetting", fileName = "NewMapSetting")]
public class MapSetting : ScriptableObject
{
    [Serializable]
    private struct GateSetting
    {
        [SerializeField] private int id;

        public int Id => id;

        [SerializeField] private Vector2 position;

        public Vector2 Position => position;

        [SerializeField] private float directionAngle;

        public float DirectionAngle => directionAngle;
    }

    [SerializeField] private List<GateSetting> settings;

    private Dictionary<int, GateSetting> gateDict = new Dictionary<int, GateSetting>();

    public bool GetGateSetting(int id, out Vector2 position, out float direction)
    {
        if (gateDict == null || gateDict.Count == 0)
        {
            foreach (var setting in settings)
            {
                gateDict.Add(setting.Id, setting);
            }
        }

        var isGettingGateSuccess = gateDict.TryGetValue(id, out var gateSetting);

        position = isGettingGateSuccess ? gateSetting.Position : Vector2.zero;
        direction = isGettingGateSuccess ? gateSetting.DirectionAngle : 0f;

        return isGettingGateSuccess;
    }
}
