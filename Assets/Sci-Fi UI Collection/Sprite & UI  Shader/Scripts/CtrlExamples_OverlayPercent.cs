using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 namespace UUIFX
{
     public class CtrlExamples_OverlayPercent : MonoBehaviour
    {
        public float speed = 2;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            float f =   GetComponent<Image>().material.GetFloat("_OverlayPercent");
           
            f += speed * Time.deltaTime;
            if (f > 1) f = 0f;
            GetComponent<Image>().material.SetFloat("_OverlayPercent", f);
        }

    }

}