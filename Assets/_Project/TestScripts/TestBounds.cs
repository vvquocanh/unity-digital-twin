using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBounds : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var bounds = GetComponent<MeshRenderer>().localBounds.max;

        Debug.Log($"Object {gameObject.name}: {bounds}");
    }


}
