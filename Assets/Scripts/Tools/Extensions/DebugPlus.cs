using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DebugPlus 
{
    public static void DebugContents(this IEnumerable objects)
    {
        string output = objects.ToString() + ": ";
        foreach (var obj in objects)
        {
            output += obj.ToString();
            output += ", ";
        }
        Debug.Log(output);
    }
}
