using UnityEngine;

public class TurretTracker : MonoBehaviour
{
    [SerializeField] Transform aimTarget;
    [SerializeField] float slowness;

    Vector3 basePosition;
    GameObject trackedTarget;
    void Awake()
    {
        basePosition = aimTarget.localPosition;
    }

    public void TrackObject(GameObject go = null)
    {
        trackedTarget = go;
    }

    void Update()
    {
        TrackTarget();
    }

    private void TrackTarget()
    {
        if(trackedTarget == null)
        {
            aimTarget.localPosition = basePosition;
            return;
        }
        Vector3 localizedTarget = trackedTarget.transform.position;
        localizedTarget = transform.InverseTransformPoint(localizedTarget);
        aimTarget.localPosition = Vector3.Lerp(aimTarget.localPosition, localizedTarget, 1/slowness);
    }
}
