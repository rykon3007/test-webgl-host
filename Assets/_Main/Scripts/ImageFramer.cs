using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ImageFramer : MonoBehaviour
{
    public string imageUrl = "https://placehold.co/200x300.jpg";
    public Material imageMaterial; // Unlit/Texture の空マテリアルをアサイン

    void Start()
    {
        StartCoroutine(LoadImage());
    }

    IEnumerator LoadImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("画像取得失敗: " + request.error);
            yield break;
        }

        Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

        // マテリアルをコピーして設定（重要）
        Material runtimeMat = new Material(imageMaterial);
        runtimeMat.mainTexture = texture;

        // ここが肝心！！Renderer.material にセットする
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = runtimeMat;

        Debug.Log("画像を適用しました");
    }
}
