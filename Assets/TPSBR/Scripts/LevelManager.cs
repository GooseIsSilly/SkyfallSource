using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("UI Settings")]
    public string nextSceneName = "NextLevel";
    public Color backgroundColor = new Color(0.2f, 0.5f, 0.9f, 1f);
    public Color buttonColor = new Color(1f, 0.9f, 0.2f, 1f);

    [Header("Title Logo")]
    [Tooltip("If true, displays a logo image instead of text")]
    public bool useLogoImage = false;
    [Tooltip("Logo sprite to display when useLogoImage is true")]
    public Sprite logoImage;
    [Tooltip("Preserve the logo image aspect ratio")]
    public bool preserveLogoAspect = true;

    [Header("Title Text (used when useLogoImage is false)")]
    public string gameTitle = "SKYFALL";

    [Header("Display Mode")]
    [Tooltip("Use 3D character models instead of 2D images")]
    public bool use3DCharacters = false;

    [Header("2D Character Settings (Images)")]
    public Sprite characterImage;
    public Sprite secondCharacterImage;
    public bool showSecondCharacter = false;
    public bool preserveAspect = true;

    [Header("3D Character Settings (Models)")]
    [Tooltip("Character model prefab to spawn (e.g., MenuSoldier)")]
    public GameObject characterModel;
    [Tooltip("Second character model prefab (optional)")]
    public GameObject secondCharacterModel;
    [Tooltip("Animation to play on the character")]
    public string animationStateName = "Idle";
    [Tooltip("Position for the 3D character")]
    public Vector3 characterPosition = new Vector3(2, -1, 3);
    [Tooltip("Rotation for the 3D character")]
    public Vector3 characterRotation = new Vector3(0, 180, 0);
    [Tooltip("Scale for the 3D character")]
    public float characterScale = 1f;
    [Tooltip("Position for the second 3D character")]
    public Vector3 secondCharacterPosition = new Vector3(-2, -1, 4);
    [Tooltip("Rotation for the second 3D character")]
    public Vector3 secondCharacterRotation = new Vector3(0, 180, 0);

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    public bool loopMusic = true;

    [Header("UI Elements")]
    public TMP_FontAsset customFont;
    public Sprite buttonBackground;
    private Canvas gameCanvas;
    private GameObject backgroundPanel;
    private TextMeshProUGUI titleText;
    private GameObject logoPanel;
    private GameObject buttonPanel;
    private TextMeshProUGUI instructionText;
    private GameObject characterPanel;
    private GameObject secondCharacterPanel;
    private AudioSource audioSource;
    
    // 3D character instances
    private GameObject spawnedCharacter;
    private GameObject spawnedSecondCharacter;
    private Animator characterAnimator;
    private Animator secondCharacterAnimator;

    [Header("Input")]
    private InputAction transitionAction;

    private float pulseTime = 0f;

    void Start()
    {
        // If running as dedicated server, skip startup UI and go straight to Loader
        if (Application.isBatchMode)
        {
            SceneManager.LoadScene("Loader");
            return;
        }

        SetupInput();
        SetupAudio();
        CreateGameUI();
    }

    void Update()
    {
        AnimateButton();
    }

    void SetupInput()
    {
        transitionAction = new InputAction("Transition", InputActionType.Button, "<Keyboard>/space");
        transitionAction.performed += OnTransitionPressed;
        transitionAction.Enable();
    }

    void SetupAudio()
    {
        if (backgroundMusic != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = backgroundMusic;
            audioSource.volume = musicVolume;
            audioSource.loop = loopMusic;
            audioSource.playOnAwake = false;
            audioSource.Play();
        }
    }

    void OnTransitionPressed(InputAction.CallbackContext context)
    {
        LoadNextLevel();
    }

    void CreateGameUI()
    {
        CreateCanvas();
        CreateBackground();
        
        if (use3DCharacters)
        {
            // Spawn 3D character models
            Spawn3DCharacters();
        }
        else
        {
            // Use 2D images
            CreateSecondCharacterImage();
            CreateCharacterImage();
        }
        
        CreateTitleText();
        CreateButtonPanel();
        CreateInstructionText();
    }

    void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("GameCanvas");
        gameCanvas = canvasObject.AddComponent<Canvas>();
        gameCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        gameCanvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
    }

    void CreateBackground()
    {
        GameObject bgObject = new GameObject("Background");
        bgObject.transform.SetParent(gameCanvas.transform, false);
        backgroundPanel = bgObject;

        Image bgImage = bgObject.AddComponent<Image>();
        bgImage.color = backgroundColor;

        RectTransform bgRect = bgObject.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
    }

    void CreateTitleText()
    {
        if (useLogoImage && logoImage != null)
        {
            CreateLogoImage();
        }
        else
        {
            CreateTitleTextElement();
        }
    }

    /// <summary>Creates a sprite image as the title logo.</summary>
    void CreateLogoImage()
    {
        GameObject logoObject = new GameObject("LogoImage");
        logoObject.transform.SetParent(gameCanvas.transform, false);
        logoPanel = logoObject;

        Image img = logoObject.AddComponent<Image>();
        img.sprite = logoImage;
        img.preserveAspect = preserveLogoAspect;

        RectTransform logoRect = logoObject.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.05f, 0.55f);
        logoRect.anchorMax = new Vector2(0.55f, 0.85f);
        logoRect.offsetMin = Vector2.zero;
        logoRect.offsetMax = Vector2.zero;
    }

    /// <summary>Creates a TextMeshPro element as the title.</summary>
    void CreateTitleTextElement()
    {
        GameObject titleObject = new GameObject("TitleText");
        titleObject.transform.SetParent(gameCanvas.transform, false);

        titleText = titleObject.AddComponent<TextMeshProUGUI>();
        if (customFont != null) titleText.font = customFont;
        titleText.text = gameTitle;
        titleText.fontSize = 240;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;

        Outline outline = titleObject.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.3f);
        outline.effectDistance = new Vector2(4, -4);

        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.1f, 0.5f);
        titleRect.anchorMax = new Vector2(0.6f, 0.8f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }

    void CreateButtonPanel()
    {
        GameObject panelObject = new GameObject("ButtonPanel");
        panelObject.transform.SetParent(gameCanvas.transform, false);
        buttonPanel = panelObject;

        Image panelImage = panelObject.AddComponent<Image>();
        if (buttonBackground != null)
        {
            panelImage.sprite = buttonBackground;
            panelImage.type = Image.Type.Sliced;
        }
        else
        {
            panelImage.color = new Color(0.85f, 0.1f, 0.1f, 1f);
        }

        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.25f);
        panelRect.anchorMax = new Vector2(0.45f, 0.35f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Shadow shadow = panelObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(0, -5);
    }

    void CreateInstructionText()
    {
        GameObject textObject = new GameObject("InstructionText");
        textObject.transform.SetParent(buttonPanel.transform, false);

        instructionText = textObject.AddComponent<TextMeshProUGUI>();
        if (customFont != null) instructionText.font = customFont;
        instructionText.text = "PRESS SPACE TO START";
        instructionText.fontSize = 42;
        instructionText.fontStyle = FontStyles.Bold;
        instructionText.color = Color.black;
        instructionText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    void AnimateButton()
    {
        if (buttonPanel == null) return;

        pulseTime += Time.deltaTime * 2f;
        float scale = 1f + Mathf.Sin(pulseTime) * 0.05f;
        buttonPanel.transform.localScale = new Vector3(scale, scale, 1f);
    }

    void CreateCharacterImage()
    {
        if (characterImage == null) return;

        GameObject charObject = new GameObject("CharacterImage");
        charObject.transform.SetParent(gameCanvas.transform, false);
        characterPanel = charObject;

        Image charImage = charObject.AddComponent<Image>();
        charImage.sprite = characterImage;
        charImage.preserveAspect = preserveAspect;

        RectTransform charRect = charObject.GetComponent<RectTransform>();
        charRect.anchorMin = new Vector2(0.6f, 0f);
        charRect.anchorMax = new Vector2(1f, 1f);
        charRect.offsetMin = Vector2.zero;
        charRect.offsetMax = Vector2.zero;

        charObject.transform.SetAsLastSibling();
    }

    void CreateSecondCharacterImage()
    {
        if (!showSecondCharacter || secondCharacterImage == null) return;

        GameObject charObject = new GameObject("SecondCharacterImage");
        charObject.transform.SetParent(gameCanvas.transform, false);
        secondCharacterPanel = charObject;

        Image charImage = charObject.AddComponent<Image>();
        charImage.sprite = secondCharacterImage;
        charImage.preserveAspect = preserveAspect;

        RectTransform charRect = charObject.GetComponent<RectTransform>();
        charRect.anchorMin = new Vector2(0.5f, 0f);
        charRect.anchorMax = new Vector2(0.9f, 1f);
        charRect.offsetMin = Vector2.zero;
        charRect.offsetMax = Vector2.zero;

        charObject.transform.SetSiblingIndex(1);
    }

    public void LoadNextLevel()
    {
        if (Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning($"Scene '{nextSceneName}' cannot be loaded. Make sure it's added to Build Settings.");

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.Log("No more levels available. Restarting from first scene.");
                SceneManager.LoadScene(0);
            }
        }
    }

    public void SetNextSceneName(string sceneName)
    {
        nextSceneName = sceneName;
        if (instructionText != null)
        {
            instructionText.text = "Press Space To Play";
        }
    }

    public void SetGameTitle(string title)
    {
        gameTitle = title;
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public void SetBackgroundMusic(AudioClip clip)
    {
        backgroundMusic = clip;
        if (audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (audioSource != null)
        {
            audioSource.volume = musicVolume;
        }
    }

    public void SetCharacterImage(Sprite sprite)
    {
        characterImage = sprite;
        if (characterPanel != null)
        {
            Image img = characterPanel.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
            }
        }
    }

    public void SetSecondCharacterImage(Sprite sprite)
    {
        secondCharacterImage = sprite;
        if (secondCharacterPanel != null)
        {
            Image img = secondCharacterPanel.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = sprite;
            }
        }
    }

    void Spawn3DCharacters()
    {
        // Spawn main character
        if (characterModel != null)
        {
            spawnedCharacter = Instantiate(characterModel);
            spawnedCharacter.name = "StartupCharacter";
            spawnedCharacter.transform.position = characterPosition;
            spawnedCharacter.transform.rotation = Quaternion.Euler(characterRotation);
            spawnedCharacter.transform.localScale = Vector3.one * characterScale;
            
            characterAnimator = spawnedCharacter.GetComponent<Animator>();
            if (characterAnimator == null)
            {
                characterAnimator = spawnedCharacter.GetComponentInChildren<Animator>();
            }
            
            if (characterAnimator != null && !string.IsNullOrEmpty(animationStateName))
            {
                PlayAnimation(characterAnimator, animationStateName);
            }
            
            Debug.Log($"[LevelManager] Spawned 3D character: {characterModel.name}");
        }
        
        // Spawn second character if enabled
        if (showSecondCharacter && secondCharacterModel != null)
        {
            spawnedSecondCharacter = Instantiate(secondCharacterModel);
            spawnedSecondCharacter.name = "StartupCharacter2";
            spawnedSecondCharacter.transform.position = secondCharacterPosition;
            spawnedSecondCharacter.transform.rotation = Quaternion.Euler(secondCharacterRotation);
            spawnedSecondCharacter.transform.localScale = Vector3.one * characterScale;
            
            secondCharacterAnimator = spawnedSecondCharacter.GetComponent<Animator>();
            if (secondCharacterAnimator == null)
            {
                secondCharacterAnimator = spawnedSecondCharacter.GetComponentInChildren<Animator>();
            }
            
            if (secondCharacterAnimator != null && !string.IsNullOrEmpty(animationStateName))
            {
                PlayAnimation(secondCharacterAnimator, animationStateName);
            }
            
            Debug.Log($"[LevelManager] Spawned second 3D character: {secondCharacterModel.name}");
        }
    }
    
    void PlayAnimation(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;
        
        // Try to play as a state
        if (HasAnimationState(animator, stateName))
        {
            animator.Play(stateName, 0, 0f);
            Debug.Log($"[LevelManager] Playing animation state: {stateName}");
        }
        else
        {
            // Try as a trigger
            animator.SetTrigger(stateName);
            Debug.Log($"[LevelManager] Setting animation trigger: {stateName}");
        }
    }
    
    bool HasAnimationState(Animator animator, string stateName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return false;
        
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == stateName)
                return true;
        }
        
        return false;
    }

    void OnDestroy()
    {
        if (transitionAction != null)
        {
            transitionAction.performed -= OnTransitionPressed;
            transitionAction.Disable();
            transitionAction.Dispose();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            Destroy(audioSource);
        }

        if (gameCanvas != null)
        {
            Destroy(gameCanvas.gameObject);
        }
        
        if (logoPanel != null)
        {
            Destroy(logoPanel);
        }
        
        // Clean up 3D characters
        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
        }
        
        if (spawnedSecondCharacter != null)
        {
            Destroy(spawnedSecondCharacter);
        }
    }
}
