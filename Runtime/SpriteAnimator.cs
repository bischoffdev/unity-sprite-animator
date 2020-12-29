using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace blog.softwaretester.spriteanimator
{
    public class SpriteAnimator : MonoBehaviour
    {
        /// <summary>
        /// Attach a function to OnTrigger in order to react to animation triggers that are defined in the UI.
        /// <code>OnTrigger += MyCustomTriggerHandler;</code>
        /// </summary>
        public event Action<int, string> OnTrigger;

        [Serializable]
        private struct Trigger
        {
            public int imageIndex;
            public string triggerName;
        }

        [Serializable]
        private struct SpriteGroup
        {
            public string groupId;
            public List<Sprite> sprites;
            public float spritesPerSecond;
            public List<Trigger> triggers;
        }

        [SerializeField]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
        private List<SpriteGroup> spriteGroups;

        [SerializeField]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
        private Image targetImage;

        [SerializeField]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
        private AnimationMode autoPlay = AnimationMode.NONE;

        /// <summary>
        /// The different animation modes to use. If none is specified, ONCE is used by default.
        /// </summary>
        public enum AnimationMode
        {
            NONE,
            ONCE,
            ONCE_REVERSE,
            PING_PONG,
            PING_PONG_LOOP,
            LOOP,
            LOOP_REVERSE
        }

        private SpriteGroup activeSpriteGroup;
        private int activeSpriteIndex;
        private bool isPlaying;
        private bool isReverse;
        private AnimationMode animationMode;
        private float secondsPerSprite;
        private float passedTime;
        private float animationDelay;
        private int startIndex;
        private int endIndex;

        /// <summary>
        /// This returns the Image element that this SpriteAnimator is attached to.
        /// </summary>
        public Image Image => targetImage;

        /// <summary>
        /// Sets the sprite group to the first one in the list and autoplays if applicable.
        /// </summary>
        private void Start()
        {
            activeSpriteGroup = spriteGroups[0];
            if (autoPlay != AnimationMode.NONE)
            {
                Play(autoPlay);
            }
        }

        /// <summary>
        /// Sets the active sprite group to play animations in.
        /// </summary>
        /// <param name="groupId">The group ID to switch to.</param>
        public void SetSpriteGroup(string groupId)
        {
            activeSpriteGroup = GetSpriteGroupByGroupId(groupId);
        }

        /// <summary>
        /// Switches to and stops at the sprite with the specified index.
        /// </summary>
        /// <param name="spriteIndex">The index of the sprite in the current group.</param>
        public void GotoAndStop(int spriteIndex)
        {
            GotoAndStop(spriteIndex, 0);
        }

        /// <summary>
        /// Switches to and stops at the sprite with the specified index after a delay.
        /// </summary>
        /// <param name="spriteIndex">The index of the sprite in the current group.</param>
        /// <param name="delay">A delay in seconds to wait before the switch.</param>
        public void GotoAndStop(int spriteIndex, float delay)
        {
            Play(AnimationMode.ONCE, delay, spriteIndex, spriteIndex);
        }

        /// <summary>
        /// Plays from the sprite with the specified index.
        /// </summary>
        /// <param name="spriteIndex">The index of the sprite in the current group.</param>
        public void GotoAndPlay(int spriteIndex)
        {
            GotoAndPlay(spriteIndex, 0);

        }

        /// <summary>
        /// Plays from the sprite with the specified index after a delay.
        /// </summary>
        /// <param name="spriteIndex">The index of the sprite in the current group.</param>
        /// <param name="delay">A delay in seconds to wait before the switch.</param>
        public void GotoAndPlay(int spriteIndex, float delay)
        {
            Play(AnimationMode.ONCE, delay, spriteIndex, -1);
        }

        /// <summary>
        /// Plays from the sprite with the specified index using a specified animation mode.
        /// </summary>
        /// <param name="mode">The animation mode to use.</param>
        public void Play(AnimationMode mode)
        {
            Play(mode, 0);
        }

        /// <summary>
        /// Plays from the sprite with the specified index using a specified animation mode after a delay.
        /// </summary>
        /// <param name="mode">The animation mode to use.</param>
        /// <param name="delay">A delay in seconds to wait before the switch.</param>
        public void Play(AnimationMode mode, float delay)
        {
            Play(mode, delay, 0, -1);
        }

        /// <summary>
        /// Plays a section of sprites using a specified animation mode after a delay.
        /// </summary>
        /// <param name="mode">The animation mode to use.</param>
        /// <param name="delay">A delay in seconds to wait before the switch.</param>
        /// <param name="startSpriteIndex">The starting point (sprite index) of the section to play.</param>
        /// <param name="endSpriteIndex">The end point (sprite index) of the section to play.</param>
        public void Play(AnimationMode mode, float delay, int startSpriteIndex, int endSpriteIndex)
        {
            animationDelay = delay;

            switch (mode)
            {
                case AnimationMode.ONCE:
                case AnimationMode.LOOP:
                case AnimationMode.PING_PONG:
                    isReverse = false;
                    break;
                case AnimationMode.ONCE_REVERSE:
                case AnimationMode.LOOP_REVERSE:
                case AnimationMode.PING_PONG_LOOP:
                    isReverse = true;
                    break;
            }

            endSpriteIndex = endSpriteIndex > -1 ? endSpriteIndex : activeSpriteGroup.sprites.Count - 1;

            if (startSpriteIndex < 0 || startSpriteIndex > activeSpriteGroup.sprites.Count - 1)
            {
                throw new Exception("Invalid start sprite index " + startSpriteIndex + " for group " + activeSpriteGroup.groupId + ".");
            }
            if (endSpriteIndex < 0 || endSpriteIndex > activeSpriteGroup.sprites.Count - 1 || endSpriteIndex < startSpriteIndex)
            {
                throw new Exception("Invalid end sprite index " + endSpriteIndex + " for group " + activeSpriteGroup.groupId + ".");
            }

            startIndex = startSpriteIndex;
            endIndex = endSpriteIndex;

            activeSpriteIndex = isReverse ? endIndex : startIndex;
            animationMode = mode;

            passedTime = 0;
            secondsPerSprite = activeSpriteGroup.spritesPerSecond > 0 ? 1f / activeSpriteGroup.spritesPerSecond : 1f / 25f;

            targetImage.sprite = activeSpriteGroup.sprites[activeSpriteIndex];
            CheckTriggers();

            isPlaying = true;
        }

        /// <summary>
        /// Pause the current animation.
        /// </summary>
        public void Pause()
        {
            isPlaying = false;
        }

        /// <summary>
        /// Resume the current animation after a pause.
        /// </summary>
        public void Resume()
        {
            isPlaying = true;
        }

        /// <summary>
        /// Return the number of sprites in a certain group.
        /// </summary>
        /// <param name="groupId">The group ID.</param>
        /// <returns></returns>
        public int GetSpriteCountInGroup(string groupId)
        {
            return GetSpriteGroupByGroupId(groupId).sprites.Count;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        private void Update()
        {
            if (isPlaying)
            {
                targetImage.sprite = activeSpriteGroup.sprites[activeSpriteIndex];
                Animate();
            }
        }

        private void CheckTriggers()
        {
            foreach (Trigger trigger in activeSpriteGroup.triggers)
            {
                if (activeSpriteIndex == trigger.imageIndex)
                {
                    OnTrigger?.Invoke(activeSpriteIndex, trigger.triggerName);
                }
            }
        }

        private void Animate()
        {
            passedTime += Time.unscaledTime;
            if (animationDelay > 0)
            {
                if (passedTime >= animationDelay)
                {
                    passedTime -= animationDelay;
                }
                else
                {
                    return;
                }
            }

            if (passedTime >= secondsPerSprite)
            {
                passedTime -= secondsPerSprite;

                if (isReverse)
                {
                    activeSpriteIndex--;
                    if (activeSpriteIndex < startIndex)
                    {
                        switch (animationMode)
                        {
                            case AnimationMode.ONCE_REVERSE:
                            case AnimationMode.PING_PONG:
                                isPlaying = false;
                                return;
                            case AnimationMode.PING_PONG_LOOP:
                                isReverse = false;
                                activeSpriteIndex = startIndex;
                                break;
                            case AnimationMode.LOOP_REVERSE:
                                activeSpriteIndex = endIndex;
                                break;
                        }
                    }
                }
                else
                {
                    activeSpriteIndex++;
                    if (activeSpriteIndex > endIndex)
                    {
                        switch (animationMode)
                        {
                            case AnimationMode.ONCE:
                                isPlaying = false;
                                return;
                            case AnimationMode.PING_PONG:
                            case AnimationMode.PING_PONG_LOOP:
                                isReverse = true;
                                activeSpriteIndex = endIndex;
                                break;
                            case AnimationMode.LOOP:
                                activeSpriteIndex = startIndex;
                                return;
                        }
                    }
                }
                targetImage.sprite = activeSpriteGroup.sprites[activeSpriteIndex];
                CheckTriggers();
            }
        }

        private SpriteGroup GetSpriteGroupByGroupId(string groupId)
        {
            if (spriteGroups.Count == 0)
            {
                throw new Exception("No sprite groups exist. Please specify at least one!");
            }

            foreach (SpriteGroup spriteGroup in spriteGroups)
            {
                if (spriteGroup.groupId == groupId)
                {
                    return spriteGroup;
                }
            }
            throw new Exception("No sprite group with id '" + groupId + "' exists. Please choose one of " + string.Join(", ", GetSpriteGroupIds()));
        }

        private List<string> GetSpriteGroupIds()
        {
            return (from SpriteGroup spriteGroup in spriteGroups select spriteGroup.groupId).ToList();
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
