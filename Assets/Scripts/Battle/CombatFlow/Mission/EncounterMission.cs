using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EncounterMission : Mission
{
    public override bool MetVictoryCondition()
    {
        return TurnManager.TurnTakers.Where(bot => bot.Allegiance == Allegiance.ENEMY).Count() == 0;
    }
}
