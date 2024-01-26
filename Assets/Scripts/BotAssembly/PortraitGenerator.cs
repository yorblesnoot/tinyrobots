using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PortraitGenerator : MonoBehaviour
{
    [SerializeField] Image portrait;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] Camera headCam;
    public void AttachPortrait(TinyBot bot)
    {
        headCam.transform.SetPositionAndRotation(bot.headshotPosition.position, bot.headshotPosition.rotation);
        headCam.Render();
        Texture2D tex = new(512, 512, TextureFormat.RGBA32, 1, false); // need to specify only 1 mipmap level
        Graphics.CopyTexture(renderTexture, tex);
        bot.portrait = Sprite.Create(tex, new Rect(0, 0, 512, 512), new Vector2(256, 256));
    } 
}
