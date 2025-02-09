using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpawnPatternTestButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        MonsterPatternManager.Instance.SpawnNextPattern();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
}
