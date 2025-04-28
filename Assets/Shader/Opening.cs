using UnityEngine;
using UnityEngine.UI;

public class Opening : MonoBehaviour
{
    [SerializeField] private Sprite[] sprites;

    Image img;
    int i = 0;

    void Start()
    {
        img = GetComponent<Image>();
        img.sprite = sprites[0];
    }

    public void onClickImage()
    {
        i++;
        if (i >= sprites.Length) {
            onClickSkip();
            return;
        }
        img.sprite = sprites[i];
    }

    public void onClickSkip()
    {
        // 다음 Scene로 이동
    }
}
