using System.Collections.Generic;
using UnityEngine;

public class ConfigDatabase
{
    private static ConfigDatabase _instance;
    public static ConfigDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new ConfigDatabase();
            return _instance;
        }
    }

    Dictionary<string, FacilityConfig> _dict = new Dictionary<string, FacilityConfig>();
    List<FacilityConfig> _ordered = new List<FacilityConfig>();
    List<RealmConfig> _realms = new List<RealmConfig>();
    bool _loaded;

    public IReadOnlyList<FacilityConfig> AllFacilities => _ordered;
    public IReadOnlyList<RealmConfig> AllRealms => _realms;
    public int RealmCount => _realms.Count;

    public void LoadAll(GameConfig gameConfig)
    {
        if (_loaded) return;
        _loaded = true;

        _dict.Clear();
        _ordered.Clear();
        _realms.Clear();

        foreach (var cfg in gameConfig.facilities)
        {
            if (cfg == null) continue;
            if (string.IsNullOrEmpty(cfg.id))
            {
                Debug.LogError($"[配置] {cfg.name} 的 id 是空的");
                continue;
            }
            if (_dict.ContainsKey(cfg.id))
            {
                Debug.LogError($"[配置] 重复的 id: {cfg.id}");
                continue;
            }

            _dict.Add(cfg.id, cfg);
            _ordered.Add(cfg);
        }

        foreach (var r in gameConfig.realms)
        {
            if (r == null) continue;
            _realms.Add(r);
        }

        // 按 realmIndex 排序，防止 Inspector 里拖的顺序错了
        _realms.Sort((a, b) => a.realmIndex.CompareTo(b.realmIndex));

        if (_realms.Count == 0)
            Debug.LogError("[配置] 境界列表是空的，突破功能不会生效");
    }

    public FacilityConfig GetFacility(string id)
    {
        return _dict.TryGetValue(id, out var cfg) ? cfg : null;
    }

    public RealmConfig GetRealm(int index)
    {
        if (index < 0 || index >= _realms.Count) return null;
        return _realms[index];
    }
}
