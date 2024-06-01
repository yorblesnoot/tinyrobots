using UnityEngine;
using UnityEngine.UI;
namespace UUIFX
{
    [RequireComponent(typeof(Image))]
    public class FadeEffect : MonoBehaviour
    {
        public float speed = 1.0f;
        private Image image;
        private Material material;

        void Start()
        {
            image = GetComponent<Image>();
            material = image.material;
            material.SetFloat("_Speed", speed);
        }

        void Update()
        {
            float fade = Mathf.PingPong(Time.time * speed, 1);
            material.SetFloat("_Fade", fade);
        }
    }
}