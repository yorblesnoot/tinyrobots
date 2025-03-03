using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [TextArea(3, 10)] public string Description;
    public float range;
    public bool ModifiableRange = false;
    public int cooldown = 1;
    [HideInInspector] public float EffectivenessMultiplier = 1;
    public int cost;
    public Sprite icon;
    protected DurationModule DurationModule;

    [HideInInspector] public int CurrentCooldown;
    [HideInInspector] public TinyBot Owner;

    public Transform emissionPoint;
    public List<TreeNode<ModdedPart>> SubTrees;

    protected abstract AbilityEffect[] Effects { get; }

    public virtual void Initialize(TinyBot botUnit)
    {
        Owner = botUnit;
        Owner.BeganTurn.AddListener(LapseCooldown);
    }

    public bool IsScalable()
    {
        foreach (var effect in Effects)
        {
            if (effect.BaseEffectMagnitude > 0) return true;
        }
        return false;
    }

    public abstract bool IsActive { get; }

    void LapseCooldown()
    {
        CurrentCooldown = Mathf.Clamp(CurrentCooldown - 1, 0, CurrentCooldown);
    }

    public string GetEffectPhrases()
    {
        var phrases = new List<string>();
        foreach(var effect in Effects)
        {
            if(effect.BaseEffectMagnitude == 0) continue;
            string phrase = effect.FinalEffectiveness.ToString() + effect.Description;
            phrases.Add(phrase);
        }
        return phrases.Count > 0 ? phrases.ToOxfordList() : "---";
    }
}
