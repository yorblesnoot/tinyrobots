using System.Linq;

public static class DynamicAbilityDescription 
{
    readonly static string terminatingCharacter = "/";
    public static string Describe(Ability ability)
    {
        string description = ability.Description;
        Substitute("sub", GetSubNames(ability));
        return description;

        void Substitute(string marker, object value)
        {
            marker = "*" + marker + terminatingCharacter;
            int markerIndex = description.IndexOf(marker);
            if (markerIndex == -1) return;

            int baseIndex = markerIndex + marker.Length;
            int endIndex = baseIndex;
            while(description[endIndex] != '/') endIndex++;
            if (value == null)
            {
                description = description.Remove(endIndex, 1);
                description = description.Remove(markerIndex, marker.Length);
            }
            else description = description.Replace(description[markerIndex..(endIndex+1)], value.ToString());
        }
    }
    

    static string GetSubNames(Ability ability)
    {
        if (ability.SubTrees == null || ability.SubTrees.Count == 0) return null;
        return ability.SubTrees[0].Value.Sample.Abilities.Select(a => a.name).ToList().ToOxfordList();
    }
}
