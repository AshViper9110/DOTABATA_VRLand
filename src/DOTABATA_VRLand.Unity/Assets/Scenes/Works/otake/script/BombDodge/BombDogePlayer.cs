using UnityEngine;

public class BombDogePlayer : MonoBehaviour
{
    public GameObject EngelRing;
    public bool isDead;

    PlayerTransform playerTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        EngelRing.SetActive(false);
        playerTransform = GetComponent<PlayerTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isDead)
        {

            if(!EngelRing.activeSelf)
            {
                EngelRing.SetActive(true);
            }

            EngelRing.transform.position = playerTransform.Head.position + (Vector3.up * 0.3f);
           
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        
    }
}
