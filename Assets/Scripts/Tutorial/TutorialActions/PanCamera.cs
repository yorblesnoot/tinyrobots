using Cinemachine;
using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanCamera : TutorialAction
{
    [SerializeField] float moveDuration = 2;
    [SerializeField] CinemachineVirtualCamera cam;
    CinemachineTrackedDolly dolly;
    private void Awake()
    {
        dolly = cam.GetCinemachineComponent<CinemachineTrackedDolly>();
    }
    public override IEnumerator Execute()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        cam.Priority = 5;
        yield return Tween.Custom(dolly, 0, 1, moveDuration, OnChange).ToYieldInstruction();
        cam.Priority = 0;
        PrimaryCursor.TogglePlayerLockout(false);
    }

    void OnChange(CinemachineTrackedDolly dolly, float value)
    {
        dolly.m_PathPosition = value;
    }
}
