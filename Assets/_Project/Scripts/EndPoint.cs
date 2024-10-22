using UnityEngine;

public abstract class EndPoint : ScriptableObject
{
    [SerializeField] protected int id;

    public int Id => id;

    public abstract bool IsDestination(int checkId);
}
