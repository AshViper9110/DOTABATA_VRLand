using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using static PositionChecker;

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
    public Vector3 posOffset = new Vector3(0f, 0f, 0f);

    private EventSystem eventSystem;

    private GameObject currentHover;

    [SerializeField] private float scrollSpeed = 2;

    // 右スティック
    [SerializeField] private SteamVR_Action_Vector2 rightStickAction;


    [SerializeField] private SnapTurn snapTurn;

    void Start()
    {
        if (pointerOrigin == null)
        {
            pointerOrigin = transform;
        }

        eventSystem = EventSystem.current;

        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;

            lineRenderer.startWidth = 0.005f;
            lineRenderer.endWidth = 0.005f;

            //lineRenderer.enabled = false;

            lineRenderer.alignment =
                LineAlignment.View;

            lineRenderer.numCornerVertices = 8;
            lineRenderer.numCapVertices = 8;

            lineRenderer.useWorldSpace = true;
        }
    }

    void Update()
    {
        GraphicRaycaster[] raycasters =
            FindObjectsOfType<GraphicRaycaster>();

        Quaternion rotation =
            pointerOrigin.rotation *
            Quaternion.Euler(rotationOffset);

        Vector3 direction =
            rotation * Vector3.forward;

        Vector3 start =
            pointerOrigin.position + posOffset;

        Vector3 end =
            start + direction * rayDistance;

        bool hitUI = false;

        PointerEventData pointerData =
            new PointerEventData(eventSystem);

        Vector2 screenPoint =
            Camera.main.WorldToScreenPoint(end);

        pointerData.position = screenPoint;

        List<RaycastResult> results =
            new List<RaycastResult>();

        foreach (GraphicRaycaster raycaster in raycasters)
        {
            if (raycaster == null)
                continue;

            if (!raycaster.isActiveAndEnabled)
                continue;

            raycaster.Raycast(pointerData, results);
        }

        GameObject target = null;

        foreach (RaycastResult result in results)
        {
            GameObject obj = result.gameObject;

            Button button =
                obj.GetComponentInParent<Button>();

            InputField inputField =
                obj.GetComponentInParent<InputField>();

            TMP_InputField tmpInputField =
                obj.GetComponentInParent<TMP_InputField>();

            Toggle toggle = obj.GetComponentInParent<Toggle>();

            ScrollRect scrollRect = obj.GetComponent<ScrollRect>();
            if (rightStickAction != null) {
                if (!scrollRect) {
                    scrollRect = button?.GetComponentInParent<ScrollRect>();
                }
                snapTurn.enabled = true;
                if (scrollRect) {
                    snapTurn.enabled = false;
                }
            }

            // Button / InputField のみ対象
            if (button == null &&
                inputField == null &&
                tmpInputField == null &&
                toggle == null)
            {
                continue;
            }

            hitUI = true;

            target =
                ExecuteEvents.GetEventHandler
                <IPointerEnterHandler>(obj);

            end = result.worldPosition;

            // Hover開始
            if (target != currentHover)
            {
                if (currentHover != null)
                {
                    ExecuteEvents.Execute(
                        currentHover,
                        pointerData,
                        ExecuteEvents.pointerExitHandler
                    );
                }

                if (target != null)
                {
                    ExecuteEvents.Execute(
                        target,
                        pointerData,
                        ExecuteEvents.pointerEnterHandler
                    );
                }

                currentHover = target;
            }

            // Trigger
            if (triggerAction != null &&
                triggerAction.stateDown)
            {
                // Button
                if (button != null)
                {
                    button.onClick?.Invoke();

                    Debug.Log(
                        "Clicked Button : " +
                        button.name);
                }

                // InputField
                if (inputField != null)
                {
                    StartCoroutine(
                        FocusInputField(inputField));

                    Debug.Log(
                        "Selected InputField : " +
                        inputField.name);
                }

                // TMP_InputField
                if (tmpInputField != null)
                {
                    StartCoroutine(
                        FocusTMPInputField(tmpInputField));

                    Debug.Log(
                        "Selected TMP_InputField : " +
                        tmpInputField.name);
                }

                // Toggle
                if (toggle != null)
                {
                    toggle.Select();

                    toggle.isOn = !toggle.isOn;

                    Debug.Log(
                        "Toggled : " +
                        toggle.name);
                }
            }
            // Turn
            if (rightStickAction != null &&
                rightStickAction.axis != Vector2.zero) {
                // ScrollRect
                if (scrollRect != null) {
                    float y = rightStickAction.axis.y;
                    scrollRect.verticalNormalizedPosition += y * scrollSpeed * Time.deltaTime;

                    Debug.Log(
                        "Scroll View : " +
                         scrollRect.name);
                }
            }

            break;
        }

        // Hover解除
        if (!hitUI)
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

            // 何もない場所でTrigger
            if (triggerAction != null &&
                triggerAction.stateDown)
            {
                // UI選択解除
                eventSystem.SetSelectedGameObject(null);

                Debug.Log("UI Selection Cleared");
            }
        }

        // レーザー
        if (lineRenderer != null)
        {
            lineRenderer.enabled = hitUI;

            if (hitUI)
            {
                lineRenderer.SetPosition(0, start);
                lineRenderer.SetPosition(1, end);
            }
        }

        Debug.DrawLine(start, end, Color.cyan);
    }

    private IEnumerator FocusInputField(
    InputField input)
    {
        yield return null;

        eventSystem.SetSelectedGameObject(null);

        yield return null;

        eventSystem.SetSelectedGameObject(
            input.gameObject);

        input.Select();

        input.ActivateInputField();

        input.MoveTextEnd(false);

        Debug.Log(
            "InputField Focused : " +
            input.isFocused);
    }
    private IEnumerator FocusTMPInputField(
    TMP_InputField input)
    {
        yield return null;

        eventSystem.SetSelectedGameObject(null);

        yield return null;

        eventSystem.SetSelectedGameObject(
            input.gameObject);

        input.Select();

        input.ActivateInputField();

        input.MoveTextEnd(false);

        Debug.Log(
            "TMP Focused : " +
            input.isFocused);
    }
}