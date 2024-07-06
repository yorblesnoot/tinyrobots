using UnityEngine;

public class PortraitGenerator : MonoBehaviour
{
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] Camera headCam;

    Vector3 photoPosition;
    private void Awake()
    {
        photoPosition = new(-500, -500, -500);
    }
    public void AttachPortrait(TinyBot bot)
    {
        Vector3 botPosition = bot.transform.position;
        bot.transform.position = photoPosition;
        headCam.transform.SetPositionAndRotation(bot.headshotPosition.position, bot.headshotPosition.rotation);
        headCam.Render();
        Texture2D tex = new(512, 512, TextureFormat.RGBA32, 1, false); // need to specify only 1 mipmap level
        Graphics.CopyTexture(renderTexture, tex);
        bot.Portrait = Sprite.Create(tex, new Rect(0, 0, 512, 512), new Vector2(256, 256));
        bot.transform.position = botPosition;
    } 
}
