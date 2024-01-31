using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListHelper
{
    public static void TransferItemTo<T>(this  List<T> list1, List<T> list2, T item)
    {
        list1.Remove(item);
        list2.Add(item);
    }

    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static T GrabRandomly<T>(this List<T> list)
    {
        if (list.Count == 0) Debug.LogError($"{list} is empty.");
        int index = Random.Range(0, list.Count);
        T output = list[index];
        list.RemoveAt(index);
        return output;
    }
}
