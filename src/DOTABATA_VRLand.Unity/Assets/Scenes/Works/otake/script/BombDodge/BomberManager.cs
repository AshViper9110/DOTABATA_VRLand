using UnityEngine;

public class BomberManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.parent != null)
        {

            if (other.transform.parent.transform.parent.gameObject.GetComponent<PlayerTransform>())
            {
                BombDogePlayer player = other.transform.parent.transform.parent.gameObject.GetComponent<BombDogePlayer>();
                player.isDead = true;
            }
        }
    }
}
