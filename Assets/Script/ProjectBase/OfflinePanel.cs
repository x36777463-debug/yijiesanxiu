using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflinePanel : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text messageText;
    public Button claimButton;

    void Awake()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(Hide);
    }

    public void Show(double seconds, double gained)
    {
        gameObject.SetActive(true);

        double hours = seconds / 3600.0;
        messageText.text = $"离线 {hours:F1} 小时\n获得灵气 {NumberFormatter.Format(gained)}";
    }

    void Hide()
    {
        gameObject.SetActive(false);
    }
}