using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI 引用（留空会自动生成）")]
    public TMP_Text versionText;
    public TMP_Text resetButtonText;
    public Button resetButton;
    public Button menuButton;
    public Button quitButton;
    public Button closeButton;
    public Button volumeButton;
    public MainMenuPanel mainMenu;

    /// <summary>面板当前是否打开（ESC 键逻辑要用）</summary>
    public bool IsOpen => gameObject.activeSelf;

    TMP_Text _volumeButtonText;
    bool _confirming;
    bool _built;

    void Awake()
    {
        BuildMissingButtons();

        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClicked);
        if (menuButton != null) menuButton.onClick.AddListener(OnBackToMenu);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuit);
        if (volumeButton != null) volumeButton.onClick.AddListener(OnVolumeClicked);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (versionText != null)
            versionText.text = $"版本 {Application.version}";

        RefreshVolumeLabel();
        ResetConfirmState();
    }

    public void Hide()
    {
        ResetConfirmState();
        gameObject.SetActive(false);
    }

    // ---------------- 自动生成缺失的按钮 ----------------

    void BuildMissingButtons()
    {
        if (_built) return;
        _built = true;

        if (resetButton == null) return;

        if (volumeButton == null)
        {
            volumeButton = CloneFromReset(GetVolumeLabel(), out _volumeButtonText);
            volumeButton.name = "VolumeButton";
        }

        if (menuButton == null) menuButton = CloneFromReset("返回主菜单", out _);
        if (quitButton == null) quitButton = CloneFromReset("退出游戏", out _);

        if (mainMenu == null)
            mainMenu = FindObjectOfType<MainMenuPanel>(true);

        LayoutButtons();
    }

    Button CloneFromReset(string label, out TMP_Text labelText)
    {
        var src = resetButton.gameObject;
        var go = Instantiate(src, src.transform.parent);

        var btn = go.GetComponent<Button>();
        if (btn != null)
            btn.onClick = new Button.ButtonClickedEvent();   // 清掉复制来的监听

        labelText = go.GetComponentInChildren<TMP_Text>(true);
        if (labelText != null) labelText.text = label;

        go.name = "Btn_" + label;
        return btn;
    }

    void LayoutButtons()
    {
        // 对话框加高，好放下五个按钮
        if (resetButton != null && resetButton.transform.parent is RectTransform dlg)
            dlg.sizeDelta = new Vector2(dlg.sizeDelta.x, 1000f);

        if (versionText != null && versionText.transform is RectTransform vrt)
            vrt.anchoredPosition = new Vector2(vrt.anchoredPosition.x, -90f);

        SetY(volumeButton, 340f);
        SetY(resetButton, 170f);
        SetY(menuButton, 0f);
        SetY(quitButton, -170f);
        SetY(closeButton, -340f);
    }

    void SetY(Button b, float y)
    {
        if (b == null) return;

        var rt = (RectTransform)b.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, y);
    }

    // ---------------- 音量 ----------------

    string GetVolumeLabel()
    {
        return AudioManager.Instance != null
            ? AudioManager.Instance.GetVolumeLabel()
            : "音量：80%";
    }

    void RefreshVolumeLabel()
    {
        if (_volumeButtonText == null && volumeButton != null)
            _volumeButtonText = volumeButton.GetComponentInChildren<TMP_Text>(true);

        if (_volumeButtonText != null)
            _volumeButtonText.text = GetVolumeLabel();
    }

    void OnVolumeClicked()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.CycleVolume();

        RefreshVolumeLabel();
    }

    // ---------------- 其他按钮 ----------------

    void ResetConfirmState()
    {
        CancelInvoke(nameof(CancelConfirm));
        _confirming = false;

        if (resetButtonText != null)
            resetButtonText.text = "清除存档";
    }

    void OnResetClicked()
    {
        // 第一次点只提示，3 秒内再点才真删
        if (!_confirming)
        {
            _confirming = true;
            resetButtonText.text = "再点一次确认";
            Invoke(nameof(CancelConfirm), 3f);
            return;
        }

        GameManager.Instance.ResetSave();
        Hide();
    }

    void CancelConfirm()
    {
        _confirming = false;
        if (resetButtonText != null) resetButtonText.text = "清除存档";
    }

    void OnBackToMenu()
    {
        GameManager.Instance.Pause();
        Hide();

        if (mainMenu != null)
            mainMenu.gameObject.SetActive(true);
    }

    void OnQuit()
    {
        if (GameManager.Instance != null) GameManager.Instance.ForceSave();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
