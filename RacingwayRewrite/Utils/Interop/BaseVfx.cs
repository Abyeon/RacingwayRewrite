using System;
using RacingwayRewrite.Utils.Interop.Structs;

namespace RacingwayRewrite.Utils.Interop;

public abstract unsafe class BaseVfx : IDisposable
{
    public string Path = "";
    public DateTime Expires;
    public bool Loop;
    
    public VfxData* Data;
    public bool IsValid => Data != null && Data->Instance != null;

    protected abstract void Refresh();

    public void CheckForRefresh()
    {
        if (!IsValid) Refresh();
    }
    
    public void Dispose()
    {
        if (Plugin.VfxFunctions == null) throw new NullReferenceException("Vfx functions are not initialized");

        Plugin.Framework.RunOnFrameworkThread(() =>
        {
            if (IsValid)
            {
                Plugin.VfxFunctions.DestroyVfx(Data);
                Data = null;
            }
        });
        
        GC.SuppressFinalize(this);
    }
}
