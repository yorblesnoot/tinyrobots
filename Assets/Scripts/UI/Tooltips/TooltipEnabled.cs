using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipEnabled : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField][TextArea(3, 10)] protected string TooltipText;
    public void OnPointerEnter(PointerEventData eventData)
    {
        TextTooltip.Show(TooltipText, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TextTooltip.Hide();
    }
}
