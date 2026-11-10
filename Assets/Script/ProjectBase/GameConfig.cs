using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "修仙/全局配置", order = 1)]
public class GameConfig : ScriptableObject
{
    [Header("设施清单（按显示顺序拖进来）")]
    public List<FacilityConfig> facilities = new List<FacilityConfig>();

    [Header("境界清单（按境界顺序拖进来）")]
    public List<RealmConfig> realms = new List<RealmConfig>();

    [Header("全局参数")]
    public float tickInterval = 0.1f;
    public float autoSaveInterval = 30f;
    public float maxOfflineHours = 8f;

    [Header("离线")]
    [Tooltip("离线收益效率，0~1")]
    public float offlineEfficiency = 0.5f;
}
