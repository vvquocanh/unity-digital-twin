using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/MapSetting", fileName = "NewMapSetting")]
public class MapSetting : ScriptableObject
{
    [SerializeField] private List<Gate> gateList;

    private Dictionary<int, Gate> gateDict = new Dictionary<int, Gate>();

    public bool GetGateSetting(int id, out Vector2 position, out float direction)
    {
        if (gateDict == null || gateDict.Count == 0)
        {
            foreach (Gate gate in gateList)
            {
                gateDict.Add(gate.Id, gate);
            }
        }

        var isGettingGateSuccess = gateDict.TryGetValue(id, out var gateSetting);

        position = isGettingGateSuccess ? gateSetting.Position : Vector2.zero;
        direction = isGettingGateSuccess ? gateSetting.DirectionAngle : 0f;

        return isGettingGateSuccess;
    }
}
