using System;
using System.Reflection.Metadata;
using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Utility;

namespace RacingwayRewrite.Utils;

public class FontManager : IDisposable
{
    public IFontHandle? FontHandle { get; set; }
    public string FontName { get; set; } = "No Font Found";

    public FontManager()
    {
        try
        {
            FontHandle = Plugin.Configuration.TimerFont.CreateFontHandle(Plugin.PluginInterface.UiBuilder.FontAtlas);
            FontName = Plugin.Configuration.TimerFont.ToLocalizedString(Plugin.ClientState.ClientLanguage.ToCode());
        }
        catch (Exception e)
        {
            Plugin.Configuration.TimerFont = new SingleFontSpec
            {
                FontId = new GameFontAndFamilyId(GameFontFamily.Axis),
                SizePt = 34.0f
            };
            
            Plugin.Chat.Error("Could not load font.");
            Plugin.Log.Error(e.ToString());
        }
    }

    public void ResetFont()
    {
        Plugin.Configuration.TimerFont = new SingleFontSpec
        {
            FontId = new GameFontAndFamilyId(GameFontFamily.Axis),
            SizePt = 34.0f
        };

        var font = Plugin.Configuration.TimerFont;
        FontHandle = font.CreateFontHandle(Plugin.PluginInterface.UiBuilder.FontAtlas);
        
        var locale = Plugin.ClientState.ClientLanguage.ToCode();
        var fontFamily = font.FontId.Family.GetLocalizedName(locale);
        var fontStyle = font.FontId.GetLocalizedName(locale);
        fontStyle = fontStyle.Equals(fontFamily) ? "" : $" - {fontStyle}";
        FontName = $"{fontFamily}{fontStyle} ({font.SizePt}pt)";
    }
    
    public void Dispose()
    {
        FontHandle?.Dispose();
        GC.SuppressFinalize(this);
    }
    
    public bool FontReady => FontHandle is { Available: true };
}
