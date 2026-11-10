using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Header("UI 引用")]
    public Button continueButton;
    public Button newGameButton;
    public TMP_Text newGameButtonText;
    public Button quitButton;

    bool _confirmingNew;

    void Awake()
    {
        continueButton.onClick.AddListener(OnContinue);
        newGameButton.onClick.AddListener(OnNewGame);
        quitButton.onClick.AddListener(OnQuit);
    }

    void OnEnable()
    {
        RefreshButtons();
    }

    void RefreshButtons()
    {
        bool hasSave = SaveSystem.HasSave();
        continueButton.interactable = hasSave;
    }

    // ---------- 继续 ----------
    void OnContinue()
    {
        Hide();
    }

    // ---------- 新的开始（有存档时二次确认）----------
    void OnNewGame()
    {
        if (SaveSystem.HasSave() && !_confirmingNew)
        {
            _confirmingNew = true;
            newGameButtonText.text = "再点一次（覆盖存档）";
            Invoke(nameof(CancelConfirm), 3f);
            return;
        }

        CancelInvoke(nameof(CancelConfirm));
        _confirmingNew = false;
        newGameButtonText.text = "新 的 开 始";

        GameManager.Instance.ResetSave();
        Hide();
    }

    void CancelConfirm()
    {
        _confirmingNew = false;
        newGameButtonText.text = "新 的 开 始";
    }

    // ---------- 退出 ----------
    void OnQuit()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void Hide()
    {
        gameObject.SetActive(false);
        GameManager.Instance.Resume();
    }
}
