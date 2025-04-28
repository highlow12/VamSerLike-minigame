using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.SampleScene
{
    public class SceneController : MonoBehaviour
    {
        // 씬 이름을 인스펙터에서 설정할 수 있도록 public 변수로 선언
        public string sceneToLoad = "SampleScene";

        // 씬을 로드하는 메서드
        public void LoadScene()
        {
            // 현재 씬과 동일한 씬을 로드하려고 하면 에러 방지
            if (SceneManager.GetActiveScene().name != sceneToLoad)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.Log("Already in " + sceneToLoad);
            }
        }

        // 씬을 빌드 인덱스로 로드하는 대체 메서드
        public void LoadSceneByIndex(int sceneIndex)
        {
            // 유효한 씬 인덱스인지 확인
            if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(sceneIndex);
            }
            else
            {
                Debug.LogError("Invalid scene index!");
            }
        }
    }
}