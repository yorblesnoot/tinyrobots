using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartySelectionPortrait : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;

    UnityAction onRightClick;
    Sprite defaultSprite;

    private void Awake()
    {
        defaultSprite = portrait.sprite;
    }

    public void Become(BotCharacter character, UnityAction onClick, UnityAction rightClick = null)
    {
        portrait.sprite = character.CharacterPortrait;
        button.onClick.AddListener(onClick);
        onRightClick = rightClick ?? onClick;
    }

    public void Clear()
    {
        portrait.sprite = defaultSprite;
        onRightClick = null;
        button.onClick.RemoveAllListeners();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right) onRightClick?.Invoke();
    }
}
