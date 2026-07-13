using DG.Tweening;
using UnityEngine;

public class LoopRGBColor : MonoBehaviour {
    [SerializeField] private Material myMaterial;
    [SerializeField] private Gradient myGradient;

    private void Start() {
        myMaterial.DOGradientColor(myGradient, 3).SetLoops(-1);
    }
}
