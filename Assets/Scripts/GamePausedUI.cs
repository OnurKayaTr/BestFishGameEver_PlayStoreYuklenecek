using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamePausedUI : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button pauseButton;  // Pause butonu
    [SerializeField] private Button toggleMusicButton;  // Arka plan müziði açma/kapama butonu
    [SerializeField] private Button toggleEffectsButton;  // Ses efektlerini açma/kapama butonu
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);  // Baþlangýçta kapalý
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }

        if (toggleMusicButton != null)
        {
            toggleMusicButton.onClick.AddListener(ToggleBackgroundMusic);
        }

        if (toggleEffectsButton != null)
        {
            toggleEffectsButton.onClick.AddListener(ToggleSoundEffects);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
        }
    }

    private void ToggleBackgroundMusic()
    {
        SoundManager.Instance.ToggleBackgroundMusic();
    }

    private void ToggleSoundEffects()
    {
        SoundManager.Instance.ToggleSoundEffects();
    }

    private void RetryGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
