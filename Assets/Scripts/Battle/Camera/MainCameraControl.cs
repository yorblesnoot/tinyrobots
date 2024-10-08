using Cinemachine;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainCameraControl : MonoBehaviour
{

    [SerializeField] CameraSet cams;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] float scrollZoneX;
    [SerializeField] float scrollZoneY;
    [SerializeField] float zoomSpeed;
    [SerializeField] float actionCutDuration = 2f;
    [SerializeField] float actionCutMaxZoom = 5f;
    [SerializeField] int focalPointDeadzone = 5;
    [SerializeField] CinemachineConfiner[] confiners;

    Vector3Int mapCorner;
    static CameraSet Cams;
    string xInput;
    string yInput;

    public static MainCameraControl Instance;
    private void OnEnable()
    {
        EnableInputSystem();
    }

    #region Initialization
    public void Initialize(byte[,,] map)
    {
        mapCorner = new(map.GetLength(0), map.GetLength(1), map.GetLength(2));
        ConfineCameras(mapCorner);
        Cams = cams;
        Cams.FocalPoint = transform;
        Instance = this;
        RestrictCamera(false);
        xInput = Cams.Free.m_XAxis.m_InputAxisName;
        yInput = Cams.Free.m_YAxis.m_InputAxisName;
        LockCameraPivot();
    }
    void ConfineCameras(Vector3Int corner)
    {
        GameObject boundingBox = new();
        boundingBox.layer = LayerMask.NameToLayer("Ignore Raycast");
        BoxCollider boundingCollider = boundingBox.AddComponent<BoxCollider>();
        Vector3 bounds = corner;
        bounds.x -= focalPointDeadzone;
        bounds.y -= focalPointDeadzone;
        bounds.z -= focalPointDeadzone;

        Vector3 center = corner;
        center /= 2;
        boundingCollider.size = bounds;
        boundingCollider.center = center;
        foreach(var cam in confiners)
        {
            cam.m_BoundingVolume = boundingCollider;
        }
    }

    BaseInput playerInput;
    InputAction rotator;
    InputAction rotateHold;
    InputAction rotateOn;
    private void EnableInputSystem()
    {
        playerInput = new();
        rotator = playerInput.Main.RotatePosition;
        rotator.Enable();

        rotateHold = playerInput.Main.Rotating;
        rotateHold.Enable();
        rotateHold.canceled += EndFocusRotation;

        rotateOn = playerInput.Main.RotateOn;
        rotateOn.Enable();
        rotateOn.performed += BeginFocusRotation;
    }
    #endregion

    void BeginFocusRotation(InputAction.CallbackContext context)
    {
        if (!freeCameraAvailable) return;
        ToggleAutoCam(false);

        Cams.Free.m_XAxis.m_InputAxisName = xInput;
        Cams.Free.m_YAxis.m_InputAxisName = yInput;
    }

    void EndFocusRotation(InputAction.CallbackContext context)
    {
        LockCameraPivot();
    }

    static bool freeCameraAvailable = true;
    public static void RestrictCamera(bool value = true)
    {
        freeCameraAvailable = !value;
    }

    private void LateUpdate()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl)) RestrictCamera(freeCameraAvailable);
        if(freeCameraAvailable) PlayerControlCamera();
        cams.Brain.ManualUpdate();
        Cams.Automatic.m_MinDuration = 50;
    }

    private void PlayerControlCamera()
    {
        Vector3 mouse = Input.mousePosition;

        if (Input.GetKey(KeyCode.W))
        {
            Zoom(zoomSpeed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            Zoom(-zoomSpeed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            SlideCamera(new Vector3(-scrollZoneX / 2, 0f, 0f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            SlideCamera(new Vector3(scrollZoneX / 2, 0f, 0f));
        }
        else if(!rotateHold.inProgress)
        {
            PanCamera(mouse);
        }
    }

    private void PanCamera(Vector3 mouse)
    {
        float leftScroll = Mathf.Clamp(scrollZoneX - mouse.x, 0, float.MaxValue);
        float rightScroll = Mathf.Clamp(mouse.x - (Screen.width - scrollZoneX), 0, float.MaxValue);
        float downScroll = Mathf.Clamp(scrollZoneY - mouse.y, 0, float.MaxValue);
        float upScroll = Mathf.Clamp(mouse.y - (Screen.height - scrollZoneY), 0, float.MaxValue);
        Vector3 moveOffset = new(rightScroll - leftScroll, upScroll - downScroll, 0f);

        if (moveOffset != Vector3.zero) SlideCamera(moveOffset);
    }

    private void SlideCamera(Vector3 moveOffset)
    {
        if(!freeCameraAvailable) return;
        Quaternion yRotation = Camera.main.transform.rotation;

        moveOffset *= scrollSpeed;
        Vector3 slidPosition = transform.position + yRotation * moveOffset;
        transform.position = slidPosition;
        ClampFocusInMap();
    }

    private void Zoom(float factor)
    {
        Vector3 direction = Cams.FocalPoint.position - Camera.main.transform.position;
        direction.Normalize();
        direction *= factor;
        transform.position += direction;
        ClampFocusInMap();
    }

    static void LockCameraPivot()
    {
        Cams.Free.m_YAxis.m_InputAxisName = "";
        Cams.Free.m_XAxis.m_InputAxisName = "";
        Cams.Free.m_YAxis.m_InputAxisValue = 0f;
        Cams.Free.m_XAxis.m_InputAxisValue = 0f;
    }

    void ClampFocusInMap()
    {
        Vector3 minimum = Vector3.one * focalPointDeadzone;
        Vector3 maximum = mapCorner - minimum;
        transform.position = transform.position.Clamp(minimum, maximum);
    }

    public static void CutToUnit(TinyBot bot, bool autoCam = true)
    {
        Cams.FocalPoint.transform.position = bot.TargetPoint.position;
        if (autoCam) ToggleAutoCam();
    }

    static void ToggleAutoCam(bool on = true)
    {
        Cams.Automatic.gameObject.SetActive(on);
        Cams.Free.Priority = on ? 0 : 3;
        Cams.Automatic.m_MinDuration = 0;
    }

    static bool tracking;

    public static void TrackTarget(Transform target)
    {
        if (tracking) return;
        tracking = true;
        RestrictCamera(true);
        Instance.StartCoroutine(TrackTowardsEntity(target));
    }

    public static void ReleaseTracking()
    {
        tracking = false;
        RestrictCamera(false);
    }

    static IEnumerator TrackTowardsEntity(Transform target)
    {
        while (tracking)
        {
            Cams.FocalPoint.transform.position = Vector3.Lerp(Cams.FocalPoint.transform.position, target.position, Time.deltaTime);
            yield return null;
        }
    }

    public static void ActionPanTo(Vector3 target)
    {
        RestrictCamera(true);
        Instance.StartCoroutine(ActionCut(target, Instance.actionCutDuration));
    }

    static IEnumerator ActionCut(Vector3 target, float duration)
    {
        Vector3 startPosition = Cams.FocalPoint.position;
        float timeElapsed = 0;
        while (timeElapsed < duration)
        {
            float progress = timeElapsed / duration;
            Cams.FocalPoint.transform.position = Vector3.Lerp(startPosition, target, .5f * progress);
            Instance.Zoom(Mathf.Lerp(0, -Instance.actionCutMaxZoom, progress));
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        RestrictCamera(false);
    }

    [System.Serializable]
    class CameraSet
    {
        public CinemachineFreeLook Free;
        public CinemachineClearShot Automatic;
        public CinemachineBrain Brain;
        public Transform FocalPoint;
    }
}
