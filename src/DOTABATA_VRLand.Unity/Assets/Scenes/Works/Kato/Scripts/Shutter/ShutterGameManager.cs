using UnityEngine;

public class ShutterGameManager : MonoBehaviour
{
    public Shutter[] shutters;
    int currentIndex = 0;

    public bool canInput = false;

    public GameObject finishText;

    void Start()
    {
        UpdateShutters();
    }

    public void NextShutter()
    {
        currentIndex++;
        UpdateShutters();

        if (currentIndex >= shutters.Length)
        {
            Debug.Log("ƒNƒŠƒAI");
            finishText.SetActive(true);
        }
    }

    void UpdateShutters()
    {
        for (int i = 0; i < shutters.Length; i++)
        {
            bool isCurrent = (i == currentIndex);
        }
    }

    public Shutter GetCurrentShutter()
    {
        if (currentIndex < shutters.Length)
            return shutters[currentIndex];

        return null;
    }
}