using UnityEngine;
using TMPro;
using System.Collections;


public class Shutter : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }
    public Direction correctDirection;
    public TextMeshPro arrowText;

    public ShutterGameManager shuttergameManager;

    void Start()
    {
        correctDirection = (Direction)Random.Range(0, 4);

        UpdateArrow();
    }

    public void TryOpen(Direction input)
    {
        if (input == correctDirection)
        {
            Debug.Log("開いた！");
            StartCoroutine(OpenShutter());
        }
        else
        {
            Debug.Log("ミス！");
        }
    }

    Vector3 GetMoveDirection()
    {
        switch (correctDirection)
        {
            case Direction.Up:
                return Vector3.up;

            case Direction.Down:
                return Vector3.down;

            case Direction.Left:
                return Vector3.left;

            case Direction.Right:
                return Vector3.right;
        }

        return Vector3.zero;
    }

    IEnumerator OpenShutter()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + GetMoveDirection() * 5f;

        float time = 0;

        while (time < 1f)
        {
            time += Time.deltaTime * 3f;

            transform.position =
                Vector3.Lerp(startPos, endPos, time);

            yield return null;
        }

        shuttergameManager.NextShutter();

        //gameObject.SetActive(false);
    }

    void UpdateArrow()
    {
        switch (correctDirection)
        {
            case Direction.Up:
                arrowText.text = "↑";
                break;

            case Direction.Down:
                arrowText.text = "↓";
                break;

            case Direction.Left:
                arrowText.text = "←";
                break;

            case Direction.Right:
                arrowText.text = "→";
                break;
        }
    }
}