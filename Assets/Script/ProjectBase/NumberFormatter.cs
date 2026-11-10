public static class NumberFormatter
{
    public static string Format(double v)
    {
        if (v < 10000) return v.ToString("F1");
        if (v < 1e8) return (v / 1e4).ToString("F2") + "万";
        if (v < 1e12) return (v / 1e8).ToString("F2") + "亿";
        return (v / 1e12).ToString("F2") + "兆";
    }
}