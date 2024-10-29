using UnityEngine;
using static CameraConfig;

public class CameraMain : MonoBehaviour
{

    [SerializeField] private CameraConfig cameraConfig;


    private Vector3 lastCameraPos;

    private Vector3 lastCameraMoveDirection;

    private Vector3 moveSpeed;

    private CameraAxisBounder currentXBounder;

    private CameraAxisBounder currentZBounder;

    private Vector3 lastMousePos;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GetComponent<Camera>();

        UpdateLastCameraPos();
    }

    private void Start()
    {
        CalculateMoveBounder();
    }

    private void LateUpdate()
    {
        DoMouse();
    }

    private void DoMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseDown(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Mouse(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            MouseUp();
        }
        else
        {
            NoMouse();
        }

        ZoomCamera(Input.mouseScrollDelta.y);
    }

    private void MouseDown(Vector3 newMousePos)
    {
        lastMousePos = newMousePos;
        CameraStopMoving();
    }

    private void Mouse(Vector3 newMousePos)
    {
        var direction = new Vector3(lastMousePos.x - newMousePos.x, 0, lastMousePos.y - newMousePos.y);

        CameraControlMove(direction);
        UpdateLastCameraMoveDirection(direction);
        lastMousePos = newMousePos;
    }

    private void MouseUp()
    {
        SetFreeMoveDestination();
    }

    private void NoMouse()
    {
        CameraFreeMove();
    }

    private void CalculateMoveBounder()
    {
        currentXBounder = GetBounder(cameraConfig.XBounder);

        currentZBounder = GetBounder(cameraConfig.ZBounder);
    }

    private CameraAxisBounder GetBounder(CameraZoomBounder cameraZoomBounder)
    {
        return new CameraAxisBounder
        {
            min = GetRespectiveValue(cameraZoomBounder.minOrthographicSizeBounder.min,
                cameraZoomBounder.maxOrthographicSizeBounder.min),
            max = GetRespectiveValue(cameraZoomBounder.minOrthographicSizeBounder.max,
                cameraZoomBounder.maxOrthographicSizeBounder.max)
        };
    }

    private float GetRespectiveValue(float minValue, float maxValue)
    {
        var proportion = (mainCamera.orthographicSize - cameraConfig.HardMinOrthographicSize) /
                         (cameraConfig.HardMaxOrthographicSize - cameraConfig.HardMinOrthographicSize);
        return proportion * (maxValue - minValue) + minValue;
    }

    private void CameraStopMoving()
    {
        moveSpeed = Vector3.zero;
        UpdateLastCameraPos();
        lastCameraMoveDirection = Vector3.zero;
    }

    private void UpdateLastCameraPos()
    {
        lastCameraPos = transform.position;
    }

    private void CameraControlMove(Vector3 moveDirection)
    {
        if (moveDirection.sqrMagnitude < Mathf.Epsilon) return;
        var translation = GetCameraTranslation(moveDirection, cameraConfig.CameraDraggingSpeed);
        lastCameraPos = ClampPosition(transform.position + translation);
        LerpMove(lastCameraPos, cameraConfig.CameraDraggingSmooth);
    }

    private Vector3 GetCameraTranslation(Vector3 moveDirection, float translationSpeed)
    {
        return GetCorrectMoveDirection(moveDirection) * (translationSpeed * Time.deltaTime);
    }

    private Vector3 GetCorrectMoveDirection(Vector3 targetPosition)
    {
        var radius = targetPosition.magnitude;
        var angle = Mathf.Atan2(targetPosition.z, targetPosition.x) -
                    transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        targetPosition.x = Mathf.Cos(angle) * radius;
        targetPosition.z = Mathf.Sin(angle) * radius;
        return targetPosition;
    }

    private Vector3 ClampPosition(Vector3 currentPosition)
    {
        currentPosition.x = Mathf.Clamp(currentPosition.x, currentXBounder.min, currentXBounder.max);
        currentPosition.y = transform.position.y;
        currentPosition.z = Mathf.Clamp(currentPosition.z, currentZBounder.min, currentZBounder.max);

        return currentPosition;
    }

    private void LerpMove(Vector3 targetPosition, float lerpSmooth)
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSmooth);
    }

    private void SmoothDampMove(Vector3 targetPosition, float smoothDampSmooth)
    {
        transform.position =
            Vector3.SmoothDamp(transform.position, targetPosition, ref moveSpeed, smoothDampSmooth);
    }

    private void UpdateLastCameraMoveDirection(Vector3 direction)
    {
        lastCameraMoveDirection = direction;
    }

    private void CameraFreeMove()
    {
        if ((lastCameraPos - transform.position).sqrMagnitude < Mathf.Epsilon) return;

        CalculateInitialMoveVelocity(transform.position, cameraConfig.CameraFreeMoveSmooth);

        SmoothDampMove(lastCameraPos, cameraConfig.CameraFreeMoveSmooth);
    }

    private void SetFreeMoveDestination()
    {
        lastCameraPos = ClampPosition(transform.position + GetCameraTranslation(
            lastCameraMoveDirection,
            cameraConfig.CameraFreeMoveSpeed));
    }

    private void CalculateInitialMoveVelocity(Vector3 currentCameraPosition, float smooth, float multiply = 2)
    {
        moveSpeed.x = (lastCameraPos.x - currentCameraPosition.x) / smooth;
        moveSpeed.y = 0;
        moveSpeed.z = (lastCameraPos.z - currentCameraPosition.z) / smooth;
        moveSpeed *= multiply;
    }

    private void ZoomCamera(float zoomDelta)
    {
        if (Mathf.Abs(zoomDelta) < Mathf.Epsilon) return;

        var currentOrthographicSize = mainCamera.orthographicSize;
        var zoomChanged = zoomDelta * cameraConfig.ZoomSpeed * Time.deltaTime;
        var targetOrthographicSize = Mathf.Clamp(currentOrthographicSize - zoomChanged,
            cameraConfig.HardMinOrthographicSize,
            cameraConfig.HardMaxOrthographicSize);
        mainCamera.orthographicSize = Mathf.Lerp(currentOrthographicSize, targetOrthographicSize,
            cameraConfig.ZoomDraggingSmooth);

        RecalculateCamera();
    }

    private void RecalculateCamera()
    {
        CalculateMoveBounder();
        var newPosition = ClampPosition(transform.position);
        transform.position = lastCameraPos = newPosition;
    }

}