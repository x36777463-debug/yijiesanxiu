using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FacilityItemView : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text nameText;
    public TMP_Text infoText;
    public TMP_Text upgradeButtonText;
    public Button upgradeButton;

    FacilityConfig _cfg;
    FacilityData _data;

    bool _guideOn;

    public void Bind(FacilityConfig cfg, FacilityData data)
    {
        _cfg = cfg;
        _data = data;

        upgradeButton.onClick.AddListener(OnClickUpgrade);
        Refresh();
    }

    void OnClickUpgrade()
    {
        bool ok = GameManager.Instance.TryUpgrade(_cfg.id);

        if (ok)
        {
            AudioManager.Upgrade();
            UIAnimator.Punch(upgradeButton.transform, 1.10f, 0.14f);
        }
        else
        {
            AudioManager.Error();
        }

        Refresh();
    }

    public void Refresh()
    {
        if (_cfg == null || _data == null) return;

        nameText.text = _cfg.displayName;

        // 未解锁：告诉玩家需要什么境界
        if (!GameManager.Instance.IsUnlocked(_cfg))
        {
            var realm = ConfigDatabase.Instance.GetRealm(_cfg.unlockRealm);
            string realmName = realm != null ? realm.displayName : "???";

            infoText.text = $"需要 {realmName} 解锁";
            upgradeButtonText.text = "未解锁";
            upgradeButton.interactable = false;
            SetGuide(false);
            return;
        }

        double output = GameManager.Instance.GetFacilityOutput(_cfg.id);
        infoText.text = $"Lv.{_data.level}   {NumberFormatter.Format(output)}/秒";

        double cost = GameManager.Instance.GetUpgradeCost(_cfg.id);
        upgradeButtonText.text = $"升级 {NumberFormatter.Format(cost)}";
        upgradeButton.interactable = GameManager.Instance.CanUpgrade(_cfg.id);

        // 新手引导：开局自带等级的那个设施（打坐），在玩家还没升过级时呼吸提示
        bool needGuide = _cfg.startLevel > 0
                         && _data.level <= _cfg.startLevel
                         && GameManager.Instance.Data.totalSpirit < 30;

        SetGuide(needGuide);
    }

    void SetGuide(bool on)
    {
        if (on == _guideOn) return;
        _guideOn = on;

        if (on)
        {
            UIAnimator.Breathe(upgradeButton.transform, 0.97f, 1.07f, 1.1f);
        }
        else
        {
            var runner = upgradeButton.GetComponent<UITweenRunner>();
            if (runner != null) runner.StopAll();
        }
    }
}
