using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    const int CurrentVersion = 1;

    static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static GameData Load()
    {
        if (!File.Exists(SavePath)) return null;

        try
        {
            string json = File.ReadAllText(SavePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            if (data == null) return null;

            Migrate(data);
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[存档] 读取失败：{e.Message}");
            return null;      // 坏档就当没有，开新档，别让游戏崩
        }
    }

    public static void Save(GameData data)
    {
        if (data == null) return;

        try
        {
            data.version = CurrentVersion;
            data.lastSaveTime = DateTime.UtcNow.ToString("o");

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[存档] 写入失败：{e.Message}");
        }
        Debug.Log($"[存档] 已保存到 {SavePath}");
    }

    public static void Delete()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    static void Migrate(GameData data)
    {
        if (data.facilities == null)
            data.facilities = new System.Collections.Generic.List<FacilityData>();

        if (data.version < 1)
            data.version = 1;

        // 以后加新字段就在这里补默认值：
        // if (data.version < 2) { data.xxx = 默认值; data.version = 2; }
    }

    public static string GetSavePath() => SavePath;
}