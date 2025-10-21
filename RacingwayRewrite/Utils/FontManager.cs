using System;
using System.Reflection.Metadata;
using Dalamud.Interface;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;

namespace RacingwayRewrite.Utils;

public class FontManager : IDisposable
{
    public IFontHandle? FontHandle { get; set; }
    public string FontName { get; set; } = "No Font Found";

    public FontManager()
    {
        try
        {
            if (Plugin.Configuration.TimerFont != null)
            {
                FontHandle = Plugin.Configuration.TimerFont.CreateFontHandle(Plugin.PluginInterface.UiBuilder.FontAtlas);
                FontName = Plugin.Configuration.TimerFont.ToLocalizedString(System.Globalization.CultureInfo.CurrentCulture.Name);
            }
            else
            {
                ResetFont();
            }
        }
        catch (Exception e)
        {
            Plugin.Configuration.TimerFont = null;
            Plugin.Chat.Error("Could not load font.");
            Plugin.Log.Error(e.ToString());
        }
    }

    public void ResetFont()
    {
        var fontStyle = new GameFontStyle()
        {
            SizePt = 34.0f,
            FamilyAndSize = GameFontFamilyAndSize.Axis36
        };
                
        FontHandle = Plugin.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(fontStyle);
        FontName = "Default";
    }
    
    public void Dispose()
    {
        if (FontPushed) PopFont();
        FontHandle?.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool FontPushed = false;
    public bool FontReady => FontHandle is { Available: true };

    public void PushFont()
    {
        try
        {
            if (FontPushed)
            {
                throw new InvalidOperationException("Font has already been pushed.");
            }

            if (FontReady)
            {
                FontHandle!.Push();
                FontPushed = true;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
    }

    public void PopFont()
    {
        try
        {
            if (FontPushed)
            {
                FontHandle!.Pop();
                FontPushed = false;
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error(e.ToString());
        }
    }
}
