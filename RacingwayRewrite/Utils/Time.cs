using System;
using System.Numerics;
using System.Text;
using Dalamud.Interface.Colors;

namespace RacingwayRewrite.Utils;

public class Time
{
    public static Vector4 GetParseColor(double percent)
    {
        return percent switch
        {
            < 0.25 => ImGuiColors.ParsedGrey,
            < 0.49 => ImGuiColors.ParsedGreen,
            < 0.74 => ImGuiColors.ParsedBlue,
            < 0.94 => ImGuiColors.ParsedPurple,
            < 0.98 => ImGuiColors.ParsedOrange,
            < 1 => ImGuiColors.ParsedPink,
            1 => ImGuiColors.ParsedGold,
            _ => throw new ArgumentOutOfRangeException(nameof(percent), percent, null)
        };
    }
    
    public static string PrettyFormatTimeSpan(TimeSpan span)
    {
        StringBuilder sb = new StringBuilder();

        if (span.TotalMicroseconds < 0)
            sb.Append("-");
        if (span.Days > 0)
            sb.Append($"{span.Days}:");
        if (span.Hours > 0)
            sb.Append($"{span.Hours}:");

        sb.Append($"{Math.Abs(span.Minutes):00}:{Math.Abs(span.Seconds):00}.{Math.Abs(span.Milliseconds):000}");

        return sb.ToString();
    }

    public static string PrettyFormatTimeSpanSigned(TimeSpan span)
    {
        StringBuilder sb = new StringBuilder();

        if (span.TotalMicroseconds < 0)
        {
            sb.Append("-");
        } else
        {
            sb.Append("+");
        }

        if (span.Days > 0)
            sb.Append($"{span.Days}:");
        if (span.Hours > 0)
            sb.Append($"{span.Hours}:");

        sb.Append($"{Math.Abs(span.Minutes):00}:{Math.Abs(span.Seconds):00}.{Math.Abs(span.Milliseconds):000}");

        return sb.ToString();
    }
}
