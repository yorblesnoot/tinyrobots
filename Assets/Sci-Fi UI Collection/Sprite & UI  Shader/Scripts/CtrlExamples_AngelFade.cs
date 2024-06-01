using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 namespace UUIFX
{
     public class CtrlExamples_AngelFade : MonoBehaviour
    {
        public bool clockwise = true; //  «∑ÒÀ≥ ±’Î
        public float speed = 120;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            float f =   GetComponent<Image>().material.GetFloat("_DisplayAngle");
            if (clockwise)
            {
                f -= speed * Time.deltaTime;
                if (f <= 0) f = 360;
            }
            else
            {
                f += speed * Time.deltaTime;
                if (f >= 360) f = 0;
            }
            GetComponent<Image>().material.SetFloat("_DisplayAngle", f);
        }

    }

}