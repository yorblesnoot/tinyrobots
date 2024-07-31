using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevBattle : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            List<TinyBot> targets = new(TurnManager.TurnTakers);
            foreach(var bot in targets)
            {
                if(bot.Allegiance == Allegiance.ENEMY) bot.Die();
            }
        }
    }
}
