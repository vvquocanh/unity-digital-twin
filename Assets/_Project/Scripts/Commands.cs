using UnityEngine;

[CreateAssetMenu(fileName = "NewCommands", menuName = "MQTT/Commands")]
public class Commands : ScriptableObject
{
    [SerializeField] private string setPosition;

    public string SetPosition => setPosition;

    [SerializeField] private string setDirection;

    public string SetDirection => setDirection;


    [SerializeField] private string changeDirection;

    public string ChangeDirection => changeDirection;

    [SerializeField] private string setNextIntersection;

    public string SetNextIntersection => setNextIntersection;

    [SerializeField] private string changeStatus;

    public string ChangeStatus => changeStatus;
}
