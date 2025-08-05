using UnityEngine;
using Eepy;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool isGamePaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            GameplayUI.OnGameplayUIClosed += Unpause;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;

            GameplayUI.OnGameplayUIClosed -= Unpause;
        }
    }

    private void Update()
    {
        if (!isGamePaused)
        {
            if (InputManager.GetKeyDown(InputAction.Pause))
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (!isGamePaused)
        {
            isGamePaused = true;
            Time.timeScale = 0f;
            GameplayUI.OpenMenu(GameplayUI.pauseMenu);
        }
    }

    public void Unpause()
    {
        if (isGamePaused)
        {
            isGamePaused = false;
            Time.timeScale = 1f;
            GameplayUI.CloseGameplayUI();
        }
    }
}
