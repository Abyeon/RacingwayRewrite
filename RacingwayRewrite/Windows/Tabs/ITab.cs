using System;

namespace RacingwayRewrite.Windows.Tabs;

public interface ITab : IDisposable
{
    string Name { get; }
    void Draw();
}
