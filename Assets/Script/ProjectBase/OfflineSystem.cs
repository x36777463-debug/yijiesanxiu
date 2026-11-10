using System;

public static class OfflineSystem
{
    public struct Result
    {
        public double seconds;
        public double gainedSpirit;

        public bool HasReward => gainedSpirit > 0;
    }

    public static Result Calc(GameData data, GameConfig cfg, double rate)
    {
        var result = new Result();

        if (data == null || cfg == null) return result;
        if (string.IsNullOrEmpty(data.lastSaveTime)) return result;
        if (rate <= 0) return result;

        if (!DateTime.TryParse(data.lastSaveTime, null,
                System.Globalization.DateTimeStyles.RoundtripKind, out DateTime last))
            return result;

        double seconds = (DateTime.UtcNow - last).TotalSeconds;

        // 坑 1：玩家把系统时间往前调，秒数会是负数
        if (seconds <= 0) return result;

        // 坑 2：离线三个月，收益要封顶
        double maxSeconds = cfg.maxOfflineHours * 3600.0;
        if (seconds > maxSeconds) seconds = maxSeconds;

        result.seconds = seconds;
        result.gainedSpirit = rate * seconds * cfg.offlineEfficiency;
        return result;
    }
}