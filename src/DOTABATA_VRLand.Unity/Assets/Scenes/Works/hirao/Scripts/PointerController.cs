using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;

public class PointerController : MonoBehaviour
{
    [Header("SteamVR")]
    public SteamVR_Action_Boolean triggerAction;

    [Header("Pointer")]
    public Transform pointerOrigin;

    [Header("Laser")]
    public LineRenderer lineRenderer;

    [Header("Ray Settings")]
    public float rayDistance = 20f;

    [Header("Rotation Offset")]
    public Vector3 rotationOffset =
        new Vector3(25f, 0f, 0f);

    private EventSystem eventSystem;

    private GameObject currentHover;

    void Start()
    {
        if (pointerOrigin == null)
        {
            pointerOrigin = transform;
        }

        eventSystem = EventSystem.current;

        // LineRenderer 初期化
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;

            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;

            lineRenderer.enabled = false;

            // VR向け設定
            lineRenderer.alignment =
                LineAlignment.View;

            lineRenderer.numCornerVertices = 8;
            lineRenderer.numCapVertices = 8;

            lineRenderer.useWorldSpace = true;
        }
    }

    void Update()
    {
        // シーン内の全GraphicRaycaster取得
        GraphicRaycaster[] raycasters =
            FindObjectsOfType<GraphicRaycaster>();

        Quaternion rotation =
            pointerOrigin.rotation *
            Quaternion.Euler(rotationOffset);

        Vector3 direction =
            rotation * Vector3.forward;

        Vector3 start =
            pointerOrigin.position;

        Vector3 end =
            start + direction * rayDistance;

        bool hitButton = false;

        PointerEventData pointerData =
            new PointerEventData(eventSystem);

        // 画面座標化
        Vector2 screenPoint =
            Camera.main.WorldToScreenPoint(end);

        pointerData.position = screenPoint;

        List<RaycastResult> results =
            new List<RaycastResult>();

        // 全CanvasにRaycast
        foreach (GraphicRaycaster raycaster in raycasters)
        {
            if (raycaster == null)
                continue;

            if (!raycaster.isActiveAndEnabled)
                continue;

            if (raycaster.gameObject == null)
                continue;

            raycaster.Raycast(pointerData, results);
        }

        GameObject newHover = null;

        // Buttonのみ検出
        foreach (RaycastResult result in results)
        {
            Button button =
                result.gameObject
                .GetComponentInParent<Button>();

            if (button == null)
                continue;

            hitButton = true;

            GameObject target =
                button.gameObject;

            newHover =
                ExecuteEvents.GetEventHandler
                <IPointerEnterHandler>(target);

            end = result.worldPosition;

            // Hover開始
            if (newHover != currentHover)
            {
                // Hover解除
                if (currentHover != null)
                {
                    ExecuteEvents.Execute(
                        currentHover,
                        pointerData,
                        ExecuteEvents.pointerExitHandler
                    );
                }

                // Hover開始
                if (newHover != null)
                {
                    ExecuteEvents.Execute(
                        newHover,
                        pointerData,
                        ExecuteEvents.pointerEnterHandler
                    );
                }

                currentHover = newHover;
            }

            // Triggerクリック
            if (triggerAction != null &&
                triggerAction.stateDown)
            {
                ExecuteEvents.Execute(
                    target,
                    pointerData,
                    ExecuteEvents.pointerClickHandler
                );

                Debug.Log(
                    "Clicked : " + target.name);
            }

            // 最初のButtonだけ処理
            break;
        }

        // Hover解除
        if (!hitButton)
        {
            if (currentHover != null)
            {
                ExecuteEvents.Execute(
                    currentHover,
                    pointerData,
                    ExecuteEvents.pointerExitHandler
                );

                currentHover = null;
            }
        }

        // レーザー表示
        if (lineRenderer != null)
        {
            lineRenderer.enabled = hitButton;

            if (hitButton)
            {
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
            }
        }

        // Debug
        Debug.DrawLine(start, end, Color.cyan);
    }
}