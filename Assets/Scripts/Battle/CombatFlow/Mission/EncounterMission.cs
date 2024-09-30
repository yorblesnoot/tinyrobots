using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterMission : Mission
{
    public override bool MetEndCondition(TurnManager turnManager)
    {
        if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.PLAYER).Count() == 0)
        {
            BattleEnder.GameOver();
            return true;
        }
        else if (TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.ENEMY).Count() == 0)
        {
            BattleEnder.PlayerWin();
            return true;
        }
        return false;
    }
}
