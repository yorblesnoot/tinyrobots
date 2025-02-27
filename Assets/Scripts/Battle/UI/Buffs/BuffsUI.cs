using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffsUI : MonoBehaviour
{
    [SerializeField] BuffIcon[] icons;
    TinyBot activeTarget;
    private void Awake()
    {
        PrimaryCursor.PlayerSelectedBot.AddListener(ShowBuffs);
    }

    void ShowBuffs(TinyBot bot)
    {
        
        if(activeTarget != null) activeTarget.Buffs.BuffsChanged.RemoveListener(ShowBuffs);
        activeTarget = bot;
        bot.Buffs.BuffsChanged.AddListener(ShowBuffs);
        List<AppliedBuff> showableBuffs = bot.Buffs.ActiveBuffs.Values.Where(applied => applied.Buff.Description != "").ToList();
        showableBuffs.PassDataToUI(icons, (applied, icon) => icon.Become(applied));
    }
}
