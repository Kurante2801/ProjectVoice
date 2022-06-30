using Cysharp.Threading.Tasks;
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
        using var request = UnityWebRequestTexture.GetTexture("file://" + path);
        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            throw new Exception(request.error);

        var texture = DownloadHandlerTexture.GetContent(request);
        texture.wrapMode = TextureWrapMode.Clamp; // This fixes a 1 px border around the image

        return texture;
    }
}
