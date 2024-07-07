using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UUIFX
{
    public class CircularFilledWithTime : MonoBehaviour
    {
        public float totalTime = 5.0f; // 总时间
        public float startAngle = 90.0f; // 开始角度
        private float elapsedTime = 0.0f;
        public bool clockwise = true; // 是否顺时针
        public bool loop = false;


        // Start is called before the first frame update
        void Start()
        {

            GetComponent<Image>().material.SetFloat("_StartAngle", startAngle);
        }

        public void Init(float time)
        {
            elapsedTime = 0;
            totalTime = time;
        }

        // Update is called once per frame
        void Update()
        {
            if (elapsedTime > totalTime && loop)
            {
                Init(totalTime);
            }
            if (elapsedTime < totalTime)
            {
                // 累加已经过去的时间
                elapsedTime += Time.deltaTime;

                // 计算当前的结束角度
                float endAngle = startAngle + (clockwise ? 360.0f : -360.0f) * (elapsedTime / totalTime);

                // 使结束角度保持在 0 到 360 之间
                endAngle = endAngle % 360.0f;
                if (endAngle < 0) endAngle += 360;
                // 将结束角度传递给 Shader
                GetComponent<Image>().material.SetFloat("_EndAngle", endAngle);
            }
            else
            {

                GetComponent<Image>().material.SetFloat("_EndAngle", startAngle);
            }
        }

    }
}