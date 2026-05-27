using UnityEngine;

public class InputManager : MonoBehaviour
{
    public ShutterGameManager shuttergameManager;

    Vector2 startPos;
    bool isDragging = false;

    public Shutter[] shutters;
    int currentIndex = 0;

    void Update()
    {
        if (shuttergameManager.isGameOver)
            return;

        if (!shuttergameManager.canInput)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Vector2 endPos = Input.mousePosition;
            Vector2 dir = (endPos - startPos).normalized;

            Shutter.Direction inputDir = GetDirection(dir);

            Shutter current = shuttergameManager.GetCurrentShutter();
            if (current != null)
            {
                current.TryOpen(inputDir);
            }

            isDragging = false;
        }
    }

    Shutter.Direction GetDirection(Vector2 dir)
    {
        if (Vector2.Dot(dir, Vector2.up) > 0.7f)
            return Shutter.Direction.Up;
        if (Vector2.Dot(dir, Vector2.down) > 0.7f)
            return Shutter.Direction.Down;
        if (Vector2.Dot(dir, Vector2.right) > 0.7f)
            return Shutter.Direction.Right;
        return Shutter.Direction.Left;
    }
}