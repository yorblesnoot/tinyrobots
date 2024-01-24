using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnPortrait : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] RenderTexture renderTexture;

    public void Become(TinyBot bot, Camera headCam)
    {
        gameObject.SetActive(true);
        headCam.transform.SetPositionAndRotation(bot.headshotPosition.position, bot.headshotPosition.rotation);
        headCam.Render();
        Texture2D tex = new(512, 512, TextureFormat.RGBA32, 1, false); // need to specify only 1 mipmap level
        Graphics.CopyTexture(renderTexture, tex);
        portrait.sprite = Sprite.Create(tex, new Rect(0, 0, 512, 512), new Vector2(256, 256));

    }
}
