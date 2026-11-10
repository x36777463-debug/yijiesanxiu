using System.Collections;
using UnityEngine;

/// <summary>
/// 轻量 UI 动画工具。用协程实现，不依赖任何插件。
/// UIAnimator.Punch(xxx.transform) 就能让控件弹一下。
/// </summary>
public static class UIAnimator
{
    /// <summary>
    /// 让控件"弹一下"：放大到 peak 再回到原大小。
    /// </summary>
    public static void Punch(Transform target, float peak = 1.08f, float duration = 0.12f)
    {
        if (target == null) return;

        var runner = target.GetComponent<UITweenRunner>();
        if (runner == null) runner = target.gameObject.AddComponent<UITweenRunner>();

        runner.Punch(peak, duration);
    }

    /// <summary>
    /// 呼吸效果（适合新手引导），返回 runner 方便停止。
    /// </summary>
    public static UITweenRunner Breathe(Transform target, float min = 0.96f, float max = 1.06f, float period = 1.2f)
    {
        if (target == null) return null;

        var runner = target.GetComponent<UITweenRunner>();
        if (runner == null) runner = target.gameObject.AddComponent<UITweenRunner>();

        runner.Breathe(min, max, period);
        return runner;
    }
}

/// <summary>真正跑动画的组件，由 UIAnimator 自动添加</summary>
public class UITweenRunner : MonoBehaviour
{
    Vector3 _baseScale = Vector3.one;
    Coroutine _co;
    bool _baseCaptured;

    void Awake()
    {
        CaptureBase();
    }

    void CaptureBase()
    {
        if (_baseCaptured) return;
        _baseScale = transform.localScale;
        _baseCaptured = true;
    }

    public void Punch(float peak, float duration)
    {
        CaptureBase();
        StopAll();
        _co = StartCoroutine(PunchRoutine(peak, duration));
    }

    public void Breathe(float min, float max, float period)
    {
        CaptureBase();
        StopAll();
        _co = StartCoroutine(BreatheRoutine(min, max, period));
    }

    public void StopAll()
    {
        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }
        transform.localScale = _baseScale;
    }

    void OnDisable()
    {
        // 对象被隐藏时恢复原大小，避免下次打开是压缩状态
        transform.localScale = _baseScale;
        _co = null;
    }

    IEnumerator PunchRoutine(float peak, float duration)
    {
        float half = Mathf.Max(0.01f, duration * 0.5f);

        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(_baseScale, _baseScale * peak, t / half);
            yield return null;
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(_baseScale * peak, _baseScale, t / half);
            yield return null;
        }

        transform.localScale = _baseScale;
        _co = null;
    }

    IEnumerator BreatheRoutine(float min, float max, float period)
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime;
            float k = (Mathf.Sin(t / period * Mathf.PI * 2f) + 1f) * 0.5f;  // 0~1
            transform.localScale = _baseScale * Mathf.Lerp(min, max, k);
            yield return null;
        }
    }
}
