using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Transform focusPoint;
    [SerializeField] float rotationSpeed = .1f;
    [SerializeField] float scrollSpeed = 1f;
    [SerializeField] float scrollZoneX;
    [SerializeField] float scrollZoneY;
    [SerializeField] float zoomSpeed;
    [SerializeField] int maxZoom;
    [SerializeField] int minZoom;

    Quaternion startRotation;
    Vector3 initialClick;
    private void Update()
    {
        Vector3 mouse = Input.mousePosition;
        if (Input.GetMouseButtonDown(2))
        {
            startRotation = focusPoint.rotation;
            initialClick = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            //Vector3 rotationCenter = new(Screen.width/2, Screen.height/2);
            Vector3 centerOffset = mouse - initialClick;
            Vector3 weightedDirection = (centerOffset * rotationSpeed);
            Vector3 finalEulerRotation = startRotation.eulerAngles;
            finalEulerRotation.x -= weightedDirection.y;
            finalEulerRotation.y += weightedDirection.x;
            focusPoint.rotation = Quaternion.Euler(finalEulerRotation);
        }
        else if(Input.GetKey(KeyCode.W))
        {
            Zoom(zoomSpeed);
        }
        else if (Input.GetKey(KeyCode.S))
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

    private void SlideCamera(Vector3 moveOffset)
    {
        Vector3 eulerRotation = transform.rotation.eulerAngles;
        eulerRotation.x = 0;
        eulerRotation.z = 0;
        Quaternion yRotation = Quaternion.Euler(eulerRotation);

        moveOffset *= scrollSpeed;
        transform.position += yRotation * moveOffset;
    }

    private void Zoom(float factor)
    {
        Vector3 cameraPosition = Camera.main.transform.localPosition;
        cameraPosition.z = Mathf.Clamp(cameraPosition.z + factor, minZoom, maxZoom);
        Camera.main.transform.localPosition = cameraPosition;
    }
}
