using UnityEngine;

public class TurretTracker : MonoBehaviour
{
    [SerializeField] Transform aimTarget;
    [SerializeField] float slowness;

    Vector3 basePosition;
    void Awake()
    {
        basePosition = aimTarget.localPosition;
    }

    public void TrackTarget(GameObject target)
    {
        if(target == null)
        {
            aimTarget.localPosition = basePosition;
            return;
        }
        Vector3 localizedTarget = target.transform.position;
        localizedTarget = transform.InverseTransformPoint(localizedTarget);
        aimTarget.localPosition = Vector3.Lerp(aimTarget.localPosition, localizedTarget, 1/slowness);
    }
}
