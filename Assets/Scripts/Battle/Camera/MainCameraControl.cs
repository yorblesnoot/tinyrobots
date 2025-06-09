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
    [SerializeField] bool confineCameras = false;
    [SerializeField] int cameraConfineMargin = 1;
    [SerializeField] CameraSet cams;
    [SerializeField] CinemachineConfiner[] confiners;

    Vector3Int mapCorner;
    static CameraSet Cams => Instance.cams;
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
    public static BoxCollider CameraBounds;
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
        if(confineCameras) ConfineCameras(mapCorner);
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
        CameraBounds = boundingBox.AddComponent<BoxCollider>();
        CameraBounds.center = corner / 2;
        corner.x -= cameraConfineMargin;
        corner.y -= cameraConfineMargin;
        corner.z -= cameraConfineMargin;

        CameraBounds.size = corner;
        
        foreach(var cam in confiners)
        {
            cam.m_BoundingVolume = CameraBounds;
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
        if (Input.GetKeyDown(KeyCode.Backspace)) PrimaryCursor.TogglePlayerLockout(!PrimaryCursor.LockoutPlayer);
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

        PlayerMoveFreeCam(climbedPosition);
    }

    void PlayerMoveFreeCam(Vector3 position)
    {
        Vector3 minimum = Vector3.one * focalPointDeadzone;
        Vector3 maximum = mapCorner - minimum;
        Vector3 newPosition = position.Clamp(minimum, maximum);
        Cams.FreeFocalPoint.position = newPosition;
        Cams.AutoFocalPoint.position = newPosition;
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
        PlayerMoveFreeCam(slidPosition);
    }

    private void Zoom(float factor)
    {
        Vector3 direction = Cams.FreeFocalPoint.position - Camera.main.transform.position;
        direction.y = 0;
        direction.Normalize();
        direction *= factor;
        PlayerMoveFreeCam(Cams.FreeFocalPoint.position + direction);
    }

    static void LockCameraPivot()
    {
        Cams.Free.m_YAxis.m_InputAxisName = "";
        Cams.Free.m_XAxis.m_InputAxisName = "";
        Cams.Free.m_YAxis.m_InputAxisValue = 0f;
        Cams.Free.m_XAxis.m_InputAxisValue = 0f;
    }

    public static void FindViewOfPosition(Vector3 target, Action callback = null)
    {
        Instance.StartCoroutine(Instance.FindView(target, callback));   
    }

    
    IEnumerator FindView(Vector3 target, Action callback)
    {
        Cams.AutoFocalPoint.transform.position = target;
        ToggleAutoCam();
        panning = true;
        yield return new WaitForSeconds(Cams.Brain.m_DefaultBlend.BlendTime);
        panning = false;
        ToggleAutoCam(false);
        callback?.Invoke();
    }

    public static void PanToPosition(Vector3 target)
    {
        panning = true;
        Tween.Position(Cams.FreeFocalPoint, endValue: target, duration: Instance.panDuration).OnComplete(() => panning = false);
    }


    static void ToggleAutoCam(bool on = true)
    {
        if (!on)
        {
            Cams.FreeFocalPoint.position = Cams.AutoFocalPoint.position;
            Cams.Brain.ManualUpdate();
        }
        //Cams.Automatic.gameObject.SetActive(on);
        Cams.Free.Priority = on ? 0 : 3;
        
    }

    
    public static void TrackTarget(Transform target)
    {
        tracking = true;
        Instance.StartCoroutine(TrackTowardsEntity(target));
    }

    public static void ReleaseTracking()
    {
        tracking = false;
    }

    static IEnumerator TrackTowardsEntity(Transform target)
    {
        ToggleAutoCam(true);
        Transform tracker = Cams.AutoFocalPoint;
        while (tracking)
        {
            Vector3 targetPosition = Vector3.Lerp(tracker.position, target.position, Time.deltaTime);
            tracker.position = targetPosition;
            Cams.FreeFocalPoint.position = targetPosition;
            yield return null;
        }
        ToggleAutoCam(false);
    }

    
}
