using UnityEngine.Events;

public class Observable<T> where T : struct
{
    public UnityEvent OnChange = new();
    T localValue;
    public T Value { get { return localValue; }
        set { localValue = value; OnChange?.Invoke(); }
    }
}
