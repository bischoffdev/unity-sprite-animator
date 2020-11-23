using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace blog.softwaretester.spriteanimator
{
    public class SpriteAnimator : MonoBehaviour
    {
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
        private List<SpriteGroup> spriteGroup;

        [SerializeField]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "<Pending>")]
        private Image targetImage;

        public enum AnimationMode
        {
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

        public Image Image => targetImage;

        public void GotoAndStop(int spriteIndex)
        {
            GotoAndStop(activeSpriteGroup.groupId, spriteIndex, 0);
        }

        public void GotoAndStop(string groupId, int spriteIndex)
        {
            GotoAndStop(groupId, spriteIndex, 0);
        }

        public void GotoAndStop(string groupId, int spriteIndex, float delay)
        {
            Play(groupId, AnimationMode.ONCE, delay, spriteIndex, spriteIndex);
        }

        public void GotoAndPlay(int spriteIndex)
        {
            GotoAndPlay(activeSpriteGroup.groupId, spriteIndex, 0);
        }

        public void GotoAndPlay(string groupId, int spriteIndex)
        {
            GotoAndPlay(groupId, spriteIndex, 0);
        }

        public void GotoAndPlay(string groupId, int spriteIndex, float delay)
        {
            Play(groupId, AnimationMode.ONCE, delay, spriteIndex, -1);
        }

        public void Play(string groupId, AnimationMode mode)
        {
            Play(groupId, mode, 0);
        }

        public void Play(string groupId, AnimationMode mode, float delay)
        {
            Play(groupId, mode, delay, 0, -1);
        }

        public void SetActiveSpriteGroup(string groupId)
        {
            if (groupId == null && activeSpriteGroup == null) activeSpriteGroup = spriteGroup[0];
            else activeSpriteGroup = GetAnimationSpriteGroupByGroupId(groupId);
        }

        public void Play(string groupId, AnimationMode mode, float delay, int startSpriteIndex, int endSpriteIndex)
        {
            animationDelay = delay;
            SetActiveSpriteGroup(groupId);

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
                throw new Exception("Invalid start sprite index " + startSpriteIndex + " for group " + groupId + ".");
            }
            if (endSpriteIndex < 0 || endSpriteIndex > activeSpriteGroup.sprites.Count - 1 || endSpriteIndex < startSpriteIndex)
            {
                throw new Exception("Invalid end sprite index " + endSpriteIndex + " for group " + groupId + ".");
            }

            startIndex = startSpriteIndex;
            endIndex = endSpriteIndex;

            activeSpriteIndex = isReverse ? endIndex : startIndex;
            animationMode = mode;

            passedTime = 0;
            secondsPerSprite = activeSpriteGroup.spritesPerSecond > 0 ? 1 / activeSpriteGroup.spritesPerSecond : 1 / 25f;

            targetImage.sprite = activeSpriteGroup.sprites[activeSpriteIndex];
            CheckTriggers();

            isPlaying = true;
        }

        public void Pause()
        {
            isPlaying = false;
        }

        public void Resume()
        {
            isPlaying = true;
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
            passedTime += Time.deltaTime;
            if (animationDelay > 0)
            {
                if (passedTime >= animationDelay)
                {
                    animationDelay = 0;
                    passedTime = 0;
                }
                return;
            }

            if (passedTime >= secondsPerSprite)
            {
                passedTime = 0;

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

        public int GetSpriteCountInGroup(string groupId)
        {
            return GetAnimationSpriteGroupByGroupId(groupId).sprites.Count;
        }

        private SpriteGroup GetAnimationSpriteGroupByGroupId(string groupId)
        {
            if (spriteGroup.Count == 0)
            {
                throw new Exception("You need at least one sprite group to use SpriteAnimator.");
            }

            foreach (SpriteGroup animationSprite in spriteGroup)
            {
                if (animationSprite.groupId == groupId)
                {
                    if (animationSprite.sprites.Count == 0)
                    {
                        throw new Exception("No sprites in group: " + groupId);
                    }
                    return animationSprite;
                }
            }
            throw new Exception("No sprite group: " + groupId);
        }
    }
}
