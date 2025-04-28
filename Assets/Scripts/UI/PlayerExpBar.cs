using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerExpBar : MonoBehaviour
{
    [SerializeField] private Slider experienceSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    [SerializeField] private float fillSpeed = 2f;

    private float targetFillAmount = 0f;

    private void Start()
    {
        // Slider ������Ʈ�� ���ٸ� �ڵ����� �������� �õ�
        if (experienceSlider == null)
        {
            experienceSlider = GetComponent<Slider>();
            if (experienceSlider == null)
            {
                Debug.LogError("PlayerExpBar�� Slider ������Ʈ�� �����ϴ�!");
            }
        }

        UpdateUI();
        Debug.Log("PlayerExpBar �ʱ�ȭ �Ϸ�");
    }

    private void Update()
    {
        // �ε巯�� ����ġ �� ���� ȿ��
        if (GameManager.Instance != null && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            targetFillAmount = GameManager.Instance.playerExperience / GameManager.Instance.experienceToLevelUp;

            if (Mathf.Abs(experienceSlider.value - targetFillAmount) > 0.005f)
            {
                experienceSlider.value = Mathf.Lerp(experienceSlider.value, targetFillAmount, Time.deltaTime * fillSpeed);

                // ��ǥ ���� ����� ��������� ��Ȯ�� ������ ����
                if (Mathf.Abs(experienceSlider.value - targetFillAmount) < 0.01f)
                {
                    experienceSlider.value = targetFillAmount;
                }
            }

            // ���� �ؽ�Ʈ ������Ʈ �߰�
            if (levelText != null)
            {
                levelText.text = GameManager.Instance.playerLevel.ToString();
            }
        }
    }

    // �޼��带 public���� �����Ͽ� �ܺο��� ���� �����ϰ� ��
    public void UpdateUI()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance�� null�Դϴ�!");
            return;
        }

        UpdateLevelText();
        targetFillAmount = GameManager.Instance.playerExperience / GameManager.Instance.experienceToLevelUp;
        experienceSlider.value = targetFillAmount;

        Debug.Log($"����ġ �� ���� ������Ʈ: {targetFillAmount * 100:F1}% (����ġ: {GameManager.Instance.playerExperience}/{GameManager.Instance.experienceToLevelUp})");
    }

    // ���� �ؽ�Ʈ ������Ʈ
    private void UpdateLevelText()
    {
        Debug.Log($"UpdateLevelText ȣ���. GameManager null üũ: {GameManager.Instance != null}");

        if (levelText != null && GameManager.Instance != null)
        {
            Debug.Log($"���� ����: {GameManager.Instance.playerLevel}");
            levelText.text = "Lv. " + GameManager.Instance.playerLevel.ToString();
        }
        else
        {
            Debug.LogError("levelText �Ǵ� GameManager.Instance�� null�Դϴ�!");
        }
    }
}