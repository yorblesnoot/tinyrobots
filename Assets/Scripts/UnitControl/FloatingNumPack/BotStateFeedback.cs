using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BotStateFeedback : MonoBehaviour
{

    [SerializeField] GameObject floatNumber;

    public static ObjectPool numberPool;

    private void Awake()
    {
        numberPool = new(floatNumber);
    }

    readonly AvailableDisplacements displacements = new();
    static readonly Vector3 displacementFactor = new(.5f, 0, 0);
    static readonly float riseFactor = .2f;
    void PopupFloatingNumber(int number, Color color, int displacement)
    {
        Vector3 popPosition = transform.position;
        popPosition += transform.rotation * displacementFactor * (displacement % 2);
        popPosition.y += Mathf.Abs(displacement) * riseFactor;
        TMP_Text floatText = numberPool.InstantiateFromPool(popPosition, Quaternion.identity).GetComponentInChildren<TMP_Text>();
        floatText.text = number.ToString();
        floatText.color = color;
    }

    class Popup
    {
        public int number;
        public Color color;
    }

    Queue<Popup> popupQueue;

    public void QueuePopup(int number, Color color)
    {

        if (popupQueue == null || popupQueue.Count == 0)
        {
            popupQueue = new();
            StartCoroutine(SequencePopups());
        }
        popupQueue.Enqueue(new Popup {  number = number, color = color });
    }

    static readonly float popDelay = .2f;
    private IEnumerator SequencePopups()
    {
        yield return null;
        while(popupQueue.Count > 0)
        {
            yield return new WaitForSeconds(popDelay);
            Popup newpop = popupQueue.Dequeue();
            int displace = displacements.CheckoutDisplacement();
            StartCoroutine(displacements.CountdownToCheckIn(FloatingNumber.lifespan, displace));
            PopupFloatingNumber(newpop.number, newpop.color, displace);
        }
    }
}

class AvailableDisplacements
{
    Dictionary<int, bool> displacements = new();
    public AvailableDisplacements()
    {
        displacements.Add(0, true);
    }
    public int CheckoutDisplacement()
    {
        int checkout = 0;
        if(displacements.Where(x => x.Value == true).Count() == 0)
        {
            int max = displacements.Select(x => x.Key).Max();
            max++;
            displacements.Add(max, true);
            displacements.Add(-max, true);
        }
        checkout = displacements.First(x => x.Value == true).Key;
        displacements[checkout] = false;
        return checkout;
    }

    public IEnumerator CountdownToCheckIn(float timer, int num)
    {
        yield return new WaitForSeconds(timer);
        displacements[num] = true;
    }
}
