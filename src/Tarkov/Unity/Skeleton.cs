/*
 * Lone EFT DMA Radar - ESP Extension
 */

using LoneEftDmaRadar.Tarkov.GameWorld.Player;
using LoneEftDmaRadar.Tarkov.Unity.Structures;
using System.Numerics;

namespace LoneEftDmaRadar.Tarkov.Unity
{
    /// <summary>
    /// Contains abstractions for drawing Player Skeletons.
    /// </summary>
    public sealed class Skeleton
    {
        private const int JOINTS_COUNT = 26;

        /// <summary>
        /// Bones Buffer for ESP rendering (26 points = 13 lines).
        /// </summary>
        public static readonly SKPoint[] ESPBuffer = new SKPoint[JOINTS_COUNT];

        private readonly Dictionary<Bones, UnityTransform> _bones;
        private readonly AbstractPlayer _player;

        /// <summary>
        /// Skeleton Root Transform.
        /// </summary>
        public UnityTransform Root { get; private set; }

        /// <summary>
        /// All Transforms for this Skeleton (including Root).
        /// </summary>
        public IReadOnlyDictionary<Bones, UnityTransform> Bones => _bones;

        public Skeleton(AbstractPlayer player)
        {
            _player = player;
            _bones = new Dictionary<Bones, UnityTransform>();
            InitializeBones();
        }

        private void InitializeBones()
        {
            try
            {
                // Read the root bone first
                var rootTransform = _player.Transform;
                if (rootTransform?.TransformInternal.IsValid() ?? false)
                {
                    Root = rootTransform;
                    _bones[Bones.HumanBase] = rootTransform;
                }

                // Initialize all required bones for skeleton
                foreach (var bone in GetRequiredBones())
                {
                    // This would need to be implemented based on how the player object stores bone transforms
                    // For now, we'll create placeholder logic
                    // In the actual implementation, you'd read the bone transform chain from memory
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Skeleton] Failed to initialize bones for {_player.Name}: {ex.Message}");
            }
        }

        private static IEnumerable<Bones> GetRequiredBones()
        {
            return new[]
            {
                Bones.HumanHead,
                Bones.HumanNeck,
                Bones.HumanSpine3,
                Bones.HumanSpine2,
                Bones.HumanSpine1,
                Bones.HumanPelvis,
                Bones.HumanLCollarbone,
                Bones.HumanRCollarbone,
                Bones.HumanLForearm2,
                Bones.HumanRForearm2,
                Bones.HumanLPalm,
                Bones.HumanRPalm,
                Bones.HumanLThigh2,
                Bones.HumanRThigh2,
                Bones.HumanLFoot,
                Bones.HumanRFoot
            };
        }

