using System;
using System.Collections.Generic;
// 这里不需要继承 MonoBehaviour，因为是纯数据类
[Serializable]
public class GameData
{
    public int version = 1;
    public double spirit;
    public double totalSpirit;
    public int realmIndex;
    public string lastSaveTime;
    public List<FacilityData> facilities = new List<FacilityData>();
}