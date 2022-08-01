using Cysharp.Threading.Tasks;
using SimpleFileBrowser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public static class TextureExtensions
{
    public static async UniTask<Texture2D> LoadTexture(string path)
    {
        bool cached = false;
        if (Context.AndroidVersionCode > 29)
        {
            path = StorageUtil.CopyToCache(path);
            cached = true;
        }

        using var request = UnityWebRequestTexture.GetTexture("file://" + path);
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            throw new Exception(request.error);

        var texture = DownloadHandlerTexture.GetContent(request);
        texture.wrapMode = TextureWrapMode.Clamp; // This fixes a 1 px border around the image

        if(cached)
            StorageUtil.DeleteFromCache(path);

        return texture;
    }

    public static Texture2D Copy(this Texture2D source)
    {
        var tex = new Texture2D(source.width, source.height, source.format, source.mipmapCount, false);
        tex.wrapMode = source.wrapMode;
        Graphics.CopyTexture(source, tex);
        return tex;
    }

    /// <summary>
    /// Applies Gaussian blur to a texture.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="ammount"></param>
    public static void Blur(this Texture2D tex, int ammount)
    {
        Context.Instance.BlurMaterial.SetFloat("_KernelSize", ammount);
        var rt = new RenderTexture(tex.width, tex.height, 0);
        Graphics.Blit(tex, rt, Context.Instance.BlurMaterial);
        tex.ReadPixels(new Rect(0f, 0f, rt.width, rt.height), 0, 0, false);
        tex.Apply();
    }

    /// <summary>
    /// Returns a blurred copy of the texture.
    /// </summary>
    /// <param name="tex"></param>
    /// <param name=""></param>
    /// <returns></returns>
    public static Texture2D Blurred(this Texture2D original, int ammount)
    {
        var tex = original.Copy();
        tex.Blur(ammount);
        return tex;
    }

    public static Texture2D Blurred(this Texture original, int ammount) => Blurred((Texture2D)original, ammount);
}
