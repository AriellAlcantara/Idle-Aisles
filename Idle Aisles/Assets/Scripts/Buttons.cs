using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
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

    [Header("Main Menu")]
    [Tooltip("Menu panel GameObject (main menu)")]
    public GameObject menuPanel;

    [Tooltip("Play button on the main menu")]
    public Button playButton;

    [Tooltip("Exit/Quit button on the main menu")]
    public Button exitButton;

    [Tooltip("Root Game GameObject to enable when entering play (should be the 'Game' GameObject)")]
    public GameObject gameRoot;

    [Header("Pause UI")]
    [Tooltip("Pause panel to show when the game is paused")]
    public GameObject pausePanel;

    [Tooltip("Unpause button inside pause panel")]
    public Button pauseUnpauseButton;

    [Tooltip("Quit session button inside pause panel - returns to main menu and restarts progress")]
    public Button pauseQuitSessionButton;

    [Tooltip("Quit game button inside pause panel - exits application")]
    public Button pauseQuitGameButton;

    private void Awake()
    {
        // Basic registrations
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

        // Register main menu buttons if assigned
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }

        // Register pause panel buttons if assigned
        if (pauseUnpauseButton != null)
        {
            pauseUnpauseButton.onClick.AddListener(TogglePause);
        }

        if (pauseQuitSessionButton != null)
        {
            pauseQuitSessionButton.onClick.AddListener(QuitSession);
        }

        if (pauseQuitGameButton != null)
        {
            pauseQuitGameButton.onClick.AddListener(ExitGame);
        }

        // Fallback: try to auto-find Play / Exit buttons inside menuPanel by visible text if fields not assigned
        if (menuPanel != null)
        {
            // Only attempt if playButton or exitButton are null
            var buttons = menuPanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (playButton == null)
                {
                    // check UI Text
                    var uiText = b.GetComponentInChildren<Text>();
                    if (uiText != null && uiText.text.Trim().Equals("Play", System.StringComparison.OrdinalIgnoreCase))
                    {
                        playButton = b;
                        playButton.onClick.AddListener(PlayGame);
                    }
                }

#if UNITY_TEXTMESHPRO_PRESENT
                if (playButton == null)
                {
                    var tmp = b.GetComponentInChildren<TMPro.TMP_Text>();
                    if (tmp != null && tmp.text.Trim().Equals("Play", System.StringComparison.OrdinalIgnoreCase))
                    {
                        playButton = b;
                        playButton.onClick.AddListener(PlayGame);
                    }
                }
#endif

                if (exitButton == null)
                {
                    var uiText2 = b.GetComponentInChildren<Text>();
                    if (uiText2 != null && uiText2.text.Trim().Equals("Exit", System.StringComparison.OrdinalIgnoreCase))
                    {
                        exitButton = b;
                        exitButton.onClick.AddListener(ExitGame);
                    }
                }

#if UNITY_TEXTMESHPRO_PRESENT
                if (exitButton == null)
                {
                    var tmp2 = b.GetComponentInChildren<TMPro.TMP_Text>();
                    if (tmp2 != null && tmp2.text.Trim().Equals("Exit", System.StringComparison.OrdinalIgnoreCase))
                    {
                        exitButton = b;
                        exitButton.onClick.AddListener(ExitGame);
                    }
                }
#endif

                if (playButton != null && exitButton != null)
                    break;
            }
        }

        // Do not force gameRoot or gamePanel inactive here; respect editor scene setup

        // Ensure menu visible at start (if assigned)
        if (menuPanel != null)
            menuPanel.SetActive(true);

        // Initialize label to reflect current timescale
        UpdatePauseLabel(Time.timeScale <= 0f ? "Unpause" : "Pause");

        // Ensure panels initial state: trader hidden
        if (traderPanel != null)
            traderPanel.SetActive(false);

        // Ensure pause panel hidden initially
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // Debugging helpers
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogWarning("No EventSystem found in scene - UI buttons won't receive clicks.");
        }

        if (playButton == null)
        {
            Debug.LogWarning("Play button reference not assigned and could not be auto-found. Assign it in the Inspector or ensure a child button in menuPanel has text 'Play'.");
        }

        if (exitButton == null)
        {
            Debug.LogWarning("Exit button reference not assigned and could not be auto-found. Assign it in the Inspector or ensure a child button in menuPanel has text 'Exit'.");
        }
    }

    // Keep Start empty for future use or to ensure interaction ordering
    private void Start()
    {
    }

    public void TogglePause()
    {
        if (Time.timeScale > 0f)
        {
            // Pause: deactivate the pause button first, then show pause UI and stop time
            if (pauseButton != null)
                pauseButton.gameObject.SetActive(false);

            if (pausePanel != null)
                pausePanel.SetActive(true);

            Time.timeScale = 0f;
            UpdatePauseLabel("Unpause");
        }
        else
        {
            // Unpause: hide pause UI, re-enable pause button, resume time
            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (pauseButton != null)
                pauseButton.gameObject.SetActive(true);

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

    public void PlayGame()
    {
        Debug.Log("PlayGame invoked");

        // Enable game root first (try to find it if not assigned)
        if (gameRoot == null)
        {
            // Try common name
            var found = GameObject.Find("Game");
            if (found != null)
                gameRoot = found;
            else
            {
                // Try tag 'Game' or 'GameRoot'
                try {
                    found = GameObject.FindWithTag("Game");
                } catch { found = null; }
                if (found == null)
                {
                    try { found = GameObject.FindWithTag("GameRoot"); } catch { found = null; }
                }
                if (found != null)
                    gameRoot = found;
            }
        }

        if (gameRoot != null)
        {
            gameRoot.SetActive(true);
        }
        else
        {
            Debug.LogWarning("PlayGame: gameRoot is not assigned and could not be found. Assign the Game root in the Buttons component.");
        }

        // Ensure game UI panel is active if assigned
        if (gamePanel != null)
            gamePanel.SetActive(true);

        // Additional: enable common-named Game UI objects outside gameRoot
        string[] commonNames = new string[] { "Game Screen", "GameScreen", "Game UI", "GameUI", "Game_Screen", "GameCanvas", "Game Screen Canvas" };
        foreach (var n in commonNames)
        {
            var go = GameObject.Find(n);
            if (go != null)
            {
                Debug.Log($"PlayGame: enabling GameObject found by name '{n}'");
                go.SetActive(true);
            }
        }

        // Also enable any Canvas in the scene whose name contains 'game' (case-insensitive)
        var allCanvases = Object.FindObjectsOfType<Canvas>(true);
        foreach (var c in allCanvases)
        {
            if (c == null || c.gameObject == null) continue;
            var nm = c.gameObject.name.ToLowerInvariant();
            if (nm.Contains("game") || nm.Contains("gameui") || nm.Contains("game_screen"))
            {
                Debug.Log($"PlayGame: enabling Canvas '{c.gameObject.name}'");
                c.gameObject.SetActive(true);
                c.enabled = true;
                // enable any CanvasGroup
                var cg = c.gameObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
            }
        }

        // Hide menu after enabling game to avoid disabling this component prematurely
        if (menuPanel != null)
            menuPanel.SetActive(false);

        // Ensure timeScale resumed
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        // Detailed diagnostics: log active states and canvases under gameRoot
        if (gameRoot != null)
        {
            Debug.Log($"PlayGame diagnostics: gameRoot.name={gameRoot.name}, activeSelf={gameRoot.activeSelf}, activeInHierarchy={gameRoot.activeInHierarchy}, layer={LayerMask.LayerToName(gameRoot.layer)}");

            var canvases = gameRoot.GetComponentsInChildren<Canvas>(true);
            Debug.Log($"Found {canvases.Length} Canvas components under gameRoot");
            foreach (var c in canvases)
            {
                Debug.Log($"Canvas: name={c.gameObject.name}, enabled={c.enabled}, overrideSorting={c.overrideSorting}, sortingOrder={c.sortingOrder}, renderMode={c.renderMode}, worldCamera={(c.worldCamera!=null?c.worldCamera.name:"null")} ");
            }

            // List top-level children active state
            foreach (Transform child in gameRoot.transform)
            {
                Debug.Log($"Child: name={child.name}, activeSelf={child.gameObject.activeSelf}, activeInHierarchy={child.gameObject.activeInHierarchy}");
            }
        }
    }

    public void QuitSession()
    {
        Debug.Log("QuitSession invoked: clearing progress and returning to main menu");
        // Ensure timeScale is normal before switching
        Time.timeScale = 1f;
        // Clear saved progress (PlayerPrefs) to restart progress - adjust if your game stores progress elsewhere
        PlayerPrefs.DeleteAll();

        // Hide game UI
        if (gamePanel != null)
            gamePanel.SetActive(false);
        if (gameRoot != null)
            gameRoot.SetActive(false);

        // Hide pause UI and re-enable pause button
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        // Show main menu panel
        if (menuPanel != null)
            menuPanel.SetActive(true);

        // Optionally, you might want to reset other runtime state here
    }

    public void ExitGame()
    {
        Debug.Log("ExitGame invoked");
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // Ensure timeScale restored when this object is destroyed in editor/play
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }

    private void OnValidate()
    {
        // Keep inspector tidy: don't let menuPanel reference itself as gameRoot accidentally
        if (menuPanel != null && gameRoot != null && menuPanel == gameRoot)
        {
            Debug.LogWarning("menuPanel and gameRoot are the same GameObject. This will cause immediate disable when Play is pressed.");
        }
    }
}
