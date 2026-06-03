using UnityEngine;
using System.Collections.Generic;
using PDollarGestureRecognizer;
using TMPro;
using System;

public class GestureRecognizer : MonoBehaviour {
    private LineRenderer lineRenderer;
    private List<Vector3> linePoints = new List<Vector3>();
    private List<Point> gesturePoints = new List<Point>();

    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private TMP_InputField shapesType;
    [SerializeField] private TMP_InputField saveFileName;

    private int strokeId = -1;
    private Camera cam;

    public enum GestureClass {
        Circle,
        Star,
        Diamond,
        Square,
        Triangle,
        Heart,
    }

    // 図形判定後
    public Action<GestureClass, float> CompleteRecognize;

    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        cam = Camera.main;

        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 0.1f;
    }

    private void Update() {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // ← カメラからの距離

        Vector3 pos = cam.ScreenToWorldPoint(mousePos);
        pos.z = 0f;

        if (Input.GetMouseButtonDown(0)) {
            Debug.Log("StartDraw");
            strokeId++;
            linePoints.Clear();
            gesturePoints.Clear();
            lineRenderer.positionCount = 0;
        }
        if (Input.GetMouseButton(0)) {
            // 点が近すぎる場合は追加しない（重要：ノイズ対策）
            if (linePoints.Count == 0 || Vector3.Distance(linePoints[^1], pos) > 0.05f) {
                linePoints.Add(pos);
                lineRenderer.positionCount = linePoints.Count;
                lineRenderer.SetPosition(linePoints.Count - 1, pos);

                gesturePoints.Add(new Point(pos.x, pos.y, strokeId));
            }
        }
        if (Input.GetMouseButtonUp(0)) {
            Debug.Log("EndDraw");
            // 点が近すぎる場合は追加しない（重要：ノイズ対策）
            if (linePoints.Count == 0 || Vector3.Distance(linePoints[^1], pos) > 0.05f) {
                linePoints.Add(pos);
                lineRenderer.positionCount = linePoints.Count;
                lineRenderer.SetPosition(linePoints.Count - 1, pos);

                gesturePoints.Add(new Point(pos.x, pos.y, strokeId));
            }

            // 判定
            Recognize();
        }

        //SaveGesture();
    }

    private void Recognize() {
        if (gesturePoints.Count < 10) {
            Debug.Log("点が少なすぎ");
            resultText.text = "Miss";
            return;
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

            GestureClass gestureClass = (GestureClass)Enum.Parse(typeof(GestureClass), result.GestureClass, true);
            if (CompleteRecognize != null) {
                CompleteRecognize(gestureClass, result.Score);
            }
        }
        else {
            Debug.Log("失敗");
            resultText.text = "Miss";
        }
    }

    private void SaveGesture() {
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.L)) {
            GestureIO.WriteGesture(gesturePoints.ToArray(), shapesType.text, Application.dataPath + $"/Gestures/{saveFileName.text}.xml");
            Debug.Log("保存した");
        }
    }
}
