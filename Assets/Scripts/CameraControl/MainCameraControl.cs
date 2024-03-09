using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCameraControl : MonoBehaviour
{
    [SerializeField] float rotationSpeed = .1f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] float scrollZoneX;
    [SerializeField] float scrollZoneY;
    [SerializeField] float zoomSpeed;
    [SerializeField] int maxZoom;
    [SerializeField] int minZoom;
    [SerializeField] float snapSpeed;

    [SerializeField] float maxTiltAngle;

    Quaternion startRotation;
    Vector3 initialClick;

    Vector3 mapCorner;

    [SerializeField] CinemachineBrain brain;


    [System.Serializable]
    class CameraSet
    {
        public CinemachineVirtualCamera Free;
        public CinemachineClearShot  Select;
        public CinemachineVirtualCamera Action;
        public CinemachineTargetGroup Group;
    }
    [SerializeField] CameraSet cams;
    static CameraSet Cams;
    static Transform focalPoint;

    CinemachineInputProvider orbitalInput;
    public void Initialize(byte[,,] map)
    {
        mapCorner = new(map.GetLength(0), map.GetLength(1), map.GetLength(2));
        Cams = cams;
        focalPoint = transform;
    }

    BaseInput playerInput;
    InputAction rotator;
    InputAction rotateHold;
    InputAction rotateOn;

    private void OnEnable()
    {
        playerInput = new();
        rotator = playerInput.Main.RotatePosition;
        rotator.Enable();

        rotateHold = playerInput.Main.Rotating;
        rotateHold.Enable();
        rotateHold.canceled += (InputAction.CallbackContext context) => PrimaryCursor.State = CursorState.FREE;

        rotateOn = playerInput.Main.RotateOn;
        rotateOn.Enable();
        rotateOn.performed += BeginFocusRotation;
    }

    void BeginFocusRotation(InputAction.CallbackContext context)
    {
        startRotation = transform.rotation;
        initialClick = Input.mousePosition;
        PrimaryCursor.State = CursorState.SPACELOCKED;
        ChangeToFreeCam();
    }

    void ChangeToFreeCam()
    {
        var activeCam = brain.ActiveVirtualCamera;
        var transposer = Cams.Free.GetCinemachineComponent<CinemachineTransposer>();
        Vector3 activePosition = activeCam.VirtualCameraGameObject.transform.position;
        activePosition = focalPoint.transform.InverseTransformPoint(activePosition);
        transposer.m_FollowOffset = activePosition;
        Cams.Free.Priority = 100;
    }

    private void Update()
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

        else if (rotateHold.inProgress)
        {
            //Vector3 rotationCenter = new(Screen.width/2, Screen.height/2);
            Vector3 centerOffset = mouse - initialClick;
            Vector3 weightedDirection = (centerOffset * rotationSpeed);
            Vector3 finalEulerRotation = startRotation.eulerAngles;
            bool invertedAngle = finalEulerRotation.x <= 1f;
            finalEulerRotation.x -= weightedDirection.y;
            finalEulerRotation.y += weightedDirection.x;
            
            if (invertedAngle) finalEulerRotation.x = Mathf.Clamp(finalEulerRotation.x, -maxTiltAngle, 0);
            else finalEulerRotation.x = Mathf.Clamp(finalEulerRotation.x, 360-maxTiltAngle, 360);
            transform.rotation = Quaternion.Euler(finalEulerRotation);
        }
        else
        {
            float leftScroll = Mathf.Clamp(scrollZoneX - mouse.x, 0, float.MaxValue);
            float rightScroll = Mathf.Clamp(mouse.x - (Screen.width - scrollZoneX), 0, float.MaxValue);
            float downScroll = Mathf.Clamp(scrollZoneY - mouse.y, 0, float.MaxValue);
            float upScroll = Mathf.Clamp(mouse.y - (Screen.height - scrollZoneY), 0, float.MaxValue);
            Vector3 moveOffset = new(rightScroll - leftScroll, upScroll - downScroll, 0f);

            SlideCamera(moveOffset);
        }
        brain.ManualUpdate();
    }

    [SerializeField] float originReturnTime;

    private void SlideCamera(Vector3 moveOffset)
    {
        return;
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        eulerRotation.x = 0;
        eulerRotation.z = 0;
        Quaternion yRotation = Quaternion.Euler(eulerRotation);

        moveOffset *= scrollSpeed;
        Vector3 slidPosition = transform.position + yRotation * moveOffset;
        //clamp focus point within map bounds
        slidPosition = slidPosition.Clamp(Vector3.zero, mapCorner);
        transform.position = slidPosition;
    }

    private void Zoom(float factor)
    {
        Vector3 cameraPosition = Camera.main.transform.localPosition;
        cameraPosition.z = Mathf.Clamp(cameraPosition.z + factor, minZoom, maxZoom);
        Camera.main.transform.localPosition = cameraPosition;
    }

    public static void CutToUnit(TinyBot bot)
    {
        Cams.Free.Priority = 0;
        focalPoint.transform.position = bot.transform.position;
    }
}