        /// <summary>
        /// Updates the static ESP Buffer with the current Skeleton Bone Screen Coordinates.
        /// NOT THREAD SAFE! Only call from ESP rendering thread.
        /// </summary>
        /// <returns>True if successful, otherwise False.</returns>
        public bool UpdateESPBuffer()
        {
            try
            {
                // Check if we have the required bones
                if (!_bones.TryGetValue(Bones.HumanHead, out var headBone) ||
                    !_bones.TryGetValue(Bones.HumanNeck, out var neckBone) ||
                    !_bones.TryGetValue(Bones.HumanSpine3, out var upperTorsoBone) ||
                    !_bones.TryGetValue(Bones.HumanSpine2, out var midTorsoBone) ||
                    !_bones.TryGetValue(Bones.HumanSpine1, out var lowerTorsoBone) ||
                    !_bones.TryGetValue(Bones.HumanPelvis, out var pelvisBone))
                    return false;

                // Update bone positions
                var headPos = headBone.Position;
                var neckPos = neckBone.Position;
                var upperTorsoPos = upperTorsoBone.Position;
                var midTorsoPos = midTorsoBone.Position;
                var lowerTorsoPos = lowerTorsoBone.Position;
                var pelvisPos = pelvisBone.Position;

                // Convert to screen coordinates
                if (!CameraManager.WorldToScreen(ref headPos, out var headScreen, true, true))
                    return false;
                if (!CameraManager.WorldToScreen(ref neckPos, out var neckScreen))
                    return false;
                if (!CameraManager.WorldToScreen(ref upperTorsoPos, out var upperTorsoScreen))
                    return false;
                if (!CameraManager.WorldToScreen(ref midTorsoPos, out var midTorsoScreen))
                    return false;
                if (!CameraManager.WorldToScreen(ref lowerTorsoPos, out var lowerTorsoScreen))
                    return false;
                if (!CameraManager.WorldToScreen(ref pelvisPos, out var pelvisScreen))
                    return false;

                // Get limb positions
                SKPoint leftCollarScreen = default, rightCollarScreen = default;
                SKPoint leftElbowScreen = default, rightElbowScreen = default;
                SKPoint leftHandScreen = default, rightHandScreen = default;
                SKPoint leftKneeScreen = default, rightKneeScreen = default;
                SKPoint leftFootScreen = default, rightFootScreen = default;

                if (_bones.TryGetValue(Bones.HumanLCollarbone, out var leftCollar))
                {
                    var pos = leftCollar.Position;
                    CameraManager.WorldToScreen(ref pos, out leftCollarScreen);
                }
                if (_bones.TryGetValue(Bones.HumanRCollarbone, out var rightCollar))
                {
                    var pos = rightCollar.Position;
                    CameraManager.WorldToScreen(ref pos, out rightCollarScreen);
                }
                if (_bones.TryGetValue(Bones.HumanLForearm2, out var leftElbow))
                {
                    var pos = leftElbow.Position;
                    CameraManager.WorldToScreen(ref pos, out leftElbowScreen);
                }
                if (_bones.TryGetValue(Bones.HumanRForearm2, out var rightElbow))
                {
                    var pos = rightElbow.Position;
                    CameraManager.WorldToScreen(ref pos, out rightElbowScreen);
                }
                if (_bones.TryGetValue(Bones.HumanLPalm, out var leftHand))
                {
                    var pos = leftHand.Position;
                    CameraManager.WorldToScreen(ref pos, out leftHandScreen);
                }
                if (_bones.TryGetValue(Bones.HumanRPalm, out var rightHand))
                {
                    var pos = rightHand.Position;
                    CameraManager.WorldToScreen(ref pos, out rightHandScreen);
                }
                if (_bones.TryGetValue(Bones.HumanLThigh2, out var leftKnee))
                {
                    var pos = leftKnee.Position;
                    CameraManager.WorldToScreen(ref pos, out leftKneeScreen);
                }
                if (_bones.TryGetValue(Bones.HumanRThigh2, out var rightKnee))
                {
                    var pos = rightKnee.Position;
                    CameraManager.WorldToScreen(ref pos, out rightKneeScreen);
                }
                if (_bones.TryGetValue(Bones.HumanLFoot, out var leftFoot))
                {
                    var pos = leftFoot.Position;
                    CameraManager.WorldToScreen(ref pos, out leftFootScreen);
                }
                if (_bones.TryGetValue(Bones.HumanRFoot, out var rightFoot))
                {
                    var pos = rightFoot.Position;
                    CameraManager.WorldToScreen(ref pos, out rightFootScreen);
                }

                // Fill the ESP buffer with skeleton lines (pairs of points)
                int index = 0;

                // Head to pelvis (torso)
                ESPBuffer[index++] = headScreen;
                ESPBuffer[index++] = neckScreen;
                ESPBuffer[index++] = neckScreen;
                ESPBuffer[index++] = upperTorsoScreen;
                ESPBuffer[index++] = upperTorsoScreen;
                ESPBuffer[index++] = midTorsoScreen;
                ESPBuffer[index++] = midTorsoScreen;
                ESPBuffer[index++] = lowerTorsoScreen;
                ESPBuffer[index++] = lowerTorsoScreen;
                ESPBuffer[index++] = pelvisScreen;

                // Left leg
                ESPBuffer[index++] = pelvisScreen;
                ESPBuffer[index++] = leftKneeScreen;
                ESPBuffer[index++] = leftKneeScreen;
                ESPBuffer[index++] = leftFootScreen;

                // Right leg
                ESPBuffer[index++] = pelvisScreen;
                ESPBuffer[index++] = rightKneeScreen;
                ESPBuffer[index++] = rightKneeScreen;
                ESPBuffer[index++] = rightFootScreen;

                // Left arm
                ESPBuffer[index++] = leftCollarScreen;
                ESPBuffer[index++] = leftElbowScreen;
                ESPBuffer[index++] = leftElbowScreen;
                ESPBuffer[index++] = leftHandScreen;

                // Right arm
                ESPBuffer[index++] = rightCollarScreen;
                ESPBuffer[index++] = rightElbowScreen;
                ESPBuffer[index++] = rightElbowScreen;
                ESPBuffer[index++] = rightHandScreen;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Skeleton] UpdateESPBuffer failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Return screen coordinates with W2S transformation applied for Box ESP.
        /// </summary>
        /// <param name="baseScreen">Screen Coords of Base Position.</param>
        /// <returns>Box ESP Screen Coordinates.</returns>
        public SKRect? GetESPBox(SKPoint baseScreen)
        {
            try
            {
                if (!_bones.TryGetValue(Bones.HumanHead, out var headBone))
                    return null;

                var headPos = headBone.Position;
                if (!CameraManager.WorldToScreen(ref headPos, out var topScreen, true, true))
                    return null;

                float height = Math.Abs(topScreen.Y - baseScreen.Y);
                float width = height / 2.05f;

                return new SKRect()
                {
                    Top = topScreen.Y,
                    Left = topScreen.X - width / 2,
                    Bottom = baseScreen.Y,
                    Right = topScreen.X + width / 2
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Update all bone positions from memory.
        /// </summary>
        public void UpdateBonePositions()
        {
            foreach (var bone in _bones.Values)
            {
                bone.UpdatePosition();
            }
        }
    }
}

