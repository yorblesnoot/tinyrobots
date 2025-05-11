using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Observable<T> where T : struct
{
    [HideInInspector] public UnityEvent OnChange = new();
    [SerializeField] T localValue;
    public T Value { get { return localValue; }
        set { localValue = value; OnChange?.Invoke(); }
    }
}
