using Cinemachine;

public class CinemachineSecondaryConfiner : CinemachineExtension
{
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Aim && CameraIsOutOfBounds(vcam))
        {
            state.ShotQuality = 0;
        }
    }

    bool CameraIsOutOfBounds(CinemachineVirtualCameraBase vcam)
    {
        if(MainCameraControl.CameraBounds.ClosestPoint(vcam.transform.position) != vcam.transform.position) return true;
        return false;
    }
}
