using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image icon;
    AppliedBuff activeBuff;
    public void Become(AppliedBuff applied)
    {
        activeBuff = applied;
        icon.sprite = applied.Buff.Thumbnail;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BuffTooltip.Become(activeBuff, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BuffTooltip.Hide();
    }
}
