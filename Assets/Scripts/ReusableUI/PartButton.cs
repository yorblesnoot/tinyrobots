using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public abstract class PartButton : MonoBehaviour
{
    [HideInInspector] public CanvasGroup Group;
    [SerializeField] protected Button selectButton;
    public ModdedPart PartIdentity { get; protected set; }
    public abstract void DisplayPart(ModdedPart part, UnityAction<ModdedPart> activationCallback, int value, UnityAction<ModdedPart> secondaryActivation = null);
}
