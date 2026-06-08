using UnityEngine;

public class RodManager : MonoBehaviour
{
    [SerializeField]ParticleSystem Effect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Effect.Stop();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
