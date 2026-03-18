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
#if TMP_PRESENT
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

    [Tooltip("Shop button inside pause panel - opens the shop panel and hides pause menu")]
    public Button pauseShopButton;

    [Tooltip("Shop panel to enable when opening from pause")]
    public GameObject shopPanel;

    [Header("Shop - Ads UI")]
    [Tooltip("Button on the shop panel to watch ads")]
    public Button watchAdsButton;

    [Tooltip("Ads UI panel to display when watching ads")]
    public GameObject adsPanel;

    [Tooltip("Optional: Close button inside ads UI to return to shop")]
    public Button adsCloseButton;

    [Header("Shop - Item UI")]
    [Tooltip("Text component for the shop item label (UI Text)")]
    public Text shopItemText;

    [Tooltip("TextMeshPro component for the shop item label (TMP)")]
#if TMP_PRESENT
    public TMPro.TMP_Text shopItemTextTMP;
#else
    public UnityEngine.Object shopItemTextTMP;
#endif

    [Tooltip("Exit/Close button inside the shop to return to the pause menu")]
    public Button shopExitButton;

    [Tooltip("Text object shown before watching an ad (GameObject containing text)")]
    public GameObject shopItemBeforeText;

    [Tooltip("Text object shown after watching an ad (GameObject containing text)")]
    public GameObject shopItemAfterText;

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

        if (pauseShopButton != null)
        {
            pauseShopButton.onClick.AddListener(OpenShopFromPause);
        }

        // Register shop/ads buttons
        if (watchAdsButton != null)
        {
            watchAdsButton.onClick.AddListener(OpenAdsPanel);
        }

        if (adsCloseButton != null)
        {
            adsCloseButton.onClick.AddListener(CloseAdsPanel);
        }

        // Register shop exit
        if (shopExitButton != null)
        {
            shopExitButton.onClick.AddListener(CloseShopAndReturnToPause);
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

#if TMP_PRESENT
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

#if TMP_PRESENT
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

        // Ensure shop panel hidden initially
        if (shopPanel != null)
            shopPanel.SetActive(false);

        // Ensure ads panel hidden initially
        if (adsPanel != null)
            adsPanel.SetActive(false);

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

#if TMP_PRESENT
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

#if TMP_PRESENT
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
        Debug.Log("QuitSession invoked: clearing progress and restarting scene to reset game state");
        // Ensure timeScale is normal before switching
        Time.timeScale = 1f;

        // Clear saved progress
        PlayerPrefs.DeleteAll();

        // Reset runtime stateful managers/statics
        try { CoinManager.ResetAll(); } catch { }
        try { CounterBehaviour.GlobalCoinsBonus = 0; } catch { }

        // If you have other static managers, reset them here (example placeholders):
        // Example: TraderSystem.ResetStaticState();

        // Reload the active scene to fully restart the game state without quitting the application
        var active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.name, LoadSceneMode.Single);
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

    public void OpenShopFromPause()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void OpenAdsPanel()
    {
        if (adsPanel == null)
        {
            Debug.LogWarning("OpenAdsPanel: adsPanel is not assigned. Assign it in the Buttons inspector.");
            return;
        }

        // Activate ads panel and ensure any Canvas/CanvasGroup under it are enabled so it actually renders
        adsPanel.SetActive(true);
        var canvases = adsPanel.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            if (c != null && c.gameObject != null)
            {
                c.gameObject.SetActive(true);
                c.enabled = true;
                var cg = c.gameObject.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
            }
        }

        // Do not hide the shop panel — keep it visible beneath the ads UI
        Debug.Log("OpenAdsPanel: adsPanel activated (shop left visible)");
    }

    public void CloseAdsPanel()
    {
        if (adsPanel != null)
            adsPanel.SetActive(false);

        // Remove the watch-ads button to prevent spamming
        if (watchAdsButton != null)
        {
            try
            {
                watchAdsButton.gameObject.SetActive(false);
            }
            catch { /* ignore if already destroyed */ }
        }

        // Change shop item text to indicate coin earned increase (TMP/text update preserved)
        SetShopItemText("Coin Earned Increased");

        // Toggle before/after UI GameObjects if assigned
        if (shopItemBeforeText != null)
            shopItemBeforeText.SetActive(false);
        if (shopItemAfterText != null)
            shopItemAfterText.SetActive(true);

        // Increase coins-per-shopper directly on all counters by 1
        var counters = FindObjectsOfType<CounterBehaviour>();
        int increased = 0;
        foreach (var c in counters)
        {
            if (c != null)
            {
                c.coinsPerShopper += 1;
                increased++;
            }
        }
        Debug.Log($"CloseAdsPanel: increased coinsPerShopper by 1 on {increased} counters.");

        Debug.Log("CloseAdsPanel: adsPanel closed, watchAdsButton disabled, shop item updated");
    }

    private void SetShopItemText(string text)
    {
#if TMP_PRESENT
        if (shopItemTextTMP != null)
        {
            shopItemTextTMP.text = text;
            return;
        }
        // Fallback to legacy UI Text if TMP not available or not assigned
        if (shopItemText != null)
        {
            shopItemText.text = text;
            return;
        }
#else
        if (shopItemText != null)
        {
            shopItemText.text = text;
            return;
        }
#endif
        Debug.LogWarning("SetShopItemText: No valid text component found for shop item text. Assign a TMP or UI Text in the Buttons inspector.");
    }

    public void CloseShopAndReturnToPause()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
}
