using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuUiToolkitBridge : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenu";
    private const string Prefix = "HighScore: ";
    private const string Suffix = "m";

    [SerializeField] private string targetSceneName = "SampleScene";

    private Label highScoreLabel;
    private Button playButton;
    private bool delayedBindAttempted;
    private const string LogPrefix = "[MainMenuUiToolkitBridge]";

    private UIDocument document;

    private void Awake()
    {
        Scene scene = gameObject.scene;
        Debug.Log($"{LogPrefix} Awake on '{gameObject.name}', scene='{scene.name}', loaded={scene.isLoaded}.");
        if (!scene.IsValid() || scene.name != MainMenuSceneName)
        {
            Debug.LogWarning($"{LogPrefix} Disabled: not in MainMenu scene.", this);
            enabled = false;
            return;
        }

        document = GetComponent<UIDocument>();
        if (document == null)
        {
            Debug.LogError($"{LogPrefix} UIDocument is missing.", this);
            enabled = false;
            return;
        }

        Debug.Log($"{LogPrefix} UIDocument found. Waiting for Start() binding.", this);
    }

    private void Start()
    {
        // UIDocument visual tree can be populated after Awake, so bind on Start.
        Debug.Log($"{LogPrefix} Start -> Bind(rootVisualElement).", this);
        Bind(document.rootVisualElement);
        DisableLegacyCanvas();
    }

    private void Bind(VisualElement root)
    {
        if (root == null)
        {
            Debug.LogError($"{LogPrefix} Root visual element is null.", this);
            return;
        }

        Debug.Log($"{LogPrefix} Bind called. root childCount={root.childCount}.", this);
        highScoreLabel = root.Q<Label>("highscore-label");
        playButton = root.Q<Button>("play-button");
        Debug.Log($"{LogPrefix} Query results: highscore-label={(highScoreLabel != null)}, play-button={(playButton != null)}.", this);

        if (playButton == null && !delayedBindAttempted)
        {
            delayedBindAttempted = true;
            Debug.LogWarning($"{LogPrefix} play-button not found. Retrying bind in 100ms.", this);
            root.schedule.Execute(() => Bind(root)).ExecuteLater(100);
            return;
        }

        if (playButton != null)
        {
            playButton.clicked -= LoadGameScene;
            playButton.clicked += LoadGameScene;
            playButton.RegisterCallback<ClickEvent>(HandlePlayClickEvent);
            Debug.Log($"{LogPrefix} play-button callback wired.", this);
        }
        else
        {
            Debug.LogWarning($"{LogPrefix} play-button was not found in UXML.", this);
        }

        if (HighScoreService.HasSavedHighScore)
        {
            UpdateHighScoreText(HighScoreService.SavedHighScore);
        }
        else
        {
            UpdateUnsetHighScoreText();
        }
    }

    private void OnEnable()
    {
        Debug.Log($"{LogPrefix} OnEnable subscribe HighScoreUpdated.", this);
        GameSignals.HighScoreUpdated += HandleHighScoreUpdated;
    }

    private void OnDisable()
    {
        Debug.Log($"{LogPrefix} OnDisable unsubscribe HighScoreUpdated.", this);
        GameSignals.HighScoreUpdated -= HandleHighScoreUpdated;

        if (playButton != null)
        {
            playButton.clicked -= LoadGameScene;
            playButton.UnregisterCallback<ClickEvent>(HandlePlayClickEvent);
        }
    }

    private void HandleHighScoreUpdated(float score)
    {
        UpdateHighScoreText(score);
    }

    private void UpdateHighScoreText(float score)
    {
        if (highScoreLabel == null)
        {
            return;
        }

        highScoreLabel.text = $"{Prefix}{Mathf.RoundToInt(score)}{Suffix}";
    }

    private void UpdateUnsetHighScoreText()
    {
        if (highScoreLabel == null)
        {
            return;
        }

        highScoreLabel.text = $"{Prefix}-";
    }

    private void LoadGameScene()
    {
        Debug.Log($"{LogPrefix} LoadGameScene invoked. targetSceneName='{targetSceneName}'.", this);
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogError($"{LogPrefix} Target scene name is empty.", this);
            return;
        }

        Debug.Log($"{LogPrefix} Loading scene '{targetSceneName}'.", this);
        SceneManager.LoadScene(targetSceneName);
    }

    private void HandlePlayClickEvent(ClickEvent evt)
    {
        Debug.Log($"{LogPrefix} ClickEvent received on play-button. target={evt.target}.", this);
    }

    private void DisableLegacyCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            var canvas = canvases[i];
            if (canvas == null)
            {
                continue;
            }

            if (canvas.gameObject == gameObject)
            {
                continue;
            }

            canvas.gameObject.SetActive(false);
        }
    }
}
