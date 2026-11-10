using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BreakthroughBar : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_Text progressText;
    public TMP_Text buttonText;
    public Button breakButton;

    void Start()
    {
        if (breakButton != null)
            breakButton.onClick.AddListener(OnClick);

        GameManager.Instance.OnSpiritChanged += OnValueChanged;
        GameManager.Instance.OnRealmChanged += OnRealmChanged;

        Refresh();
    }

    void OnDestroy()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnSpiritChanged -= OnValueChanged;
        GameManager.Instance.OnRealmChanged -= OnRealmChanged;
    }

    void OnClick()
    {
        bool ok = GameManager.Instance.TryBreakthrough();

        if (ok)
        {
            AudioManager.Breakthrough();

            // 按钮弹一下，更有"成了"的手感
            UIAnimator.Punch(breakButton.transform, 1.15f, 0.28f);
        }
        else
        {
            AudioManager.Error();
        }
    }

    void OnValueChanged(double _) => Refresh();
    void OnRealmChanged(int _) => Refresh();

    void Refresh()
    {
        var gm = GameManager.Instance;
        if (gm == null || gm.Data == null) return;

        var cur = gm.CurrentRealm;
        var next = gm.NextRealm;

        // 已经到最后一个境界
        if (next == null)
        {
            progressText.text = "已达最高境界";
            buttonText.text = "圆满";
            breakButton.interactable = false;
            return;
        }

        double need = cur != null ? cur.breakthroughCost : 0;
        double have = gm.Data.totalSpirit;

        progressText.text =
            $"累计 {NumberFormatter.Format(have)} / {NumberFormatter.Format(need)}   →   {next.displayName}";

        bool can = gm.CanBreakthrough();
        buttonText.text = can ? $"突破 {next.displayName}" : "尚未满足";
        breakButton.interactable = can;
    }
}
