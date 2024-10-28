using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PartySelectionPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] Button button;
    public void Become(BotCharacter character, UnityAction onClick)
    {
        portrait.sprite = character.CharacterPortrait;
        button.onClick.AddListener(onClick);
    }

    public void Clear()
    {
        portrait.sprite = null;
        button.onClick.RemoveAllListeners();
    }
}
