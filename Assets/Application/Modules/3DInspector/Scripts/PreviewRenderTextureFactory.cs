using UnityEngine;
using UnityEngine.UI;

static class PreviewRenderTextureFactory
{
    public static RenderTexture CreateAndBind(Camera previewCamera, RawImage targetImage, int width, int height)
    {
        var rendTexture = Create(width, height);
        if (rendTexture == null) return null;

        if (previewCamera)
        {
            previewCamera.targetTexture = rendTexture;
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0); // transparent over UI
            previewCamera.allowHDR = false;
        }

        if (targetImage) targetImage.texture = rendTexture;

        return rendTexture;
    }

    private static RenderTexture Create(int width, int height)
    {
        var w = Mathf.Clamp(width, 128, Mathf.Min(SystemInfo.maxTextureSize, 1024));
        var h = Mathf.Clamp(height, 128, Mathf.Min(SystemInfo.maxTextureSize, 1024));

        var rt = new RenderTexture(w, h, 16, RenderTextureFormat.ARGB32)
        {
            name = $"PreviewRT_{w}x{h}",
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        if (rt.Create() && rt.IsCreated())
            return rt;

        rt.Release();
        Object.Destroy(rt);

        var fallback = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
        {
            name = "PreviewRT_Fallback_256",
            antiAliasing = 1,
            useMipMap = false,
            autoGenerateMips = false,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        fallback.Create();
        return fallback;
    }
}