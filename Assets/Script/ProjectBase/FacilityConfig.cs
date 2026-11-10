using UnityEngine;

[CreateAssetMenu(fileName = "Facility_", menuName = "修仙/设施配置", order = 0)]
public class FacilityConfig : ScriptableObject
{
    [Header("基础信息")]
    [Tooltip("唯一键，代码里用这个找配置")]
    public string id;
    public string displayName;

    [Header("数值")]
    [Tooltip("1 级升 2 级的消耗")]
    public float baseCost = 10;
    [Tooltip("每级消耗倍率，建议 1.15")]
    public float costGrowth = 1.15f;
    [Tooltip("每级每秒产出")]
    public float baseOutput = 1;

    [Header("初始状态")]
    [Tooltip("开局送几级，0 表示未解锁")]
    public int startLevel = 0;

    [Header("解锁")]
    public int unlockRealm = 0;

    [Header("离线")]
    [Tooltip("离线收益效率，0~1，0.5 表示只有一半")]
    public float offlineEfficiency = 0.5f;
}