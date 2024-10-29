using System;
using UnityEngine;

[CreateAssetMenu(menuName = "MQTT/CameraConfig", fileName = "NewCameraConfig")]
public class CameraConfig : ScriptableObject
{
    [Serializable]
    public struct CameraZoomBounder
    {
        public CameraAxisBounder minOrthographicSizeBounder;
        public CameraAxisBounder maxOrthographicSizeBounder;
    }

    [Serializable]
    public struct CameraAxisBounder
    {
        public float min;
        public float max;
    }

    #region Move

    [Header("Movement")]

    // Dragging
    [SerializeField] private float cameraDraggingSpeed = 30f;

    [SerializeField] private float cameraDraggingSmooth = 0.5f;

    // Free move
    [SerializeField] private float cameraFreeMoveSpeed = 10f;

    [SerializeField] private float cameraFreeMoveSmooth = 0.7f;

    public float CameraDraggingSpeed => cameraDraggingSpeed;

    public float CameraDraggingSmooth => cameraDraggingSmooth;

    public float CameraFreeMoveSpeed => cameraFreeMoveSpeed;

    public float CameraFreeMoveSmooth => cameraFreeMoveSmooth;

    #endregion

    #region Bounder

    [Header("Movement Bounder")]

    // X bounder
    [SerializeField] private CameraZoomBounder xBounder;

    // Z bounder
    [SerializeField] private CameraZoomBounder zBounder;

    public CameraZoomBounder XBounder => xBounder;

    public CameraZoomBounder ZBounder => zBounder;

    #endregion

    #region Zoom

    // Zoom
    [Header("Zoom")]
    // Dragging
    [SerializeField] private float zoomSpeed = 300f;

    [SerializeField] private float zoomDraggingSmooth = 0.2f;

    public float ZoomSpeed => zoomSpeed;

    public float ZoomDraggingSmooth => zoomDraggingSmooth;


    [Header("Zoom Bounder")]

    [SerializeField] private float hardMinOrthographicSize = 40f;

    [SerializeField] private float hardMaxOrthographicSize = 170f;

    public float HardMinOrthographicSize => hardMinOrthographicSize;

    public float HardMaxOrthographicSize => hardMaxOrthographicSize;


    #endregion


}

