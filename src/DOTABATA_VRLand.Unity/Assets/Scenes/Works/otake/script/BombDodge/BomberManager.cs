using UnityEngine;

public class BomberManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject,1.0f);
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
                //TODO:©•ª‚¾‚Á‚½ê‡€–S‚µ‚½‚±‚Æ‚ğ’Ê’m
                RoomModel.I.HitBomber();

            }
        }
    }
}
