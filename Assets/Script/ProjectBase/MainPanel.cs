using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text spiritText;
    public TMP_Text realmText;
    public Transform facilityListRoot;
    public FacilityItemView itemPrefab;
    public OfflinePanel offlinePanel;
    public SettingsPanel settingsPanel;
    public Button settingsButton;

    [Tooltip("可选：自动存档时闪一下的提示文字，没建就留空")]
    public TMP_Text saveHintText;

    readonly List<FacilityItemView> _items = new List<FacilityItemView>();

    float _punchTimer;
    double _lastSpirit = -1;

    void Start()
    {
        if (settingsButton != null) settingsButton.onClick.AddListener(OnClickSettings);

        BuildList();

        var gm = GameManager.Instance;
        gm.OnSpiritChanged += OnSpiritChanged;
        gm.OnRealmChanged += OnRealmChangedEvent;
        gm.OnDataReset += RebuildList;
        gm.OnSaved += OnSavedHint;

        OnSpiritChanged(gm.Data.spirit);
        RefreshRealmText();

        if (saveHintText != null) saveHintText.text = "";

        ShowOfflineIfAny();
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnSpiritChanged -= OnSpiritChanged;
        GameManager.Instance.OnRealmChanged -= OnRealmChangedEvent;
        GameManager.Instance.OnDataReset -= RebuildList;
        GameManager.Instance.OnSaved -= OnSavedHint;
    }

    void Update()
    {
        TickSpiritPunch();

        // ESC / 安卓返回键：开着的关掉，没开的打开设置
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (settingsPanel == null) return;

        if (settingsPanel.IsOpen) settingsPanel.Hide();
        else OnClickSettings();
    }

    /// <summary>灵气数字每 0.5 秒轻轻弹一下，让玩家"感觉到"在赚钱</summary>
    void TickSpiritPunch()
    {
        if (spiritText == null || GameManager.Instance == null || GameManager.Instance.Data == null) return;

        _punchTimer += Time.unscaledDeltaTime;
        if (_punchTimer < 0.5f) return;

        _punchTimer = 0f;
        double cur = GameManager.Instance.Data.spirit;

        if (_lastSpirit >= 0 && cur > _lastSpirit + 0.01)
            UIAnimator.Punch(spiritText.transform, 1.04f, 0.10f);

        _lastSpirit = cur;
    }

    void ShowOfflineIfAny()
    {
        var pending = GameManager.Instance.PendingOffline;
        if (pending == null) return;

        if (offlinePanel != null)
            offlinePanel.Show(pending.Value.seconds, pending.Value.gainedSpirit);

        GameManager.Instance.ClearPendingOffline();
    }

    void BuildList()
    {
        foreach (var cfg in ConfigDatabase.Instance.AllFacilities)
        {
            var data = GameManager.Instance.Data.facilities.Find(f => f.id == cfg.id);
            if (data == null) continue;

            var item = Instantiate(itemPrefab, facilityListRoot);
            item.Bind(cfg, data);
            _items.Add(item);
        }
    }

    void OnSpiritChanged(double value)
    {
        spiritText.text = "灵气 " + NumberFormatter.Format(value);

        foreach (var it in _items) it.Refresh();
    }

    void OnRealmChangedEvent(int _)
    {
        RefreshRealmText();

        // 境界提升会解锁新设施，全部刷新一遍
        foreach (var it in _items) it.Refresh();
    }

    void RefreshRealmText()
    {
        var realm = GameManager.Instance.CurrentRealm;
        if (realmText != null && realm != null)
            realmText.text = realm.displayName;
    }

    void OnClickSettings()
    {
        AudioManager.Click();
        if (settingsPanel != null) settingsPanel.Show();
    }

    void OnSavedHint()
    {
        if (saveHintText == null) return;

        saveHintText.text = "已保存";
        CancelInvoke(nameof(ClearSaveHint));
        Invoke(nameof(ClearSaveHint), 1.2f);
    }

    void ClearSaveHint()
    {
        if (saveHintText != null) saveHintText.text = "";
    }

    /// <summary>清档/读新档后，旧条目引用的数据已经作废，全部重建</summary>
    void RebuildList()
    {
        foreach (var it in _items)
        {
            if (it != null) Destroy(it.gameObject);
        }
        _items.Clear();

        BuildList();

        RefreshRealmText();
        OnSpiritChanged(GameManager.Instance.Data.spirit);

        _lastSpirit = -1;
    }
}
