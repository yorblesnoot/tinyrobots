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
        Vector3 localizedTarget = target.transform.position;
        localizedTarget = transform.InverseTransformPoint(localizedTarget);
        aimTarget.localPosition = Vector3.Lerp(aimTarget.localPosition, localizedTarget, 1/slowness);
    }

    public void ResetTracking()
    {
        StartCoroutine(aimTarget.gameObject.LerpTo(basePosition, .5f, true));
    }
}
