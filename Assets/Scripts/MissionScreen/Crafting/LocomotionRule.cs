using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocomotionRule : MonoBehaviour
{
    [SerializeField] GameObject inactiveMarker;
    [SerializeField] Button doneButton;
    [SerializeField] TMP_Text tooltipText;
    private void Awake()
    {
        PartSlot.ModifiedParts.AddListener(UpdateRule);
    }

    private void UpdateRule()
    {
        List<string> errors = GetCraftErrors();
        tooltipText.text = string.Join("\n", errors);
        bool valid = errors.Count == 0;
        doneButton.interactable = valid;
        inactiveMarker.SetActive(!valid);
    }

    List<string> GetCraftErrors()
    {
        List<string> errors = new();
        foreach (var character in SceneGlobals.PlayerData.CoreInventory)
        {
            string noLocomotion = $"{character.CoreName} has no locomotion part slotted.";
            if (BotCrafter.ActiveCore == character)
            {
                if (!PartSlot.PrimaryLocomotionSlotted) errors.Add(noLocomotion);
            }
            else
            {
                if (!HasLocomotion(character)) errors.Add(noLocomotion);
            }
            //check for part recursion
        }
        return errors;
    }

    bool HasLocomotion(BotCharacter character)
    {
        foreach (var part in character.Bot.Flatten())
        {
            if (part.BasePart.PrimaryLocomotion) return true;
        }
        return false;
    }
}
