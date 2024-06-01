using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 namespace UUIFX
{
     public class CtrlExamples_Fadeout : MonoBehaviour
    {
        public float speed = 0.1f;
        bool isAdd = true;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float f = GetComponent<Image>().material.GetFloat("_Position"); 
            if (f > 3)
            {
                isAdd = false;
            }
            else if (f < -2)
            {

                isAdd = true;
            }
            print(f);
            f =  isAdd? f+ speed * Time.deltaTime : f - speed * Time.deltaTime;
            GetComponent<Image>().material.SetFloat("_Position", f);
           
        }

    }

}