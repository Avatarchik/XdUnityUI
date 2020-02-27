using UnityEngine;

/**
 * 【Unity】Device Simulatorでノッチとセーフエリアの対策 - テラシュールブログ
 * http://tsubakit1.hateblo.jp/entry/2019/10/30/235150
 */
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class SafeAreaPadding : MonoBehaviour
{
    private DeviceOrientation postOrientation;
    private RectTransform _rect;

    private void Start()
    {
        _rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (Input.deviceOrientation != DeviceOrientation.Unknown && postOrientation == Input.deviceOrientation)
            return;

        postOrientation = Input.deviceOrientation;

        var area = Screen.safeArea;
        var resolution = Screen.currentResolution;

        _rect.sizeDelta = Vector2.zero;
        _rect.anchorMax = new Vector2(area.xMax / resolution.width, area.yMax / resolution.height);
        _rect.anchorMin = new Vector2(area.xMin / resolution.width, area.yMin / resolution.height);
    }
}