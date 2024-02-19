using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    [SerializeField] Transform projector;

    [SerializeField] float maxTiltAngle;

    Quaternion startRotation;
    Vector3 initialClick;

    Vector3 mapCorner;

    static Transform focalPoint;
    public void Initialize(byte[,,] map)
    {
        mapCorner = new(map.GetLength(0), map.GetLength(1), map.GetLength(2));
        focalPoint = transform;
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

        if (Input.GetMouseButtonDown(2))
        {
            startRotation = transform.rotation;
            initialClick = Input.mousePosition;
            PrimaryCursor.State = CursorState.SPACELOCKED;
            StartCoroutine(gameObject.LerpTo(PrimaryCursor.Transform.position, originReturnTime));
        }
        else if(Input.GetMouseButtonUp(2)) PrimaryCursor.State = CursorState.FREE;
        else if (Input.GetMouseButton(2))
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
        
    }

    [SerializeField] float originReturnTime;

    private void SlideCamera(Vector3 moveOffset)
    {
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
    static Vector3Int[] directions = {
            new Vector3Int(-1, -1, 0), new Vector3Int(1, -1, 0),  // Edges along xy-plane
            new Vector3Int(-1, 1, 0), new Vector3Int(1, 1, 0),    // Edges along xy-plane
            new Vector3Int(-1, 0, -1), new Vector3Int(1, 0, -1),  // Edges along xz-plane
            new Vector3Int(0, -1, -1), new Vector3Int(0, 1, -1),  // Edges along yz-plane
            new Vector3Int(-1, -1, -1), new Vector3Int(1, -1, -1),  // Corners
            new Vector3Int(-1, 1, -1), new Vector3Int(1, 1, -1),    // Corners
            new Vector3Int(-1, -1, 1), new Vector3Int(1, -1, 1),    // Corners
            new Vector3Int(-1, 1, 1), new Vector3Int(1, 1, 1)        // Corners
    };
    public static void CutToUnit(TinyBot bot)
    {
        focalPoint.position = bot.transform.position;
        Vector3 startDirection = Camera.main.transform.position - focalPoint.position;
        float camDistance = startDirection.magnitude;
        Ray ray = new(bot.transform.position, startDirection);
        if (!Physics.Raycast(ray, camDistance, LayerMask.GetMask("Terrain"))) return;

        List<Vector3> openDirections = new();
        foreach(var direction in directions)
        {
            Ray secondRay = new(bot.transform.position, direction);
            if (!Physics.Raycast(secondRay, camDistance, LayerMask.GetMask("Terrain"))) openDirections.Add(direction);
        }
        Vector3 finalDirection = openDirections.OrderByDescending(x => Vector3.Dot(x, startDirection)).FirstOrDefault();
        Quaternion finalRotation = Quaternion.LookRotation(finalDirection);
        focalPoint.rotation = finalRotation;
    }
}
