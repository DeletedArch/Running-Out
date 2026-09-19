using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Sprite ButtonClickedSprite;
    [SerializeField] private Sprite ButtonNormalSprite;
    [SerializeField] private Button[] buttons;
    [SerializeField] private float buttonTextMoveDistance = 2f; 
    
    void Start()
    {
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            int index = i; // Capture the current index for the closure
            buttons[i].onClick.AddListener(() => HandleButtonClick(buttons[index], true));
        }
    }

    public void HandleButtonClick(Button button, bool isClicked)
    {
        if (button != null)
        {
            button.image.sprite = ButtonClickedSprite;
            UniTask.Delay(200).ContinueWith(() =>
            {
                button.image.sprite = ButtonNormalSprite;
            }).Forget();
            // Look for text
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                RectTransform rectTransform = buttonText.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.position = rectTransform.position - Vector3.up * buttonTextMoveDistance;
                    UniTask.Delay(200).ContinueWith(() =>
                    {
                        rectTransform.position = rectTransform.position + Vector3.up * buttonTextMoveDistance;
                    }).Forget();
                }
            }
        }
    }
}
