using UnityEngine;
using blog.softwaretester.spriteanimator;

public class Example : MonoBehaviour
{
    private SpriteAnimator spriteAnimator;
    private enum SpriteGroup { cube, thing };

    [SerializeField]
    private SpriteAnimator.AnimationMode animationMode;

    private bool isPaused;
    private SpriteGroup currentGroup;
    private TMPro.TMP_Text modeLabel;
    private TMPro.TMP_Text triggerLabel;

    private void Start()
    {
        TMPro.TMP_Text[] textFields = GetComponentsInChildren<TMPro.TMP_Text>();
        modeLabel = textFields[0];
        triggerLabel = textFields[1];
        triggerLabel.text = "";

        modeLabel.text = animationMode.ToString();

        spriteAnimator = GetComponentInChildren<SpriteAnimator>();
        spriteAnimator.OnTrigger += OnTrigger;
        currentGroup = SpriteGroup.thing;
        spriteAnimator.SetSpriteGroup(currentGroup.ToString());
        spriteAnimator.Play(animationMode);

        Buttons.OnPress += OnPress;
    }

    private void OnTrigger(int frame, string triggerName)
    {
        triggerLabel.text = triggerName;
    }

    private void OnPress(Buttons.ButtonAction action)
    {
        switch (action)
        {
            case Buttons.ButtonAction.RESET:
                spriteAnimator.Play(animationMode);
                isPaused = false;
                break;
            case Buttons.ButtonAction.PAUSE:
                isPaused = !isPaused;
                if (isPaused)
                {
                    spriteAnimator.Pause();
                }
                else
                {
                    spriteAnimator.Resume();
                }
                break;
            case Buttons.ButtonAction.SWITCH:
                currentGroup = currentGroup == SpriteGroup.cube ? SpriteGroup.thing : SpriteGroup.cube;
                spriteAnimator.SetSpriteGroup(currentGroup.ToString());
                spriteAnimator.Play(animationMode);
                triggerLabel.text = "";
                isPaused = false;
                break;
        }
    }
}
