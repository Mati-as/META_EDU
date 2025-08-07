using System.Collections;
using System.Collections.Generic;
using System.IO;
using MyCustomizedEditor.Common.Util;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UI_ContentGuideImage : UI_PopUp
{
    public static int MonthOfImageToShow;
    public static Dictionary<int,Sprite> ContentGuideImageCache = new Dictionary<int, Sprite>();
    public override bool IsBackBtnClickable
    {
        get
        {
            return true;
        }
    }

    private enum UI
    {
        ContentGuideImage
    }

    private Image _contentInfoImage;

    public override bool InitOnLoad()
    {
        base.InitOnLoad();
        BindObject(typeof(UI));


        _contentInfoImage = GetObject((int)UI.ContentGuideImage).GetComponent<Image>();
        StartCoroutine(LoadThumbnailImage());
        return true;
    }

    public IEnumerator LoadThumbnailImage()
    {
        if (ContentGuideImageCache.TryGetValue(MonthOfImageToShow, out Sprite cachedSprite))
        {
            // ✅ 4. 캐시된 이미지가 있다면 바로 적용
            _contentInfoImage.sprite = cachedSprite;
            yield break;
        }
        
        string thumbnailImagePath = Path.Combine(Application.streamingAssetsPath,
            $"ContentGuideImages/CgImage_{MonthOfImageToShow}.png");
        string thumbnailUri = "file://" + thumbnailImagePath;

        // ✅ 3. 웹 요청 (로컬에서 로딩)
        using (var uwrThumb = UnityWebRequestTexture.GetTexture(thumbnailUri))
        {
            yield return uwrThumb.SendWebRequest();

            if (uwrThumb.result != UnityWebRequest.Result.Success)
            {
                  Logger.CoreClassLog($"❌ 썸네일 로딩 실패: {$"ContentGuideImages/CgImage_{MonthOfImageToShow}.png"} - {uwrThumb.error}");
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(uwrThumb);

                var sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f); // Optional: pixelsPerUnit 설정


                // ✅ 5. 적용
                _contentInfoImage.sprite = sprite;
                
                // ✅ 6. 캐시에 저장
                if (!ContentGuideImageCache.ContainsKey(MonthOfImageToShow))
                {
                    ContentGuideImageCache[MonthOfImageToShow] = sprite;
                }
                else
                {
                    Logger.CoreClassLog($"⚠️ 캐시에 중복된 키가 있습니다: {MonthOfImageToShow}");
                }
            }
        }
    }
}