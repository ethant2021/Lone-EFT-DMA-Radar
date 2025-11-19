/*
 * Lone EFT DMA Radar - ESP Extension
 */

namespace LoneEftDmaRadar.UI.ESP
{
    /// <summary>
    /// ESP Configuration settings.
    /// </summary>
    public sealed class ESPConfig
    {
        /// <summary>
        /// Enable/Disable ESP.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// ESP Rendering Max FPS.
        /// </summary>
        [JsonPropertyName("maxFPS")]
        public int MaxFPS { get; set; } = 60;

        /// <summary>
        /// Max distance to render ESP elements.
        /// </summary>
        [JsonPropertyName("maxDistance")]
        public float MaxDistance { get; set; } = 500f;

        /// <summary>
        /// Font scale for ESP text.
        /// </summary>
        [JsonPropertyName("fontScale")]
        public float FontScale { get; set; } = 1.0f;

        /// <summary>
        /// Line width for skeleton rendering.
        /// </summary>
        [JsonPropertyName("lineWidth")]
        public float LineWidth { get; set; } = 2.0f;

        #region ESP Elements

        /// <summary>
        /// Show player ESP.
        /// </summary>
        [JsonPropertyName("showPlayers")]
        public bool ShowPlayers { get; set; } = true;

        /// <summary>
        /// Show skeleton bones.
        /// </summary>
        [JsonPropertyName("showSkeleton")]
        public bool ShowSkeleton { get; set; } = true;

        /// <summary>
        /// Show bounding box.
        /// </summary>
        [JsonPropertyName("showBox")]
        public bool ShowBox { get; set; } = true;

        /// <summary>
        /// Show player names.
        /// </summary>
        [JsonPropertyName("showNames")]
        public bool ShowNames { get; set; } = true;

        /// <summary>
        /// Show health bars.
        /// </summary>
        [JsonPropertyName("showHealth")]
        public bool ShowHealth { get; set; } = true;

        /// <summary>
        /// Show distance.
        /// </summary>
        [JsonPropertyName("showDistance")]
        public bool ShowDistance { get; set; } = true;

        /// <summary>
        /// Show aim lines.
        /// </summary>
        [JsonPropertyName("showAimlines")]
        public bool ShowAimlines { get; set; } = false;

        /// <summary>
        /// Show weapon names.
        /// </summary>
        [JsonPropertyName("showWeapon")]
        public bool ShowWeapon { get; set; } = false;

        /// <summary>
        /// Show loot ESP.
        /// </summary>
        [JsonPropertyName("showLoot")]
        public bool ShowLoot { get; set; } = false;

        /// <summary>
        /// Show exfil ESP.
        /// </summary>
        [JsonPropertyName("showExfils")]
        public bool ShowExfils { get; set; } = true;

        #endregion

        #region Colors

        /// <summary>
        /// Local player color (hex).
        /// </summary>
        [JsonPropertyName("localPlayerColor")]
        public string LocalPlayerColor { get; set; } = "#00FF00"; // Green

        /// <summary>
        /// Teammate color (hex).
        /// </summary>
        [JsonPropertyName("teammateColor")]
        public string TeammateColor { get; set; } = "#0000FF"; // Blue

        /// <summary>
        /// PMC color (hex).
        /// </summary>
        [JsonPropertyName("pmcColor")]
        public string PMCColor { get; set; } = "#FF0000"; // Red

        /// <summary>
        /// Scav color (hex).
        /// </summary>
        [JsonPropertyName("scavColor")]
        public string ScavColor { get; set; } = "#FFA500"; // Orange

        /// <summary>
        /// Boss color (hex).
        /// </summary>
        [JsonPropertyName("bossColor")]
        public string BossColor { get; set; } = "#FF00FF"; // Magenta

        /// <summary>
        /// Rogue/Guard color (hex).
        /// </summary>
        [JsonPropertyName("rogueColor")]
        public string RogueColor { get; set; } = "#FFFF00"; // Yellow

        /// <summary>
        /// Cultist color (hex).
        /// </summary>
        [JsonPropertyName("cultistColor")]
        public string CultistColor { get; set; } = "#800080"; // Purple

        /// <summary>
        /// Dead player color (hex).
        /// </summary>
        [JsonPropertyName("deadColor")]
        public string DeadColor { get; set; } = "#808080"; // Gray

        #endregion

        #region Filter Options

        /// <summary>
        /// Show local player in ESP.
        /// </summary>
        [JsonPropertyName("showLocalPlayer")]
        public bool ShowLocalPlayer { get; set; } = false;

        /// <summary>
        /// Show teammates in ESP.
        /// </summary>
        [JsonPropertyName("showTeammates")]
        public bool ShowTeammates { get; set; } = true;

        /// <summary>
        /// Show PMCs in ESP.
        /// </summary>
        [JsonPropertyName("showPMCs")]
        public bool ShowPMCs { get; set; } = true;

        /// <summary>
        /// Show Scavs in ESP.
        /// </summary>
        [JsonPropertyName("showScavs")]
        public bool ShowScavs { get; set; } = true;

        /// <summary>
        /// Show Bosses in ESP.
        /// </summary>
        [JsonPropertyName("showBosses")]
        public bool ShowBosses { get; set; } = true;

        /// <summary>
        /// Show dead players.
        /// </summary>
        [JsonPropertyName("showDead")]
        public bool ShowDead { get; set; } = false;

        #endregion

        /// <summary>
        /// Parse color from hex string to SKColor.
        /// </summary>
        public static SKColor ParseColor(string hexColor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hexColor))
                    return SKColors.White;

                hexColor = hexColor.TrimStart('#');
                
                if (hexColor.Length == 6)
                {
                    byte r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    byte g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    byte b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    return new SKColor(r, g, b);
                }
                else if (hexColor.Length == 8)
                {
                    byte a = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    byte r = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    byte g = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    byte b = Convert.ToByte(hexColor.Substring(6, 2), 16);
                    return new SKColor(r, g, b, a);
                }
            }
            catch
            {
                // Fallback to white on error
            }

            return SKColors.White;
        }
    }
}

