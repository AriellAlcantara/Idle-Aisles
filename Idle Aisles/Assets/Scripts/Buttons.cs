using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

public class Buttons : MonoBehaviour
{
    [Tooltip("Button used for pausing/unpausing the game")]
    public Button pauseButton;

    [Tooltip("UI Text component for pause button (use this if using Unity UI Text)")]
    public Text pauseButtonText;

    [Tooltip("TMP Text component for pause button (use this if using TextMeshPro)")]
#if UNITY_TEXTMESHPRO_PRESENT
    public TMPro.TMP_Text pauseButtonTMPText;
#else
    public UnityEngine.Object pauseButtonTMPText; // kept to avoid compile errors if TMP not present
#endif

    [Header("Trader UI")] 
    [Tooltip("Button that opens the Trader panel")]
    public Button traderButton;

    [Tooltip("Trader panel GameObject to activate/deactivate")]
    public GameObject traderPanel;

    [Tooltip("Game panel GameObject to activate/deactivate when showing trader panel")]
    public GameObject gamePanel;

    [Tooltip("Button inside trader panel that closes it and returns to the game panel")]
    public Button traderBackButton;

    private void Start()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }

        if (traderButton != null)
        {
            traderButton.onClick.AddListener(OpenTraderPanel);
        }

        if (traderBackButton != null)
        {
            traderBackButton.onClick.AddListener(CloseTraderPanel);
        }

        // Initialize label to reflect current timescale
        UpdatePauseLabel(Time.timeScale <= 0f ? "Unpause" : "Pause");

        // Ensure panels initial state: trader hidden, game shown (if assigned)
        if (traderPanel != null)
            traderPanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0f;
            UpdatePauseLabel("Unpause");
        }
        else
        {
            Time.timeScale = 1f;
            UpdatePauseLabel("Pause");
        }
    }

    private void UpdatePauseLabel(string text)
    {
        if (pauseButtonText != null)
        {
            pauseButtonText.text = text;
            return;
        }

#if UNITY_TEXTMESHPRO_PRESENT
        if (pauseButtonTMPText != null)
        {
            pauseButtonTMPText.text = text;
            return;
        }
#endif

        // Fallback: if button has a child Text component automatically, try to find it
        if (pauseButton != null)
        {
            var uiText = pauseButton.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                uiText.text = text;
                return;
            }

#if UNITY_TEXTMESHPRO_PRESENT
            var tmp = pauseButton.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null)
            {
                tmp.text = text;
                return;
            }
#endif
        }
    }

    public void OpenTraderPanel()
    {
        if (traderPanel != null)
            traderPanel.SetActive(true);

        if (gamePanel != null)
            gamePanel.SetActive(false);
    }

    public void CloseTraderPanel()
    {
        if (traderPanel != null)
            traderPanel.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(true);
    }

    private void OnDestroy()
    {
        // Ensure timeScale restored when this object is destroyed in editor/play
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
