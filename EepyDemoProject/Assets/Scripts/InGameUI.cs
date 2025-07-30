using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eepy;
using System.Linq;

public class InGameUI : MonoBehaviour
{
    [SerializeField] 
    private CanvasGroup pauseCanvasGroup;
    [SerializeField]
    private Text controlsText;
    [SerializeField]
    private float pauseFadeDuration = 0.1f;

    private bool isGamePaused = false;

    private void Update()
    {
        if (isGamePaused != GameManager.Instance.isGamePaused)
        {
            isGamePaused = GameManager.Instance.isGamePaused;

            StopAllCoroutines();
            StartCoroutine(Util.FadeCanvasGroup(pauseCanvasGroup, isGamePaused ? 1f : 0f, isGamePaused ? 0f : 1f, pauseFadeDuration));
        }

        if (!isGamePaused)
        {
            List<InputManager.InputState> inputStates = InputManager.GetInputStates();
            var orderedInputStates = inputStates.OrderBy(state => state.startTime);

            string text = "";
            foreach (InputManager.InputState inputState in orderedInputStates)
            {
                float holdTime = InputManager.GetKeyHoldTime(inputState.action);
                if (holdTime > 0f)
                {
                    text += $"{holdTime:F3}s - {inputState.action}\n";
                }
            }

            controlsText.SetText(text);
        }
    }
}
