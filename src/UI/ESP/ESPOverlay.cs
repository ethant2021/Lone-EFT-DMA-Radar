/*
 * Lone EFT DMA Radar - ESP Extension
 */

using LoneEftDmaRadar.DMA;
using LoneEftDmaRadar.Misc;
using LoneEftDmaRadar.Tarkov.GameWorld;
using LoneEftDmaRadar.Tarkov.GameWorld.Player;
using LoneEftDmaRadar.Tarkov.GameWorld.Player.Helpers;
using LoneEftDmaRadar.Tarkov.Unity;
using LoneEftDmaRadar.UI.Skia;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;

namespace LoneEftDmaRadar.UI.ESP
{
    /// <summary>
    /// ESP Overlay Renderer - Integrated into Radar SkiaSharp canvas.
    /// </summary>
    public sealed class ESPOverlay
    {
        private readonly ESPConfig _config;
        private readonly Dictionary<PlayerType, SKPaint> _playerPaints = new();
        private readonly SKPaint _textPaint;
        private readonly SKPaint _healthBarPaint;
        private readonly SKPaint _healthBgPaint;
        private readonly Stopwatch _fpsWatch = Stopwatch.StartNew();
        private int _frameCount;
        private int _fps;

        public bool IsEnabled => _config.Enabled;

        public ESPOverlay(ESPConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializePaints();

            _textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 12f * _config.FontScale,
                IsAntialias = true,
                Typeface = SKFonts.TextFont
            };

            _healthBarPaint = new SKPaint
            {
                Color = SKColors.Green,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            _healthBgPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 128),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private void InitializePaints()
        {
            _playerPaints[PlayerType.LocalPlayer] = CreatePaint(ESPConfig.ParseColor(_config.LocalPlayerColor));
            _playerPaints[PlayerType.Teammate] = CreatePaint(ESPConfig.ParseColor(_config.TeammateColor));
            _playerPaints[PlayerType.PMC] = CreatePaint(ESPConfig.ParseColor(_config.PMCColor));
            _playerPaints[PlayerType.PlayerScav] = CreatePaint(ESPConfig.ParseColor(_config.ScavColor));
            _playerPaints[PlayerType.Scav] = CreatePaint(ESPConfig.ParseColor(_config.ScavColor));
            _playerPaints[PlayerType.Boss] = CreatePaint(ESPConfig.ParseColor(_config.BossColor));
            _playerPaints[PlayerType.BossGuard] = CreatePaint(ESPConfig.ParseColor(_config.BossColor));
            _playerPaints[PlayerType.Raider] = CreatePaint(ESPConfig.ParseColor(_config.RogueColor));
            _playerPaints[PlayerType.Rogue] = CreatePaint(ESPConfig.ParseColor(_config.RogueColor));
            _playerPaints[PlayerType.Cultist] = CreatePaint(ESPConfig.ParseColor(_config.CultistColor));
        }

        private SKPaint CreatePaint(SKColor color)
        {
            return new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = _config.LineWidth,
                IsAntialias = true
            };
        }

        /// <summary>
        /// Render ESP on the Skia canvas.
        /// </summary>
        public void Render(SKCanvas canvas, LocalGameWorld game)
        {
            if (!_config.Enabled || game == null)
                return;

            try
            {
                // Update FPS counter
                _frameCount++;
                if (_fpsWatch.ElapsedMilliseconds >= 1000)
                {
                    _fps = _frameCount;
                    _frameCount = 0;
                    _fpsWatch.Restart();
                }

                // Render players
                if (_config.ShowPlayers)
                {
                    RenderPlayers(canvas, game);
                }

                // Render FPS counter
                canvas.DrawText($"ESP FPS: {_fps}", 10, 30, _textPaint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ESP] Render error: {ex.Message}");
            }
        }

