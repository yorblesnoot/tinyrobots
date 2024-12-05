using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CinemachineReverseCollider : CinemachineExtension
{
    [SerializeField] CinemachineCollider mainCollider;
    [SerializeField] float qualityMultiplier = .1f;
    [SerializeField] float castWidth = 1;
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Aim && CheckReverseObstructions(state))
        {
            state.ShotQuality *= qualityMultiplier;
        }
    }

    bool CheckReverseObstructions(CameraState state)
    {
        if (state.HasLookAt)
        {
            Vector3 lookAtPos = state.ReferenceLookAt;
            Vector3 pos = state.CorrectedPosition;
            Vector3 dir = pos - lookAtPos;
            float distance = dir.magnitude;
            Ray ray = new Ray(lookAtPos, dir.normalized);
            if (CastCheck(ray, distance))
                return true;
        }
        return false;
    }

    bool CastCheck(Ray ray, float distance)
    {
        int layerMask = mainCollider.m_CollideAgainst & ~mainCollider.m_TransparentLayers;
        if (castWidth == 0) 
            return RuntimeUtility.RaycastIgnoreTag(ray, out _, distance - mainCollider.m_MinimumDistanceFromTarget, layerMask, mainCollider.m_IgnoreTag);
        else return Physics.SphereCast(ray, castWidth, distance, layerMask);
    }
}
