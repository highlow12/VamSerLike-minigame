using System.Collections;
using UnityEngine;

public class Stage1VFX : MonoBehaviour
{
    float time = 0f;
    public VFXManager vfxManager;

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator VFX()
    {
        vfxManager.AnimateVoronoi(0f, 0.5f, 1500f);
        yield return new WaitForSeconds(1.5f);
        vfxManager.AnimateVoronoi(0.5f, 0, 1500f);
         yield return new WaitForSeconds(1.5f);
        vfxManager.Normalize();
        yield return new WaitForSeconds(177f);

        vfxManager.BlackOut();
        yield return new WaitForSeconds(2f);
        // 그림 일기 연출
        vfxManager.Normalize();
        // 그림 일기 소환
    }
}