        private void RenderPlayers(SKCanvas canvas, LocalGameWorld game)
        {
            try
            {
                var players = game.RegisteredPlayers;
                if (players == null || players.Count == 0)
                    return;

                var localPlayer = game.MainPlayer as LocalPlayer;
                if (localPlayer == null)
                    return;

                var localPos = localPlayer.Position3D;

                foreach (var kvp in players)
                {
                    try
                    {
                        var player = kvp.Value;
                        if (player == null || !player.IsActive)
                            continue;

                        // Check if we should show this player type
                        if (!ShouldShowPlayer(player))
                            continue;

                        // Check distance
                        var distance = Vector3.Distance(localPos, player.Position3D);
                        if (distance > _config.MaxDistance)
                            continue;

                        // Skip if dead and not showing dead
                        if (!player.IsAlive && !_config.ShowDead)
                            continue;

                        // Get player screen position
                        var worldPos = player.Position3D;
                        if (!CameraManager.WorldToScreen(ref worldPos, out var screenPos, true))
                            continue;

                        // Get paint for this player type
                        var paint = GetPlayerPaint(player);

                        // Render skeleton
                        if (_config.ShowSkeleton && player.Skeleton != null)
                        {
                            RenderSkeleton(canvas, player, paint);
                        }

                        // Render box
                        if (_config.ShowBox && player.Skeleton != null)
                        {
                            RenderBox(canvas, player, screenPos, paint);
                        }

                        // Render info text
                        RenderPlayerInfo(canvas, player, screenPos, distance);

                        // Render health bar
                        if (_config.ShowHealth)
                        {
                            RenderHealthBar(canvas, player, screenPos);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[ESP] Error rendering player: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ESP] Error in RenderPlayers: {ex.Message}");
            }
        }

        private bool ShouldShowPlayer(AbstractPlayer player)
        {
            return player.Type switch
            {
                PlayerType.LocalPlayer => _config.ShowLocalPlayer,
                PlayerType.Teammate => _config.ShowTeammates,
                PlayerType.PMC => _config.ShowPMCs,
                PlayerType.PlayerScav => _config.ShowScavs,
                PlayerType.Scav => _config.ShowScavs,
                PlayerType.Boss => _config.ShowBosses,
                PlayerType.BossGuard => _config.ShowBosses,
                PlayerType.Raider => _config.ShowScavs,
                PlayerType.Rogue => _config.ShowScavs,
                PlayerType.Cultist => _config.ShowBosses,
                _ => true
            };
        }

        private SKPaint GetPlayerPaint(AbstractPlayer player)
        {
            if (!player.IsAlive)
                return CreatePaint(ESPConfig.ParseColor(_config.DeadColor));

            if (_playerPaints.TryGetValue(player.Type, out var paint))
                return paint;

            return _playerPaints[PlayerType.PMC]; // Default
        }

        private void RenderSkeleton(SKCanvas canvas, AbstractPlayer player, SKPaint paint)
        {
            try
            {
                if (player.Skeleton == null)
                    return;

                // Update the skeleton buffer with screen coordinates
                if (!player.Skeleton.UpdateESPBuffer())
                    return;

                // Draw skeleton lines (buffer contains pairs of points)
                var buffer = Skeleton.ESPBuffer;
                for (int i = 0; i < buffer.Length - 1; i += 2)
                {
                    canvas.DrawLine(buffer[i], buffer[i + 1], paint);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ESP] Skeleton render error: {ex.Message}");
            }
        }

        private void RenderBox(SKCanvas canvas, AbstractPlayer player, SKPoint baseScreen, SKPaint paint)
        {
            try
            {
                var box = player.Skeleton?.GetESPBox(baseScreen);
                if (box.HasValue)
                {
                    canvas.DrawRect(box.Value, paint);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ESP] Box render error: {ex.Message}");
            }
        }

        private void RenderPlayerInfo(SKCanvas canvas, AbstractPlayer player, SKPoint screenPos, float distance)
        {
            try
            {
                var lines = new List<string>();

                if (_config.ShowNames)
                {
                    lines.Add(player.Name ?? "Unknown");
                }

                if (_config.ShowDistance)
                {
                    lines.Add($"{distance:F0}m");
                }

                if (_config.ShowWeapon && !string.IsNullOrEmpty(player.Weapon))
                {
                    lines.Add(player.Weapon);
                }

                if (lines.Count > 0)
                {
                    float yOffset = screenPos.Y - 20;
                    foreach (var line in lines)
                    {
                        // Draw text with black outline for visibility
                        var bounds = new SKRect();
                        _textPaint.MeasureText(line, ref bounds);
                        float xPos = screenPos.X - bounds.Width / 2;

                        // Draw outline
                        var outlinePaint = _textPaint.Clone();
                        outlinePaint.Color = SKColors.Black;
                        outlinePaint.Style = SKPaintStyle.Stroke;
                        outlinePaint.StrokeWidth = 2;
                        canvas.DrawText(line, xPos, yOffset, outlinePaint);

                        // Draw text
                        canvas.DrawText(line, xPos, yOffset, _textPaint);

                        yOffset -= 15 * _config.FontScale;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ESP] Info render error: {ex.Message}");
            }
        }

        private void RenderHealthBar(SKCanvas canvas, AbstractPlayer player, SKPoint screenPos)
        {
            try
            {
                const float barWidth = 50f;
                const float barHeight = 4f;
                float healthPercent = player.Health / player.MaxHealth;

                float x = screenPos.X - barWidth / 2;
                float y = screenPos.Y + 15;

                // Background
                var bgRect = new SKRect(x, y, x + barWidth, y + barHeight);
                canvas.DrawRect(bgRect, _healthBgPaint);

                // Health bar
                var healthRect = new SKRect(x, y, x + (barWidth * healthPercent), y + barHeight);
                
                // Color based on health
                var healthColor = healthPercent > 0.6f ? SKColors.Green :
                                 healthPercent > 0.3f ? SKColors.Yellow : SKColors.Red;
                _healthBarPaint.Color = healthColor;

                canvas.DrawRect(healthRect, _healthBarPaint);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ESP] Health bar render error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update paints when config changes.
        /// </summary>
        public void RefreshColors()
        {
            InitializePaints();
            _textPaint.TextSize = 12f * _config.FontScale;
        }
    }
}

