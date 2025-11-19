/*
 * Lone EFT DMA Radar - ESP Extension
 */

using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using LoneEftDmaRadar.DMA;

namespace LoneEftDmaRadar.Tarkov.Unity
{
    /// <summary>
    /// Manages camera calculations for ESP WorldToScreen projections.
    /// </summary>
    public class CameraManager
    {
        private const int VIEWPORT_TOLERANCE = 800;
        private static readonly object _viewportSync = new();

        // Static properties for ESP
        public static Rectangle Viewport { get; private set; }
        public static SKPoint ViewportCenter => new SKPoint(Viewport.Width / 2f, Viewport.Height / 2f);
        public static bool IsScoped { get; private set; }
        public static bool IsADS { get; private set; }

        private static float _fov;
        private static float _aspect;
        private static readonly ViewMatrix _viewMatrix = new();

        public ulong FPSCamera { get; private set; }
        public ulong OpticCamera { get; private set; }

        /// <summary>
        /// Update the Viewport Dimensions for Camera Calculations.
        /// </summary>
        public static void UpdateViewportRes(int width, int height)
        {
            lock (_viewportSync)
            {
                Viewport = new Rectangle(0, 0, width, height);
            }
        }

        /// <summary>
        /// Update camera data for ESP rendering.
        /// </summary>
        public void UpdateCamera(Matrix4x4 matrix, float fov, float aspect, bool isScoped, bool isADS)
        {
            _viewMatrix.Update(ref matrix);
            _fov = fov;
            _aspect = aspect;
            IsScoped = isScoped;
            IsADS = isADS;
        }

        /// <summary>
        /// Translates 3D World Positions to 2D Screen Positions.
        /// </summary>
        /// <param name="worldPos">Entity's world position.</param>
        /// <param name="scrPos">Entity's screen position.</param>
        /// <param name="onScreenCheck">Check if the screen positions are 'on screen'. Returns false if off screen.</param>
        /// <param name="useTolerance">Use tolerance for on-screen check.</param>
        /// <returns>True if successful, otherwise False.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WorldToScreen(ref Vector3 worldPos, out SKPoint scrPos, bool onScreenCheck = false, bool useTolerance = false)
        {
            float w = Vector3.Dot(_viewMatrix.Translation, worldPos) + _viewMatrix.M44; // Transposed

            if (w < 0.098f)
            {
                scrPos = default;
                return false;
            }

            float x = Vector3.Dot(_viewMatrix.Right, worldPos) + _viewMatrix.M14; // Transposed
            float y = Vector3.Dot(_viewMatrix.Up, worldPos) + _viewMatrix.M24; // Transposed

            if (IsScoped)
            {
                float angleRadHalf = (MathF.PI / 180f) * _fov * 0.5f;
                float angleCtg = MathF.Cos(angleRadHalf) / MathF.Sin(angleRadHalf);

                x /= angleCtg * _aspect * 0.5f;
                y /= angleCtg * 0.5f;
            }

            var center = ViewportCenter;
            scrPos = new()
            {
                X = center.X * (1f + x / w),
                Y = center.Y * (1f - y / w)
            };

            if (onScreenCheck)
            {
                int left = useTolerance ? Viewport.Left - VIEWPORT_TOLERANCE : Viewport.Left;
                int right = useTolerance ? Viewport.Right + VIEWPORT_TOLERANCE : Viewport.Right;
                int top = useTolerance ? Viewport.Top - VIEWPORT_TOLERANCE : Viewport.Top;
                int bottom = useTolerance ? Viewport.Bottom + VIEWPORT_TOLERANCE : Viewport.Bottom;

                // Check if the screen position is within the screen boundaries
                if (scrPos.X < left || scrPos.X > right ||
                    scrPos.Y < top || scrPos.Y > bottom)
                {
                    scrPos = default;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the FOV Magnitude (Length) between a point, and the center of the screen.
        /// </summary>
        /// <param name="point">Screen point to calculate FOV Magnitude of.</param>
        /// <returns>Screen distance from the middle of the screen (FOV Magnitude).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetFovMagnitude(SKPoint point)
        {
            var center = ViewportCenter;
            float dx = point.X - center.X;
            float dy = point.Y - center.Y;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
    }
}

