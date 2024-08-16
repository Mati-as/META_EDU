using UnityEngine;
using UnityEngine.UI;

public class ThumbnailImageController : MonoBehaviour
{
    private Image _thumbnail;

    private void Start()
    {
        _thumbnail = GetComponent<Image>();
        var sprite = Resources.Load<Sprite>("Common/Launcher_ThumbnailImage/" + ExtractThumbnailSuffix(gameObject.name));

        if (sprite != null)
        {
            _thumbnail.sprite = sprite;
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning($"sprite is null, check the resource : {ExtractThumbnailSuffix(gameObject.name)}");
#endif
        }
    }

    public string ExtractThumbnailSuffix(string input)
    {
        var prefix = "ThumbnailImage_";


        if (input.Contains(prefix))
        {
            return input.Substring(prefix.Length);
        }
#if UNITY_EDITOR
        Debug.LogWarning($"This image is not set to Thumbnail image.{gameObject.name}");
#endif
        return input;
    }
}