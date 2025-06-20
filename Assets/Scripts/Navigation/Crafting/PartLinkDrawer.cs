using PrimeTween;
using UnityEngine;

public class PartLinkDrawer : MonoBehaviour
{
    [SerializeField] private float lineTravelTime = 0.5f;
    
    [SerializeField] LineRenderer parentLine;

    [SerializeField] Color slottedColor = Color.green;
    Color baseColor;
    PartSlot thisSlot;
    private void Awake()
    {
        thisSlot = GetComponent<PartSlot>();
        baseColor = parentLine.startColor;
        PartSlot.ModifiedParts.AddListener(ToggleLinkHighlight);
    }

    private void ToggleLinkHighlight()
    {
        if(thisSlot.PartIdentity != null)
        {
            parentLine.endColor = slottedColor;
        }
        else
        {
            parentLine.endColor = baseColor;
        }
    }

    public void LinkToSlot(PartSlot target)
    {
        LineRenderer parentLine = target.GetComponent<PartLinkDrawer>().parentLine;
        parentLine.positionCount = 2;
        Vector3 start = thisSlot.ApproachCamera(transform.position);
        Vector3 end = thisSlot.ApproachCamera(target.transform.position);
        Tween.Custom(0, 1, lineTravelTime, (progress) => TweenLine(progress, parentLine, start, end));
    }

    void TweenLine(float progress, LineRenderer slotLine, Vector3 start, Vector3 end)
    {
        slotLine.SetPosition(0, start);
        slotLine.SetPosition(1, Vector3.Lerp(start, end, progress));
    }
}
