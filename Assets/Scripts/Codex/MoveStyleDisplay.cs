using UnityEngine;
using UnityEngine.UI;

public class MoveStyleDisplay : MonoDictionary<MoveStyle, Sprite>
{
    [SerializeField] Image icon;
    [SerializeField] MoveStyle defaultStyle;

    private void Awake()
    {
        icon.sprite = this[defaultStyle];
    }

    public void Become(ModdedPart part)
    {
        bool movePart = part.BasePart.PrimaryLocomotion;
        gameObject.SetActive(movePart);
        if (!movePart) return;
        PrimaryMovement movement = part.Sample.GetComponent<PrimaryMovement>();
        icon.sprite = this[movement.Style];
    }
}
