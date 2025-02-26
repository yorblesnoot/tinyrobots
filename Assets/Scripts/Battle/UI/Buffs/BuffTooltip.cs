using TMPro;
using UnityEngine;

public class BuffTooltip : Tooltip
{
    [SerializeField] TMP_Text buffName;
    [SerializeField] TMP_Text buffDescription;
    [SerializeField] TMP_Text buffEffect;
    [SerializeField] TMP_Text buffDuration;
    static BuffTooltip instance;

    private void Awake()
    {
        instance = this;
    }
    public static void Become(AppliedBuff applied, Vector3 position)
    {
        instance.BecomeInternal(applied, position);
    }

    void BecomeInternal(AppliedBuff applied, Vector3 position)
    {
        gameObject.SetActive(true);
        buffName.text = applied.Buff.DisplayName;
        buffDescription.text = applied.Buff.Description;
        buffEffect.text = applied.Potency + applied.Buff.LineDescription;
        buffDuration.text = applied.RemainingDuration + " Turn" + (applied.RemainingDuration != 1 ? "s" : "");
        instance.SetPosition(position);
    }

    public static void Hide()
    {
        instance.gameObject.SetActive(false);
    }
}
