using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UUIFX
{
    public class NextExampleGRP : MonoBehaviour
    {
        public GameObject Grp0, Grp1, Grp2;

        public void ActiveNext()
        {
            if (Grp0.activeSelf)
            {
                Grp0.SetActive(false);
                Grp1.SetActive(true);
                Grp2.SetActive(false);
            }
            else if (Grp1.activeSelf)
            {
                Grp0.SetActive(false);
                Grp1.SetActive(false);
                Grp2.SetActive(true);
            }
            else
            {
                Grp0.SetActive(true);
                Grp1.SetActive(false);
                Grp2.SetActive(false);
            }
        }
    }
}