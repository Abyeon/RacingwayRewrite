using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using RacingwayRewrite.Race;

namespace RacingwayRewrite.Utils;

public static class SeStringExtensions
{
    /// <summary>
    /// Adds a Player Payload to the SeString
    /// </summary>
    /// <param name="player">Reference to player</param>
    /// <returns>The current builder.</returns>
    public static SeStringBuilder AddPlayer(this SeStringBuilder builder, Player player)
    {
        PlayerPayload payload = new PlayerPayload(player.Name, player.HomeworldRow);
        SeString playerLink = new SeString(payload);
        return builder.Append(playerLink);
    }
}
