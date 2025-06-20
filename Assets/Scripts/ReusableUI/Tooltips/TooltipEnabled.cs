using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipEnabled : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField][TextArea(3, 10)] protected string TooltipText;
    [SerializeField] Color textColor = Color.white;
    public void OnPointerEnter(PointerEventData eventData)
    {
        TextTooltip.Show(TooltipText, transform.position, textColor);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TextTooltip.Hide();
    }
}
