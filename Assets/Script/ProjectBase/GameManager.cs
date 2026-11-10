using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("配置")]
    public GameConfig gameConfig;

    public GameData Data { get; private set; }
    public double Rate { get; private set; }

    public event Action<double> OnSpiritChanged;
    public event Action<double> OnRateChanged;
    public event Action<int> OnRealmChanged;

    /// <summary>整份数据被替换（开新档 / 清档）时触发，UI 需要重建</summary>
    public event Action OnDataReset;

    /// <summary>自动存档完成时触发，UI 可以闪一下"已保存"</summary>
    public event Action OnSaved;

    public OfflineSystem.Result? PendingOffline { get; private set; }
    public void ClearPendingOffline() => PendingOffline = null;

    float _tickTimer;

    public bool IsPaused { get; private set; } = true;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ConfigDatabase.Instance.LoadAll(gameConfig);
        LoadOrNewGame();
    }

    void Start()
    {
        if (gameConfig != null && gameConfig.autoSaveInterval > 0)
            InvokeRepeating(nameof(AutoSave), gameConfig.autoSaveInterval, gameConfig.autoSaveInterval);
    }

    void Update()
    {
        if (IsPaused) return;

        _tickTimer += Time.deltaTime;
        if (_tickTimer < 0.1f) return;

        Tick(_tickTimer);
        _tickTimer = 0f;
    }

    void Tick(float dt)
    {
        double gain = Rate * dt;
        Data.spirit += gain;
        Data.totalSpirit += gain;

        OnSpiritChanged?.Invoke(Data.spirit);
    }
    public void Resume()
    {
        IsPaused = false;
        _tickTimer = 0f;
        Debug.Log("[游戏] 开始");
    }

    public void Pause()
    {
        IsPaused = true;
        SaveSystem.Save(Data);
    }
    // ---------------- 境界 ----------------

    public RealmConfig CurrentRealm => ConfigDatabase.Instance.GetRealm(Data.realmIndex);
    public RealmConfig NextRealm => ConfigDatabase.Instance.GetRealm(Data.realmIndex + 1);

    public bool IsUnlocked(FacilityConfig cfg)
    {
        return cfg != null && cfg.unlockRealm <= Data.realmIndex;
    }

    public bool IsUnlocked(string facilityId)
    {
        return IsUnlocked(ConfigDatabase.Instance.GetFacility(facilityId));
    }

    /// <summary>突破进度 0~1</summary>
    public double GetBreakthroughProgress()
    {
        var cur = CurrentRealm;
        if (cur == null || cur.breakthroughCost <= 0) return 1.0;
        return Data.totalSpirit / cur.breakthroughCost;
    }

    public bool CanBreakthrough()
    {
        if (NextRealm == null) return false;      // 已经是最后一个境界

        var cur = CurrentRealm;
        if (cur == null) return false;

        return Data.totalSpirit >= cur.breakthroughCost;
    }

    public bool TryBreakthrough()
    {
        if (!CanBreakthrough()) return false;

        Data.realmIndex++;

        RecalcRate();
        OnRealmChanged?.Invoke(Data.realmIndex);
        OnSpiritChanged?.Invoke(Data.spirit);

        // 突破是重要节点，立即存盘，防止崩溃丢进度
        SaveSystem.Save(Data);

        Debug.Log($"[境界] 突破成功 → {CurrentRealm?.displayName}");
        return true;
    }

    // ---------------- 存档 ----------------

    void LoadOrNewGame()
    {
        GameData loaded = SaveSystem.Load();

        if (loaded == null)
        {
            Data = CreateNewGame();
            Debug.Log("[存档] 没有存档，开新档");
        }
        else
        {
            Data = loaded;
            FillMissingFacilities();

            // 配置里境界变少了，防止索引越界
            int maxIndex = Math.Max(0, ConfigDatabase.Instance.RealmCount - 1);
            if (Data.realmIndex > maxIndex) Data.realmIndex = maxIndex;

            Debug.Log($"[存档] 读取成功，灵气 {Data.spirit:F1}");
        }

        RecalcRate();

        if (loaded != null)
        {
            var r = OfflineSystem.Calc(Data, gameConfig, Rate);

            if (r.HasReward)
            {
                Data.spirit += r.gainedSpirit;
                Data.totalSpirit += r.gainedSpirit;
                PendingOffline = r;

                Debug.Log($"[离线] {r.seconds / 3600.0:F1} 小时，获得 {r.gainedSpirit:F1} 灵气");
            }
        }
    }

    GameData CreateNewGame()
    {
        var data = new GameData();

        foreach (var cfg in ConfigDatabase.Instance.AllFacilities)
            data.facilities.Add(new FacilityData { id = cfg.id, level = cfg.startLevel });

        return data;
    }

    // 配置里新增了设施，旧存档里没有 → 补一条
    void FillMissingFacilities()
    {
        foreach (var cfg in ConfigDatabase.Instance.AllFacilities)
        {
            if (Data.facilities.Find(f => f.id == cfg.id) == null)
            {
                Debug.Log($"[存档] 补齐新设施：{cfg.id}");
                Data.facilities.Add(new FacilityData { id = cfg.id, level = 0 });
            }
        }
    }

    void AutoSave()
    {
        SaveSystem.Save(Data);
        OnSaved?.Invoke();
    }

    public void ForceSave() => SaveSystem.Save(Data);

    public void ResetSave()
    {
        SaveSystem.Delete();
        Data = CreateNewGame();
        RecalcRate();
        OnSpiritChanged?.Invoke(Data.spirit);
        OnRealmChanged?.Invoke(Data.realmIndex);
        OnDataReset?.Invoke();
    }

    void OnApplicationPause(bool paused)
    {
        if (paused && Data != null) SaveSystem.Save(Data);
    }

    void OnApplicationQuit()
    {
        if (Data != null) SaveSystem.Save(Data);
    }

    // ---------------- 数值 ----------------

    public void RecalcRate()
    {
        double total = 0;

        foreach (var fd in Data.facilities)
        {
            var cfg = ConfigDatabase.Instance.GetFacility(fd.id);
            if (cfg == null) continue;
            if (!IsUnlocked(cfg)) continue;      // 未解锁的设施不产出

            total += cfg.baseOutput * fd.level;
        }

        var realm = CurrentRealm;
        double mult = realm != null ? realm.multiplier : 1.0;

        Rate = total * mult;
        OnRateChanged?.Invoke(Rate);
    }

    /// <summary>单个设施的实际每秒产出（含境界倍率）</summary>
    public double GetFacilityOutput(string id)
    {
        var fd = Data.facilities.Find(f => f.id == id);
        if (fd == null) return 0;

        var cfg = ConfigDatabase.Instance.GetFacility(id);
        if (cfg == null || !IsUnlocked(cfg)) return 0;

        var realm = CurrentRealm;
        double mult = realm != null ? realm.multiplier : 1.0;

        return cfg.baseOutput * fd.level * mult;
    }

    public double GetUpgradeCost(string id)
    {
        var fd = Data.facilities.Find(f => f.id == id);
        if (fd == null) return 0;

        var cfg = ConfigDatabase.Instance.GetFacility(id);
        if (cfg == null) return 0;

        return cfg.baseCost * Math.Pow(cfg.costGrowth, fd.level);
    }

    public bool CanUpgrade(string id)
    {
        var cfg = ConfigDatabase.Instance.GetFacility(id);
        if (cfg == null) return false;
        if (!IsUnlocked(cfg)) return false;

        return Data.spirit >= GetUpgradeCost(id);
    }

    public bool TryUpgrade(string id)
    {
        var fd = Data.facilities.Find(f => f.id == id);
        if (fd == null) return false;

        var cfg = ConfigDatabase.Instance.GetFacility(id);
        if (cfg == null) return false;
        if (!IsUnlocked(cfg)) return false;

        double cost = GetUpgradeCost(id);
        if (Data.spirit < cost) return false;

        Data.spirit -= cost;
        fd.level++;

        RecalcRate();
        OnSpiritChanged?.Invoke(Data.spirit);
        return true;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
