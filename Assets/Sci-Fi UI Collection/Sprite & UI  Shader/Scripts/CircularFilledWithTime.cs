using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UUIFX
{
    public class CircularFilledWithTime : MonoBehaviour
    {
        public float totalTime = 5.0f; // ��ʱ��
        public float startAngle = 90.0f; // ��ʼ�Ƕ�
        private float elapsedTime = 0.0f;
        public bool clockwise = true; // �Ƿ�˳ʱ��
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
                // �ۼ��Ѿ���ȥ��ʱ��
                elapsedTime += Time.deltaTime;

                // ���㵱ǰ�Ľ����Ƕ�
                float endAngle = startAngle + (clockwise ? 360.0f : -360.0f) * (elapsedTime / totalTime);

                // ʹ�����Ƕȱ����� 0 �� 360 ֮��
                endAngle = endAngle % 360.0f;
                if (endAngle < 0) endAngle += 360;
                // �������Ƕȴ��ݸ� Shader
                GetComponent<Image>().material.SetFloat("_EndAngle", endAngle);
            }
            else
            {

                GetComponent<Image>().material.SetFloat("_EndAngle", startAngle);
            }
        }

    }
}