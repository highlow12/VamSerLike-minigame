using UnityEngine;
using UnityEngine.UI;

public class Journal : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    Image img;
    float elapsedTime = 0;
    int i = 0;

    void Start()
    {
        img = GetComponent<Image>();
        img.sprite = sprites[i];
    }

    void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        if (elapsedTime >= 0.2f) {
            elapsedTime = 0f;
            img.sprite = sprites[i++ % sprites.Length];
        }
    }
}
