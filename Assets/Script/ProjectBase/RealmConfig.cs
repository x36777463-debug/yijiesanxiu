using UnityEngine;

[CreateAssetMenu(fileName = "Realm_", menuName = "修仙/境界配置", order = 2)]
public class RealmConfig : ScriptableObject
{
    [Tooltip("0=练气 1=筑基 2=金丹 3=元婴 4=化神")]
    public int realmIndex;

    public string displayName;

    [Tooltip("全局产出倍率")]
    public float multiplier = 1f;

    [Tooltip("突破到下一个境界需要的累计灵气。最后一个境界填 0")]
    public float breakthroughCost;
}