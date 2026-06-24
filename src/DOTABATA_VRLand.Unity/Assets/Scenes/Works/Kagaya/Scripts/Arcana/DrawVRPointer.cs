using PDollarGestureRecognizer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class DrawVRPointer : MonoBehaviour {
    private GestureRecognizer gestureRecognizer;
    // 絵描き板
    private GameObject drawBoadObj;

    // ポインターのオブジェクト
    private GameObject pointer;
    // 描く壁のレイヤー
    private LayerMask layerMask;
    // 描く位置
    private Vector3 drawPos;

    // インプット
    // 描き
    private SteamVR_Action_Boolean drawAction;
    private SteamVR_Input_Sources drawHandType;
    // 補助線
    private SteamVR_Action_Boolean drawGuideAction;


    private Material lineMaterial;
    private float lineWidth = 0.01f;

    private LineRenderer currentLine;
    private List<Vector3> points = new();
    private List<Point> gesturePoints = new List<Point>();
    // 魔法を撃つためのガイドライン
    private LineRenderer guideLine;

    // 判定VFX
    private GameObject recognizeVFX;

    /// <summary>
    /// フィールド設定
    /// </summary>
    public void SetField(GameObject drawBoad, GameObject pointer, Material material, GameObject recognizeVFX) {
        drawBoadObj = drawBoad;
        this.pointer = Instantiate(pointer);
        lineMaterial = material;
        this.recognizeVFX = recognizeVFX;
    }

    private void Start() {
        drawAction = SteamVR_Actions.default_InteractUI;
        drawHandType = SteamVR_Input_Sources.RightHand;

        drawGuideAction = SteamVR_Actions.default_GrabGrip;

        guideLine = GameObject.Find("GuideLine").GetComponent<LineRenderer>();
        guideLine.positionCount = 2;
        guideLine.startWidth = lineWidth;
        guideLine.endWidth = lineWidth;
        guideLine.enabled = false;

        gestureRecognizer = GameObject.Find("GestureRecognizer").GetComponent<GestureRecognizer>();
        layerMask = LayerMask.GetMask("DrawBoad");
    }

    private void Update() {
        MovePointer();
        GuideLines();

        StartLine();
        Draw();
        EndLine();
    }

    /// <summary>
    /// ポインター移動
    /// </summary>
    private void MovePointer() {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f, layerMask)) {
            // ポインターを円に
            pointer.GetComponentsInChildren<Transform>(true).First(_ => _.name == "DrawPointer").gameObject.SetActive(true);
            pointer.GetComponentsInChildren<Transform>(true).First(_ => _.name == "CrossHair").gameObject.SetActive(false);

            drawPos = hit.point + hit.normal * 0.005f;
            pointer.transform.position = drawPos;
            pointer.transform.rotation = Quaternion.LookRotation(-hit.normal);

        }
        else {
            // ポインターを十字に
            pointer.GetComponentsInChildren<Transform>(true).First(_ => _.name == "CrossHair").gameObject.SetActive(true);
            pointer.GetComponentsInChildren<Transform>(true).First(_ => _.name == "DrawPointer").gameObject.SetActive(false);

            pointer.transform.position = transform.position + Camera.main.transform.forward;
            pointer.transform.LookAt(Camera.main.transform);
        }
    }

    /// <summary>
    /// 描き始め
    /// </summary>
    private void StartLine() {
        if (!drawBoadObj.activeSelf) return;
        if (!drawAction.GetStateDown(drawHandType)) return;

        GameObject lineObj = new("Line");
        lineObj.transform.parent = GameObject.Find("Lines").transform;

        currentLine = lineObj.AddComponent<LineRenderer>();

        currentLine.material = lineMaterial;
        currentLine.startWidth = lineWidth;
        currentLine.endWidth = lineWidth;

        points.Clear();
        gesturePoints.Clear();
    }

    /// <summary>
    /// 描く
    /// </summary>
    private void Draw() {
        if (!drawBoadObj.activeSelf) return;
        if (!drawAction.GetState(drawHandType)) return;

        if (points.Count > 0) {
            float dist = Vector3.Distance(points[^1], drawPos);

            if (dist < 0.01f) return;
        }

        points.Add(drawPos);

        currentLine.positionCount = points.Count;
        currentLine.SetPositions(points.ToArray());
        gesturePoints.Add(new Point(drawPos.x, drawPos.y, 0));
    }

    /// <summary>
    /// 描き終わり
    /// </summary>
    private void EndLine() {
        if (!drawBoadObj.activeSelf) return;
        if (!drawAction.GetStateUp(drawHandType)) return;

        Destroy(currentLine.gameObject);
        currentLine = null;
        bool result = gestureRecognizer.Recognize(gesturePoints);
        Instantiate(recognizeVFX, drawBoadObj.transform.position, Quaternion.identity);
    }

    /// <summary>
    /// 魔法撃つ用補助線
    /// </summary>
    private void GuideLines() {
        if (drawGuideAction.GetStateDown(drawHandType)) {
            guideLine.enabled = true;
        }
        else if (drawGuideAction.GetState(drawHandType)) {
            guideLine.SetPosition(0, transform.position);
            guideLine.SetPosition(1, transform.position + Camera.main.transform.forward.normalized * 20f);
        }
        else if (drawGuideAction.GetStateUp(drawHandType)) {
            guideLine.enabled = false;
        }
    }
}
