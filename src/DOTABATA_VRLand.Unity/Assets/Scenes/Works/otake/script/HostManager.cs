using DG.Tweening;
using UnityEngine;

public class HostManager :MonoBehaviour 
{
    [SerializeField]SkinnedMeshRenderer meshRenderer;

    public enum facial
    {
        None,
        Normal,
        Smile,
    
    }

    public float range;

    public int Vec = 1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MoveHost();
    }


    // Update is called once per frame
    void Update()
    {
        if(meshRenderer.GetBlendShapeWeight(3) >=0)
        {
            meshRenderer.SetBlendShapeWeight(3, meshRenderer.GetBlendShapeWeight(3)-10);
        }
    }

    public void MoveHost()
    {
        Vec = -Vec;

        

        Debug.Log(Vec);
        this.transform.DOMove(new Vector3(transform.position.x,transform.position.y + (range*Vec), transform.position.z), 2f).SetLoops(2,LoopType.Yoyo).OnComplete(() =>
        {
            MoveHost();
        });
    }

    public void ChengeFace(facial faceType)
    {
        meshRenderer.SetBlendShapeWeight(0, 0);
        meshRenderer.SetBlendShapeWeight(1, 0);
        meshRenderer.SetBlendShapeWeight(2, 0);

        switch (faceType)
        {
            case facial.None:
                meshRenderer.SetBlendShapeWeight(0, 100);
                break;
            case facial.Normal:
                meshRenderer.SetBlendShapeWeight(1, 100);
                break;
            case facial.Smile:
                meshRenderer.SetBlendShapeWeight(2, 100);
                break;
        }
    }

    public void MoveBeard()
    {
        meshRenderer.SetBlendShapeWeight(3, 100);
    }
}
