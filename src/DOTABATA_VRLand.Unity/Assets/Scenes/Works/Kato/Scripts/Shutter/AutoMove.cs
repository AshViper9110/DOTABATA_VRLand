using UnityEngine;

public class AutoMove : MonoBehaviour
{
    public ShutterGameManager shuttergameManager;
    public float speed = 1f;
    public float stopDistance = 10f;

    void Update()
    {
        Shutter current = shuttergameManager.GetCurrentShutter();
        if (current == null) return;

        float distance = Vector3.Distance(transform.position, current.transform.position);

        if (distance > stopDistance)
        {
            transform.position += Vector3.forward * speed * Time.deltaTime;

            shuttergameManager.canInput = false;
        }
        else
        {
            shuttergameManager.canInput = true;
        }
    }
}