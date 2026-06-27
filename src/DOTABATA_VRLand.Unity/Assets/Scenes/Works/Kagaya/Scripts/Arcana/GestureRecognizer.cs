using UnityEngine;
using System.Collections.Generic;
using PDollarGestureRecognizer;
using TMPro;
using System;

public class GestureRecognizer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private TMP_InputField shapesType;
    [SerializeField] private TMP_InputField saveFileName;

    public enum GestureClass {
        Circle = 0,
        Star = 1,
        Diamond = 2,
        Square = 3,
        Triangle = 4,
        Heart = 5,
    }

    // 図形判定後コールバック
    public Action<GestureClass, float> CompleteRecognize;

    /// <summary>
    /// 図形判定
    /// </summary>
    public bool Recognize(List<Point> gesturePoints) {
        if (gesturePoints.Count < 10) {
            Debug.Log("点が少なすぎ");
            resultText.text = "Miss";
            return false;
        }

        List<string> xmlNames = new List<string>() {
            "circle_1.xml",
            "circle_2.xml",
            "circle_3.xml",
            "circle_4.xml",
            "circle_5.xml",
            "circle_6.xml",
            "circle_7.xml",
            "circle_8.xml",
            "circle_9.xml",
            "circle_10.xml",
            "circle_11.xml",
            "circle_12.xml",
            "star_1.xml",
            "star_2.xml",
            "star_3.xml",
            "star_4.xml",
            "star_5.xml",
            "diamond_1.xml",
            "diamond_2.xml",
            "diamond_3.xml",
            "diamond_4.xml",
            "diamond_5.xml",
            "square_1.xml",
            "square_2.xml",
            "square_3.xml",
            "square_4.xml",
            "square_5.xml",
            "square_6.xml",
            "square_7.xml",
            "triangle_1.xml",
            "triangle_2.xml",
            "triangle_3.xml",
            "triangle_4.xml",
            "triangle_5.xml",
            "heart_1.xml",
            "heart_2.xml",
            "heart_3.xml",
            "heart_4.xml",
            "heart_5.xml",
        };


        List<Gesture> gestures = new List<Gesture>();

        // ファイルを文字列として読む
        foreach (var xmlName in xmlNames) {
            string xml = System.IO.File.ReadAllText(Application.dataPath + "/Gestures/" + xmlName);
            gestures.Add(GestureIO.ReadGestureFromXML(xml));
        }

        Gesture candidate = new Gesture(gesturePoints.ToArray());

        Result result = PointCloudRecognizer.Classify(candidate, gestures.ToArray());

        Debug.Log($"結果: {result.GestureClass} / スコア: {result.Score}");

        if (result.Score > 0.93f) {
            Debug.Log("成功: " + result.GestureClass);
            resultText.text = $"{result.GestureClass}\n" +
                $"Score:{result.Score}";

            bool parseResult = EnumExs.TryParseFromString<GestureClass>(result.GestureClass, true, out GestureClass gestureClass);
            if (CompleteRecognize != null) {
                CompleteRecognize(gestureClass, result.Score);
                return true;
            }

            return false;
        }
        else {
            Debug.Log("失敗");
            resultText.text = "Miss";
            return false;
        }
    }

    /// <summary>
    /// 図形モデル保存
    /// </summary>
    private void SaveGesture(List<Point> gesturePoints) {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.L)) {
            GestureIO.WriteGesture(gesturePoints.ToArray(), shapesType.text, Application.dataPath + $"/Gestures/{saveFileName.text}.xml");
            Debug.Log("保存した");
        }
    }
}
