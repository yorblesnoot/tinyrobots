using Cinemachine;
using PrimeTween;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCameraControl : MonoBehaviour
{
    [SerializeField] float climbSpeed = 1f;
    [SerializeField] float wasdCameraMultiplier = 1f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] float scrollZoneX;
    [SerializeField] float scrollZoneY;
    [SerializeField] float panDuration = 2;
    [SerializeField] float zoomSpeed;
    [SerializeField] int focalPointDeadzone = 5;
    [SerializeField] int cameraConfineMargin = 1;
    [SerializeField] CameraSet cams;
    [SerializeField] CinemachineConfiner[] confiners;

    Vector3Int mapCorner;
    static CameraSet Cams;
    string xInput;
    string yInput;

    [System.Serializable]
    class CameraSet
    {
        public CinemachineFreeLook Free;
        public CinemachineClearShot Automatic;
        public CinemachineBrain Brain;
        public Transform FreeFocalPoint;
        public Transform AutoFocalPoint;
    }
    public static MainCameraControl Instance;
    public static bool CameraAnimating {  get { return tracking || panning; } }
    static bool tracking;
    static bool panning;
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
        Instance = this;
        xInput = Cams.Free.m_XAxis.m_InputAxisName; //Mouse X
        yInput = Cams.Free.m_YAxis.m_InputAxisName; //Mouse Y
        LockCameraPivot();
    }
    void ConfineCameras(Vector3Int corner)
    {
        GameObject boundingBox = new()
        {
            layer = LayerMask.NameToLayer("Ignore Raycast")
        };
        BoxCollider boundingCollider = boundingBox.AddComponent<BoxCollider>();
        boundingCollider.center = corner / 2;
        corner.x -= cameraConfineMargin;
        corner.y -= cameraConfineMargin;
        corner.z -= cameraConfineMargin;

        boundingCollider.size = corner;
        
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
        if (PrimaryCursor.LockoutPlayer || CameraAnimating) return;

        Cams.Free.m_XAxis.m_InputAxisName = xInput;
        Cams.Free.m_YAxis.m_InputAxisName = yInput;
        PrimaryCursor.RestrictCursor(true);
    }

    void EndFocusRotation(InputAction.CallbackContext context)
    {
        LockCameraPivot();
        PrimaryCursor.RestrictCursor(false);
    }

    private void LateUpdate()
    {
        //if(Input.GetKeyDown(KeyCode.Backspace)) RestrictCamera(FreeCameraAvailable);
        if(!PrimaryCursor.LockoutPlayer && !CameraAnimating) PlayerControlCamera();
        cams.Brain.ManualUpdate();
        //Cams.Automatic.m_MinDuration = 50;
    }

    private void PlayerControlCamera()
    {
        Vector3 mouse = Input.mousePosition;

        if (Input.GetKey(KeyCode.W))
        {
            Zoom(zoomSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Zoom(-zoomSpeed);
        }
        if (Input.GetKey(KeyCode.A))
        {
            SlideCamera(new Vector3(-scrollZoneX * wasdCameraMultiplier, 0f, 0f));
        }
        else if (Input.GetKey(KeyCode.D))
        {
            SlideCamera(new Vector3(scrollZoneX * wasdCameraMultiplier, 0f, 0f));
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            ClimbCamera(climbSpeed);
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            ClimbCamera(-climbSpeed);
        }
        
        if(!rotateHold.inProgress)
        {
            PanCamera(mouse);
        }
    }

    void ClimbCamera(float value)
    {
        Vector3 climbedPosition = Cams.FreeFocalPoint.position;
        climbedPosition.y += climbSpeed * value;

        Cams.FreeFocalPoint.position = climbedPosition;
        ClampFocusInMap();
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
        Quaternion yRotation = Camera.main.transform.rotation;
        moveOffset *= scrollSpeed;
        Vector3 slidPosition = Cams.FreeFocalPoint.position + yRotation * moveOffset;
        Cams.FreeFocalPoint.position = slidPosition;
        ClampFocusInMap();
    }

    private void Zoom(float factor)
    {
        Vector3 direction = Cams.FreeFocalPoint.position - Camera.main.transform.position;
        direction.y = 0;
        direction.Normalize();
        direction *= factor;
        Cams.FreeFocalPoint.position += direction;
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
        Cams.FreeFocalPoint.position = Cams.FreeFocalPoint.position.Clamp(minimum, maximum);
    }

    public static void FindViewOfPosition(Vector3 target, Action callback = null)
    {
        Cams.AutoFocalPoint.transform.position = target;
        ToggleAutoCam();
        Instance.StartCoroutine(Instance.PanLockout(callback));
        panning = true;
    }

    public static void PanToPosition(Vector3 target)
    {
        panning = true;
        Tween.Position(Cams.FreeFocalPoint, endValue: target, duration: Instance.panDuration).OnComplete(() => panning = false);
    }

    IEnumerator PanLockout(Action callback)
    {
        yield return new WaitForSeconds(Cams.Brain.m_DefaultBlend.BlendTime);
        ToggleAutoCam(false);
        panning = false;
        callback?.Invoke();
    }

    static void ToggleAutoCam(bool on = true)
    {
        if (!on) Cams.FreeFocalPoint.position = Cams.AutoFocalPoint.position;
        //Cams.Automatic.gameObject.SetActive(on);
        Cams.Free.Priority = on ? 0 : 3;
        
    }

    
    public static void TrackTarget(Transform target)
    {
        tracking = true;
        ToggleAutoCam(true);
        Instance.StartCoroutine(TrackTowardsEntity(target));
    }

    public static void ReleaseTracking()
    {
        tracking = false;
        ToggleAutoCam(false);
    }

    static IEnumerator TrackTowardsEntity(Transform target)
    {
        Transform tracker = Cams.AutoFocalPoint;
        while (tracking)
        {
            //Vector3 targetPosition = Vector3.Lerp(tracker.position, target.position, Time.deltaTime);
            tracker.position = target.position;
            yield return null;
        }
    }

    
}
