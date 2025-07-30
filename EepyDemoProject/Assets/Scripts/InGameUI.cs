using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eepy;

public class InGameUI : MonoBehaviour
{
    [SerializeField] 
    private CanvasGroup pauseCanvasGroup;
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
    }
}
