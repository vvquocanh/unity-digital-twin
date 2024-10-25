using UnityEngine;

public class GateController : MonoBehaviour
{
    [SerializeField] private Gate gate;

    public Gate Gate => gate;

    private bool isSlotAvailable = true;

    public bool IsSlotAvailable => isSlotAvailable;

    private void OnTriggerExit(Collider other)
    {
        isSlotAvailable = true;
    }

    public void OccupySlot()
    {
        isSlotAvailable = false;
    }
}
