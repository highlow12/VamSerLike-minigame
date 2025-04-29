using System.Collections;
using UnityEngine;

public class Stage2VFX : MonoBehaviour
{
    public GameObject handParticle;
    public GameObject bloodParticle;

    void Start()
    {
        StartCoroutine(VFX());

        VFXManager.Instance.handParticle = handParticle;
        VFXManager.Instance.bloodParticle = bloodParticle;
    }

    void Update()
    {

    }

    IEnumerator VFX()
    {
        VFXManager.Instance.Normalize();
        // VFXManager.Instance.HandParticle();
        // yield return new WaitForSeconds(5f);
        // VFXManager.Instance.HandParticle();
        yield return new WaitForSeconds(180f);
        VFXManager.Instance.HandParticle();

        yield return new WaitForSeconds(88f);
        VFXManager.Instance.AnimateNoise();

        yield return new WaitForSeconds(2f);
        VFXManager.Instance.Normalize();
        // 그림 일기 소환
        yield return new WaitForSeconds(5f);

    }
}
