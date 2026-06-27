using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class GamesChatBotItemId
{
    public const string HealthPotion = "health_potion";
    public const string EnergyPotion = "energy_potion";
    public const string SmallFood = "small_food";
    public const string BigFood = "big_food";
    public const string JumpBoost = "jump_boost";
    public const string SpeedBoost = "speed_boost";
    public const string CherryBoost = "cherry_boost";
    public const string TeddyBear = "teddy_bear";
    public const string Volleyball = "volleyball";
}

public static class GamesChatBotStats
{
    const string TotalSpentKey = "CHATBOT_TOTAL_APPLES_SPENT";
    const string TotalBoughtKey = "CHATBOT_TOTAL_ITEMS_BOUGHT";
    static string BoughtKey(string itemId) => "CHATBOT_BOUGHT_" + itemId;
    static string EventCountKey(string eventId) => "PIXIE_EVENT_COUNT_" + eventId;
    static string EventLastKey(string eventId) => "PIXIE_EVENT_LAST_" + eventId;
    static string EventLogKey(string eventId) => "PIXIE_EVENT_LOG_" + eventId;
    const int MaxLoggedEventTimes = 128;

    public static int TotalSpent => PlayerPrefs.GetInt(TotalSpentKey, 0);
    public static int TotalBought => PlayerPrefs.GetInt(TotalBoughtKey, 0);
    public static int GetBoughtCount(string itemId) => PlayerPrefs.GetInt(BoughtKey(itemId), 0);

    public static void RecordEvent(string eventId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(eventId) || amount <= 0)
            return;

        int now = NowUnix();
        PlayerPrefs.SetInt(EventCountKey(eventId), PlayerPrefs.GetInt(EventCountKey(eventId), 0) + amount);
        PlayerPrefs.SetInt(EventLastKey(eventId), now);

        List<int> entries = GetEventTimes(eventId);
        for (int i = 0; i < amount; i++)
            entries.Add(now);
        if (entries.Count > MaxLoggedEventTimes)
            entries.RemoveRange(0, entries.Count - MaxLoggedEventTimes);

        SaveEventTimes(eventId, entries);
        PlayerPrefs.Save();
    }

    public static int GetEventCount(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return 0;
        return PlayerPrefs.GetInt(EventCountKey(eventId), 0);
    }

    public static int GetLastEventUnix(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return 0;
        return PlayerPrefs.GetInt(EventLastKey(eventId), 0);
    }

    public static int CountEventsSince(string eventId, int sinceUnix)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return 0;

        List<int> entries = GetEventTimes(eventId);
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i] >= sinceUnix)
                count++;
        }
        return count;
    }

    static List<int> GetEventTimes(string eventId)
    {
        List<int> entries = new List<int>();
        string raw = PlayerPrefs.GetString(EventLogKey(eventId), string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return entries;

        string[] parts = raw.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out int value))
                entries.Add(value);
        }
        return entries;
    }

    static void SaveEventTimes(string eventId, List<int> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            PlayerPrefs.DeleteKey(EventLogKey(eventId));
            return;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0)
                sb.Append(',');
            sb.Append(entries[i]);
        }
        PlayerPrefs.SetString(EventLogKey(eventId), sb.ToString());
    }

    static int NowUnix()
    {
        long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (unix > int.MaxValue) return int.MaxValue;
        if (unix < int.MinValue) return int.MinValue;
        return (int)unix;
    }
}

[DisallowMultipleComponent]
public class GamesChatBot : MonoBehaviour
{
    const string PendingSystemWarningTextKey = "PIXIE_PENDING_SYSTEM_WARNING_TEXT";
    const string PendingSystemWarningColorKey = "PIXIE_PENDING_SYSTEM_WARNING_COLOR";
    const string PendingSystemWarningAutoOpenKey = "PIXIE_PENDING_SYSTEM_WARNING_AUTO_OPEN";

    [Serializable]
    class PlayerProfileData
    {
        public int selectedCharacter = -1;
        public string playerName = "";
    }

    const string PlayerNameKey = "PlayerName";
    const string MemoryKey = "PIXIE_AI_CHAT_MEMORY_V3";
    const string LastAdvicePrefix = "PIXIE_AI_LAST_ADVICE_";
    const string ActiveChoresKey = "PIXIE_AI_ACTIVE_CHORES_V2";
    const string ChoreBacklogKey = "PIXIE_AI_CHORE_BACKLOG_V2";
    const string ChoreCooldownUntilKey = "PIXIE_AI_CHORE_COOLDOWN_UNTIL_UTC";
    const string ChoreLastAssignedTaskKey = "PIXIE_AI_CHORE_LAST_ASSIGNED_TASK";
    const string ChoreLastAssignedUnixPrefix = "PIXIE_AI_CHORE_LAST_ASSIGNED_";
    const string LastBedUseKey = "PIXIE_AI_LAST_BED_USE_UTC";
    const string LastShowerUseKey = "PIXIE_AI_LAST_SHOWER_USE_UTC";
    const string HouseIntroSeenKey = "PIXIE_AI_HOUSE_INTRO_SEEN_V1";
    const string TutorialIntroSeenKey = "PIXIE_AI_TUTORIAL_INTRO_SEEN_V1";
    const string LongTermMemoryFolder = "PixieAI";
    const string LongTermMemoryFile = "games_chatbot_longterm_v1.json";
    const string JumpBoostUntilKey = "PLAYER_JUMP_BOOST_UNTIL_UTC";
    const string SpeedBoostUntilKey = "PLAYER_SPEED_BOOST_UNTIL_UTC";
    const string CherryBoostUntilKey = "PET_CHERRY_DECAY_BOOST_UNTIL_UTC";

    [Header("Ollama")]
    [SerializeField] string ollamaChatUrl = "http://localhost:11434/api/chat";
    [SerializeField] string model = "llama3.2:3b";
    [SerializeField] int maxHistoryMessages = 14;
    [TextArea(3, 8)] [SerializeField] string systemPrompt =
        "You are Pixie AI in a pet-care game. Be smart, specific, and practical. Avoid repeating yourself.";

    [Header("UI")]
    [SerializeField] KeyCode openKey = KeyCode.G;
    [SerializeField] KeyCode closeKey = KeyCode.Escape;
    [SerializeField] Rect windowRect = new Rect(20f, 20f, 560f, 460f);
    [SerializeField] string[] allowedScenes = { "House", "Hospital", "Tutorial" };
    [SerializeField] float chatWindowWidthPercent = 0.4f;
    [SerializeField] float chatWindowHeightPercent = 0.68f;
    [SerializeField] float chatWindowMinWidth = 520f;
    [SerializeField] float chatWindowMinHeight = 460f;
    [SerializeField] float chatWindowMaxWidth = 900f;
    [SerializeField] float chatWindowMaxHeight = 760f;
    [SerializeField] int chatFontSize = 20;
    [SerializeField] int statusFontSize = 15;
    [SerializeField] int inputFontSize = 17;
    [SerializeField] int choreTitleFontSize = 17;
    [SerializeField] int choreButtonFontSize = 15;
    [SerializeField] int choreTextFontSize = 18;
    [SerializeField] float inputMinHeight = 28f;
    [SerializeField] float inputMaxHeight = 120f;
    [SerializeField] float typingCharsPerSecond = 55f;
    [SerializeField] bool enableFirstHouseIntroPopup = true;
    [SerializeField] bool pauseGameDuringHouseIntro = true;
    [SerializeField] bool keepHouseIntroCentered = true;
    [SerializeField] Rect houseIntroWindowRect = new Rect(0f, 0f, 820f, 500f);
    [SerializeField] float introWindowWidthPercent = 0.58f;
    [SerializeField] float introWindowHeightPercent = 0.58f;
    [SerializeField] float introWindowMinWidth = 700f;
    [SerializeField] float introWindowMinHeight = 420f;
    [SerializeField] float introWindowMaxWidth = 1100f;
    [SerializeField] float introWindowMaxHeight = 720f;
    [SerializeField] float introTypingCharsPerSecond = 22f;
    [SerializeField] int houseIntroFontSize = 26;
    [TextArea(4, 8)] [SerializeField] string houseIntroParagraph =
        "House Tutorial\n\nWelcome Home {PET_NAME}!\n\nThis is your pet's safe place to rest and recover.\n\n• Bed restores Energy\n• Bath restores Hygiene\n• Open the Levels Menu (top-right) and select Pet Care to buy items like Kiwi, Pineapple, and Health Potions.\n\nIf your Energy drops below 50%, levels will remain locked until you rest.\n\nKeep an eye on your stats:\n• Low Hunger slows you down.\n• Low Hunger also causes Health to decrease faster.\n\nPress G to open Pixie AI and Esc to close it.\n\nThank you for playing and enjoy your adventure!";
    [TextArea(4, 12)] [SerializeField] string tutorialIntroParagraph =
        "Hello! I'm Pixie AI, and I'll help you, {PLAYER_NAME}, get started on your adventure in Pixel Care!\n\n🎮Controls:\n\nPress A to move left and D to move right.\nYou can also use the Left Arrow (←) and Right Arrow (→) keys.\nPress the Spacebar to jump.\nWant to reach higher places? Press the Spacebar twice quickly to perform a Double Jump!\n\nGold Coins are the main currency in Pixel Care.\n\n1 Gold Coin = $1\nUse Gold Coins to purchase:\n🍎 Food\n💊 Medicine\nOther helpful supplies\n\nGold Coins are rare and valuable. The best way to earn them is by completing challenging levels and overcoming difficult obstacles throughout the game.";

    [Header("Gameplay")]
    [SerializeField] bool persistConversationAcrossSessions = true;
    [SerializeField] bool forceGameOnlyResponses = true;
    [SerializeField] float lowNeedThreshold = 35f;
    [SerializeField] float alertCooldownSeconds = 20f;
    [SerializeField] float choreCooldownSeconds = 600f;
    [SerializeField] ShopButtons shopButtons;
    [SerializeField] PetNeeds petNeeds;

    [Header("Recommendation Tuning")]
    [SerializeField] string[] recommendationLevelNames = { "Level 1", "Level 2", "Level 3", "Level 4" };
    [SerializeField] int[] recommendationLevelAppleEstimates = { 3, 5, 8, 12 };

    [Header("Pixie Advisor")]
    [SerializeField] string pixieAdvisorUrl = "http://localhost:11434/api/generate";
    [SerializeField] bool useOllamaForAdvice = true;
    [SerializeField] int pixieAdvisorTimeoutSeconds = 45;
    [SerializeField] PixieDecisionConfig pixieDecisionConfig = new PixieDecisionConfig();

    readonly List<ChatLine> chatLines = new List<ChatLine>();
    readonly List<OllamaMessage> history = new List<OllamaMessage>();
    readonly List<ItemDef> itemDefs = new List<ItemDef>();
    readonly List<string> pendingAlerts = new List<string>();
    readonly List<ChoreEntry> activeChores = new List<ChoreEntry>();
    readonly List<LearnedFactData> learnedFacts = new List<LearnedFactData>();
    readonly List<string> pendingChoreIds = new List<string>();
    readonly List<string> carryoverChoreIds = new List<string>();

    Vector2 scrollPosition;
    string inputBuffer = string.Empty;
    bool isOpen;
    bool waitingForModel;
    bool autoScrollToBottom = true;
    int typingLineIndex = -1;
    float nextAlertAt;
    bool lowHungerPrev, lowEnergyPrev, lowHygienePrev, lowHappinessPrev, lowHealthPrev;
    bool energyGatePrev, criticalHungerPrev, criticalHealthPrev;
    bool needsStateInitialized;
    Coroutine requestRoutine;
    float nextReferenceRefreshAt;
    bool showHouseIntro;
    bool hasShownWelcomeGreetingThisScene;
    float preIntroTimeScale = 1f;
    Rect preIntroWindowRect;
    bool hasPreIntroWindowRect;
    bool windowLayoutInitialized;
    int lastScreenWidth;
    int lastScreenHeight;
    int introVisibleChars;
    float introTypingProgress;
    Vector2 introScrollPosition;
    string petPickedUtcIso = string.Empty;
    string activeIntroParagraph = string.Empty;
    string latestAdvisorStatus = "Advice unavailable.";
    PixieResponse latestAdvisorResponse;
    static int energyAbove80SinceUnix;
    static int happinessAbove80SinceUnix;
    static int perfectDaySinceUnix;

    GUIStyle chatLineStyle, inputAreaStyle, statusStyle, introParagraphStyle, choreTitleStyle, choreButtonStyle, choreLineStyle;

    struct ItemDef
    {
        public string id, display;
        public int cost;
        public string effect;
        public string[] aliases;
        public ItemDef(string id, string display, int cost, string effect, params string[] aliases)
        { this.id = id; this.display = display; this.cost = cost; this.effect = effect; this.aliases = aliases; }
    }

    class ChatLine
    {
        public string fullText;
        public int visibleChars;
        public float createdAt;
        public Color color = Color.white;
        public string VisibleText => string.IsNullOrEmpty(fullText) ? string.Empty : fullText.Substring(0, Mathf.Clamp(visibleChars, 0, fullText.Length));
    }

    class ChoreEntry { public string id; public string text; public int reward; public int assignedAtUnix; }
    class ChoreDefinition { public string id; public string text; public int reward; public ChoreDefinition(string id, string text, int reward) { this.id = id; this.text = text; this.reward = reward; } }
    [Serializable] class ActiveChoreData { public string id; public string text; public int reward; public int assignedAtUnix; }
    [Serializable] class ActiveChoreDataList { public List<ActiveChoreData> chores = new List<ActiveChoreData>(); }
    [Serializable] class ChoreBacklogData { public List<string> batchIds = new List<string>(); public List<string> carryoverIds = new List<string>(); }
    [Serializable] class LearnedFactData { public string key; public string value; public string savedAtUtcIso; }
    [Serializable] class LongTermMemoryData
    {
        public string petPickedUtcIso;
        public List<LearnedFactData> facts = new List<LearnedFactData>();
    }

    [Serializable] class OllamaMessage { public string role; public string content; }
    [Serializable] class OllamaOptions { public float temperature = 0.72f; public float top_p = 0.92f; }
    [Serializable] class OllamaChatRequest { public string model; public bool stream; public OllamaMessage[] messages; public OllamaOptions options; }
    [Serializable] class MemoryData { public List<OllamaMessage> history = new List<OllamaMessage>(); public List<string> transcript = new List<string>(); }
#pragma warning disable 0649
    [Serializable] class OllamaChatResponse { public string model; public OllamaMessage message; public bool done; }
#pragma warning restore 0649

    void Awake()
    {
        RefreshReferences();
        RebuildItemDefs();
        LoadChoreBacklog();
        LoadActiveChores();
        LoadMemory();
        LoadLongTermMemory();
        EnsurePetPickedTime();
        ConsumePendingSystemWarning();
        RefreshActiveChoresIfNeeded();
        SyncNeedStateImmediate();
        RefreshAdvisorStatus();
        TryStartHouseIntro();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (SaveData.IsHardResetInProgress)
            return;
        SaveMemory();
        SaveLongTermMemory();
    }

    void OnApplicationQuit()
    {
        if (SaveData.IsHardResetInProgress)
            return;
        SaveMemory();
        SaveLongTermMemory();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        isOpen = false;
        showHouseIntro = false;
        hasShownWelcomeGreetingThisScene = false;
        windowLayoutInitialized = false;
        lastScreenWidth = 0;
        lastScreenHeight = 0;
        waitingForModel = false;
        windowRect = BuildChatWindowRect();
        if (requestRoutine != null) StopCoroutine(requestRoutine);
        requestRoutine = null;
        typingLineIndex = -1;
        LoadChoreBacklog();
        LoadActiveChores();
        RefreshReferences();
        RebuildItemDefs();
        ConsumePendingSystemWarning();
        RefreshActiveChoresIfNeeded();
        SyncNeedStateImmediate();
        RefreshAdvisorStatus();
        TryStartHouseIntro();
    }

    void Update()
    {
        if (!IsSceneAllowed()) { isOpen = false; return; }
        if (Time.unscaledTime >= nextReferenceRefreshAt)
        {
            nextReferenceRefreshAt = Time.unscaledTime + 1f;
            RefreshReferences();
            RefreshActiveChoresIfNeeded();
            if (petNeeds != null && !needsStateInitialized) SyncNeedStateImmediate();
            RefreshAdvisorStatus();
        }
        UpdateChoreProgressTracking();
        if (!isOpen && WasOpenPressed()) OpenChat();
        if (isOpen && WasClosePressed()) CloseChat();
        if (isOpen && !waitingForModel && WasSubmitPressed()) SendFromInput();
        TickTyping();
        TickIntroTyping();
        CheckNeedAlerts();
        if (showHouseIntro && pauseGameDuringHouseIntro && Mathf.Abs(Time.timeScale) > 0.001f)
            Time.timeScale = 0f;
    }

    void TickTyping()
    {
        if (typingLineIndex < 0 || typingLineIndex >= chatLines.Count) return;
        ChatLine line = chatLines[typingLineIndex];
        if (line == null || string.IsNullOrEmpty(line.fullText)) { typingLineIndex = -1; return; }

        int add = Mathf.Max(1, Mathf.FloorToInt(Mathf.Max(1f, typingCharsPerSecond) * Time.unscaledDeltaTime));
        line.visibleChars = Mathf.Min(line.fullText.Length, line.visibleChars + add);
        autoScrollToBottom = true;
        if (line.visibleChars >= line.fullText.Length) typingLineIndex = -1;
    }

    void TickIntroTyping()
    {
        if (!showHouseIntro) return;
        string intro = activeIntroParagraph ?? string.Empty;
        if (introVisibleChars >= intro.Length) return;
        introTypingProgress += Mathf.Max(1f, introTypingCharsPerSecond) * Time.unscaledDeltaTime;
        introVisibleChars = Mathf.Clamp(Mathf.FloorToInt(introTypingProgress), 0, intro.Length);
    }

    void OpenChat()
    {
        if (isOpen) return;
        isOpen = true;
        if (showHouseIntro) return;
        windowRect = BuildChatWindowRect();
        windowLayoutInitialized = false;
        if (!hasShownWelcomeGreetingThisScene)
        {
            AppendLineTyped("Pixie AI: " + BuildWelcomeBackGreeting());
            hasShownWelcomeGreetingThisScene = true;
        }
        if (chatLines.Count == 0)
        {
            AppendLineImmediate("Pixie AI: Ask about purchases, savings goals, costs, bars, and chores.");
            AppendLineImmediate("Pixie AI: " + BuildNeedRecommendation());
        }
        for (int i = 0; i < pendingAlerts.Count; i++) AppendLineTyped("Pixie AI alert: " + pendingAlerts[i]);
        pendingAlerts.Clear();
        GUI.FocusControl("GamesChatBotInput");
    }

    string BuildWelcomeBackGreeting()
    {
        string playerName = GetStoredPlayerName();
        if (!string.IsNullOrEmpty(playerName))
        {
            PlayerPrefs.SetString(PlayerNameKey, playerName);
            PlayerPrefs.Save();
        }
        if (string.IsNullOrEmpty(playerName))
            return "Hi, welcome back!";
        return "Hi, welcome back " + playerName + "!";
    }

    string GetStoredPlayerName()
    {
        string playerName = PlayerPrefs.GetString(PlayerNameKey, string.Empty).Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = TryGetPlayerNameFromProfileFile();
        return playerName;
    }

    string TryGetPlayerNameFromProfileFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "player_profile_v1.json");
        if (!File.Exists(path)) return string.Empty;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;
            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);
            if (data == null || string.IsNullOrWhiteSpace(data.playerName))
                return string.Empty;
            return data.playerName.Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    void CloseChat()
    {
        isOpen = false;
        if (!showHouseIntro) return;
        showHouseIntro = false;
        introVisibleChars = 0;
        introTypingProgress = 0f;
        if (hasPreIntroWindowRect)
        {
            windowRect = preIntroWindowRect;
            hasPreIntroWindowRect = false;
        }
        if (pauseGameDuringHouseIntro) Time.timeScale = preIntroTimeScale;
    }

    void SendFromInput()
    {
        string userInput = (inputBuffer ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(userInput)) return;
        inputBuffer = string.Empty;
        GamesChatBotStats.RecordEvent("talk_ai");
        RefreshReferences();
        RebuildItemDefs();
        AppendLineImmediate("You: " + userInput);

        if (IsReportQuery(userInput))
        {
            if (requestRoutine != null) StopCoroutine(requestRoutine);
            requestRoutine = StartCoroutine(RequestPixelCareReportReply(userInput));
            return;
        }

        if (TryHandleSaveAndQuitCommand(userInput))
            return;

        if (TryHandleLearningCommand(userInput, out string learningReply))
        {
            AppendLineTyped("Pixie AI: " + learningReply);
            SaveMemory();
            return;
        }

        if (TryBuildPetAgeReply(userInput, out string petAgeReply))
        {
            history.Add(new OllamaMessage { role = "user", content = userInput });
            history.Add(new OllamaMessage { role = "assistant", content = petAgeReply });
            TrimHistory();
            AppendLineTyped("Pixie AI: " + petAgeReply);
            SaveMemory();
            return;
        }

        if (TryGetLearnedFactReply(userInput, out string learnedReply))
        {
            history.Add(new OllamaMessage { role = "user", content = userInput });
            history.Add(new OllamaMessage { role = "assistant", content = learnedReply });
            TrimHistory();
            AppendLineTyped("Pixie AI: " + learnedReply);
            SaveMemory();
            return;
        }

        if (TryBuildGroundTruthReply(userInput, out string strictReply))
        {
            history.Add(new OllamaMessage { role = "user", content = userInput });
            history.Add(new OllamaMessage { role = "assistant", content = strictReply });
            TrimHistory();
            AppendLineTyped("Pixie AI: " + strictReply);
            SaveMemory();
            return;
        }

        if (IsAdviceQuery(userInput))
        {
            if (requestRoutine != null) StopCoroutine(requestRoutine);
            requestRoutine = StartCoroutine(RequestPixieAdvisorReply(userInput));
            return;
        }

        if (requestRoutine != null) StopCoroutine(requestRoutine);
        requestRoutine = StartCoroutine(RequestOllamaReply(userInput));
    }

    bool TryHandleSaveAndQuitCommand(string userInput)
    {
        string cmd = (userInput ?? string.Empty).Trim();
        string compact = Regex.Replace(cmd.ToLowerInvariant(), "[^a-z/]", string.Empty);
        bool isSaveAndQuit =
            compact == "/saveandquit" ||
            compact == "saveandquit" ||
            compact == "/quit" ||
            compact == "quit";
        if (!isSaveAndQuit)
            return false;

        SaveMemory();
        SaveLongTermMemory();
        PlayerPrefs.Save();
        AppendLineTyped("Pixie AI: Progress saved. Quitting now.");
        StartCoroutine(QuitNextFrame());
        return true;
    }

    IEnumerator QuitNextFrame()
    {
        yield return null;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator RequestOllamaReply(string userInput)
    {
        waitingForModel = true;
        OllamaChatRequest body = new OllamaChatRequest
        {
            model = model,
            stream = false,
            messages = BuildOllamaPayload(userInput).ToArray(),
            options = new OllamaOptions()
        };

        using (UnityWebRequest req = new UnityWebRequest(ollamaChatUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 90;
            yield return req.SendWebRequest();

            string reply;
            if (req.result == UnityWebRequest.Result.Success)
            {
                OllamaChatResponse parsed = JsonUtility.FromJson<OllamaChatResponse>(req.downloadHandler.text);
                reply = parsed != null && parsed.message != null && !string.IsNullOrWhiteSpace(parsed.message.content)
                    ? parsed.message.content.Trim()
                    : BuildLocalResponse(userInput);
            }
            else
            {
                reply = "I cannot reach Ollama right now. " + BuildLocalResponse(userInput);
            }

            // Guard critical gameplay info against hallucinations.
            if (TryBuildGroundTruthReply(userInput, out string strictReply))
                reply = strictReply;
            else if (TryBuildPetAgeReply(userInput, out string petAgeReply))
                reply = petAgeReply;
            else if (TryGetLearnedFactReply(userInput, out string learnedReply))
                reply = learnedReply;
            else if (forceGameOnlyResponses && LooksLikeNonGameReply(reply))
                reply = BuildLocalResponse(userInput);

            reply = Warmify(reply);

            history.Add(new OllamaMessage { role = "user", content = userInput });
            history.Add(new OllamaMessage { role = "assistant", content = reply });
            TrimHistory();
            AppendLineTyped("Pixie AI: " + reply);
            SaveMemory();
        }

        waitingForModel = false;
        requestRoutine = null;
    }

    IEnumerator RequestPixelCareReportReply(string userInput)
    {
        waitingForModel = true;

        PixelCareReportData report = PixelCareReport.Build();
        string calculatedReport = report.ToDisplayText();
        string fallbackReply = BuildPixelCareReportFallbackReply(calculatedReport, report);

        OllamaChatRequest body = new OllamaChatRequest
        {
            model = model,
            stream = false,
            messages = BuildPixelCareReportPayload(userInput, calculatedReport).ToArray(),
            options = new OllamaOptions()
        };

        string reply = fallbackReply;
        using (UnityWebRequest req = new UnityWebRequest(ollamaChatUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 90;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                OllamaChatResponse parsed = JsonUtility.FromJson<OllamaChatResponse>(req.downloadHandler.text);
                if (parsed != null && parsed.message != null && !string.IsNullOrWhiteSpace(parsed.message.content))
                    reply = parsed.message.content.Trim();
            }
        }

        history.Add(new OllamaMessage { role = "user", content = userInput });
        history.Add(new OllamaMessage { role = "assistant", content = reply });
        TrimHistory();
        AppendLineTyped("Pixie AI:\n" + reply);
        SaveMemory();

        waitingForModel = false;
        requestRoutine = null;
    }

    IEnumerator RequestPixieAdvisorReply(string userInput)
    {
        waitingForModel = true;

        PixieDecisionResult decision = EvaluatePixieDecision(out PixieGameState state, out PixieDecisionConfig config);
        PixieResponse fallback = BuildDeterministicPixieResponse(decision);
        PixieResponse chosen = fallback;
        string debugError = null;

        if (useOllamaForAdvice)
        {
            bool completed = false;
            OllamaPixieClient client = new OllamaPixieClient(model, pixieAdvisorUrl, pixieAdvisorTimeoutSeconds);
            string system = PixiePromptBuilder.BuildSystemPrompt();
            string prompt = PixiePromptBuilder.BuildUserPrompt(state, config, decision);

            yield return client.RequestAdvice(this, system, prompt, (response, error) =>
            {
                completed = true;
                debugError = error;
                if (TrySanitizePixieResponse(response, decision, state.allowedActions, out PixieResponse sanitized))
                    chosen = sanitized;
            });

            if (!completed)
                debugError = "Pixie advisor request did not complete.";
        }

        string replyText = BuildPixieReplyText(chosen, debugError, useOllamaForAdvice);
        latestAdvisorResponse = chosen;
        latestAdvisorStatus = "Advice: " + chosen.recommended_action + " - " + chosen.message;

        history.Add(new OllamaMessage { role = "user", content = userInput });
        history.Add(new OllamaMessage { role = "assistant", content = replyText });
        TrimHistory();
        AppendLineTyped("Pixie AI: " + replyText);
        SaveMemory();

        waitingForModel = false;
        requestRoutine = null;
    }

    List<OllamaMessage> BuildOllamaPayload(string userInput)
    {
        List<OllamaMessage> messages = new List<OllamaMessage>();
        messages.Add(new OllamaMessage
        {
            role = "system",
            content = systemPrompt + "\n\nGame snapshot:\n" + BuildGameSnapshot() + "\nRules:\n" +
                      "- Speak warm and supportive, never cold.\n" +
                      "- Answer ONLY Pet Care game topics. If off-topic, politely refuse and redirect.\n" +
                      "- Prioritize exact in-game facts from the snapshot over guesses.\n" +
                      "- For shop item questions, use the Shop item knowledge section exactly. Never invent bought counts or effects.\n" +
                      "- If asked about boost timers, answer with the exact remaining time or say the boost is inactive.\n" +
                      "- If a mechanic is not active right now, say that clearly instead of improvising.\n" +
                      "- Be specific and avoid repeated lines.\n" +
                      "- If asked last purchase, use purchase memory exactly.\n" +
                      "- If asked savings, suggest amount and item priorities.\n" +
                      "- Never invent game mechanics or controls.\n" +
                      "- Use learned facts when relevant.\n" +
                      "- If data is unknown, say you do not know yet."
        });

        int window = Mathf.Max(0, maxHistoryMessages) * 2;
        int start = window <= 0 ? history.Count : Mathf.Max(0, history.Count - window);
        for (int i = start; i < history.Count; i++) messages.Add(history[i]);
        messages.Add(new OllamaMessage { role = "user", content = userInput });
        return messages;
    }

    List<OllamaMessage> BuildPixelCareReportPayload(string userInput, string calculatedReport)
    {
        List<OllamaMessage> messages = new List<OllamaMessage>();
        messages.Add(new OllamaMessage
        {
            role = "system",
            content =
                "You are Pixie AI in Pixel Care. The game has already calculated every report number. " +
                "Do not calculate, change, estimate, round, add, remove, or invent any numbers. " +
                "Use the exact report text and values provided by Unity. " +
                "Your job is to present the report clearly, then add a short explanation of whether the player is saving or wasting and what they should do next. " +
                "Keep the report headings: Pixel Care Report, Balance, Spending Breakdown, Tasks Completed, Insight."
        });
        messages.Add(new OllamaMessage
        {
            role = "user",
            content =
                "Player request: " + userInput + "\n\n" +
                "Unity calculated report values:\n" + calculatedReport + "\n\n" +
                "Answer with the report and a short Pixie analysis. Do not change any numbers."
        });
        return messages;
    }

    string BuildGameSnapshot()
    {
        StringBuilder sb = new StringBuilder();
        string scene = SceneManager.GetActiveScene().name;
        sb.AppendLine("Scene: " + scene);
        sb.AppendLine("Gold Coins: " + AppleCurrency.Get() + " (1 Gold Coin = $1)");
        sb.AppendLine("Total spent Gold Coins: " + GamesChatBotStats.TotalSpent);
        sb.AppendLine("Total items bought: " + GamesChatBotStats.TotalBought);
        sb.AppendLine(PixiePurchaseMemory.GetLastPurchaseSummary());
        sb.AppendLine("Recent purchases: " + PixiePurchaseMemory.GetPurchaseLogSummary(8));
        sb.AppendLine("Jump Boost timer: " + GetBoostStatusLine("Jump Boost", JumpBoostUntilKey));
        sb.AppendLine("Speed Boost timer: " + GetBoostStatusLine("Speed Boost", SpeedBoostUntilKey));
        sb.AppendLine("Cherry Boost timer: " + GetBoostStatusLine("Cherry Boost", CherryBoostUntilKey));
        sb.Append(BuildShopKnowledgeSnapshot());

        for (int i = 0; i < itemDefs.Count; i++)
            sb.AppendLine(itemDefs[i].display + " cost: " + itemDefs[i].cost + ", bought: " + GamesChatBotStats.GetBoughtCount(itemDefs[i].id));

        if (petNeeds != null)
        {
            sb.AppendLine("Hunger: " + Mathf.RoundToInt(petNeeds.Hunger));
            sb.AppendLine("Energy: " + Mathf.RoundToInt(petNeeds.Energy));
            sb.AppendLine("Hygiene: " + Mathf.RoundToInt(petNeeds.Hygiene));
            sb.AppendLine("Happiness: " + Mathf.RoundToInt(petNeeds.Happiness));
            sb.AppendLine("Health: " + Mathf.RoundToInt(petNeeds.Health));
        }

        string lastAdvice = PlayerPrefs.GetString(LastAdvicePrefix + scene, string.Empty);
        if (!string.IsNullOrWhiteSpace(lastAdvice)) sb.AppendLine("Last advice in scene: " + lastAdvice);

        if (TryGetPetAge(out string ageSummary)) sb.AppendLine("Pet age: " + ageSummary);
        int learnedCount = Mathf.Min(learnedFacts.Count, 12);
        for (int i = 0; i < learnedCount; i++)
        {
            LearnedFactData f = learnedFacts[i];
            if (f == null || string.IsNullOrWhiteSpace(f.key) || string.IsNullOrWhiteSpace(f.value)) continue;
            sb.AppendLine("Learned fact - " + f.key + ": " + f.value);
        }

        List<ChoreEntry> chores = BuildChoreEntries();
        for (int i = 0; i < chores.Count; i++) sb.AppendLine("Chore: " + chores[i].text + " (reward " + chores[i].reward + ")");
        return sb.ToString();
    }

    void TrimHistory()
    {
        int max = Mathf.Max(0, maxHistoryMessages) * 2;
        if (max <= 0) { history.Clear(); return; }
        while (history.Count > max) history.RemoveAt(0);
    }
    void LoadMemory()
    {
        if (!persistConversationAcrossSessions) return;
        string json = PlayerPrefs.GetString(MemoryKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json)) return;
        MemoryData d = JsonUtility.FromJson<MemoryData>(json);
        if (d == null) return;

        history.Clear();
        chatLines.Clear();
        if (d.history != null)
        {
            for (int i = 0; i < d.history.Count; i++)
            {
                OllamaMessage m = d.history[i];
                if (m == null || string.IsNullOrWhiteSpace(m.role) || string.IsNullOrWhiteSpace(m.content)) continue;
                history.Add(new OllamaMessage { role = m.role, content = m.content });
            }
        }

        if (d.transcript != null)
        {
            for (int i = 0; i < d.transcript.Count; i++)
            {
                string line = d.transcript[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                chatLines.Add(new ChatLine { fullText = line, visibleChars = line.Length, createdAt = Time.unscaledTime });
            }
        }
    }

    void SaveMemory()
    {
        if (!persistConversationAcrossSessions) return;
        MemoryData d = new MemoryData();
        for (int i = 0; i < history.Count; i++) d.history.Add(new OllamaMessage { role = history[i].role, content = history[i].content });

        int keep = Mathf.Min(chatLines.Count, 220);
        for (int i = chatLines.Count - keep; i < chatLines.Count; i++)
        {
            if (i < 0 || i >= chatLines.Count) continue;
            string line = chatLines[i].fullText;
            if (!string.IsNullOrWhiteSpace(line)) d.transcript.Add(line);
        }

        PlayerPrefs.SetString(MemoryKey, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
    }

    void CheckNeedAlerts()
    {
        if (petNeeds == null) return;
        if (!needsStateInitialized) SyncNeedStateImmediate();

        CheckSingleNeedAlert("Hunger", petNeeds.Hunger <= lowNeedThreshold, "Hunger is low. Buy Kiwi first for better value.", ref lowHungerPrev);
        CheckSingleNeedAlert("Energy", petNeeds.Energy <= lowNeedThreshold, "Energy is low. Go to sleep soon.", ref lowEnergyPrev);
        CheckSingleNeedAlert("Energy Gate", petNeeds.Energy < 50f, "Energy is below 50%, so levels are locked until you sleep.", ref energyGatePrev);
        CheckSingleNeedAlert("Hygiene", petNeeds.Hygiene <= lowNeedThreshold, "Hygiene is low. Use the shower.", ref lowHygienePrev);
        CheckSingleNeedAlert("Happiness", petNeeds.Happiness <= lowNeedThreshold, "Happiness is low. Do a fun activity.", ref lowHappinessPrev);
        CheckSingleNeedAlert("Health", petNeeds.Health <= lowNeedThreshold, "Health is low. Fix low needs or buy a health potion.", ref lowHealthPrev);
        CheckSingleNeedAlert("Critical Hunger", petNeeds.Hunger <= 10f, "Hunger is critical. Health will drop faster now.", ref criticalHungerPrev);
        CheckSingleNeedAlert("Critical Health", petNeeds.Health <= 10f, "Health is critical. If it reaches 0, progress resets to Tutorial.", ref criticalHealthPrev);
    }

    void CheckSingleNeedAlert(string needName, bool isLowNow, string message, ref bool wasLow)
    {
        if (isLowNow && !wasLow && Time.unscaledTime >= nextAlertAt)
        {
            nextAlertAt = Time.unscaledTime + Mathf.Max(1f, alertCooldownSeconds);
            string text = needName + " warning: " + message;
            if (isOpen) AppendLineTyped("Pixie AI alert: " + text);
            else pendingAlerts.Add(text);
        }
        wasLow = isLowNow;
    }

    void SyncNeedStateImmediate()
    {
        if (petNeeds == null) { needsStateInitialized = false; return; }
        lowHungerPrev = petNeeds.Hunger <= lowNeedThreshold;
        lowEnergyPrev = petNeeds.Energy <= lowNeedThreshold;
        energyGatePrev = petNeeds.Energy < 50f;
        lowHygienePrev = petNeeds.Hygiene <= lowNeedThreshold;
        lowHappinessPrev = petNeeds.Happiness <= lowNeedThreshold;
        lowHealthPrev = petNeeds.Health <= lowNeedThreshold;
        criticalHungerPrev = petNeeds.Hunger <= 10f;
        criticalHealthPrev = petNeeds.Health <= 10f;
        needsStateInitialized = true;
    }

    void OnGUI()
    {
        if (!isOpen || !IsSceneAllowed()) return;
        EnsureResponsiveWindowLayout();
        if (showHouseIntro && keepHouseIntroCentered)
        {
            houseIntroWindowRect = BuildIntroWindowRect();
            windowRect = houseIntroWindowRect;
        }
        ClampWindowToScreen();
        windowRect = GUI.Window(190027, windowRect, DrawChatWindow, "Pixie AI");
        ClampWindowToScreen();
        if (!showHouseIntro) DrawChoresPanel();
    }

    void DrawChatWindow(int id)
    {
        EnsureGuiStyles();
        if (showHouseIntro)
        {
            GUILayout.BeginVertical();
            DrawSolidRect(new Rect(8f, 24f, windowRect.width - 16f, windowRect.height - 32f), new Color(0f, 0f, 0f, 0.86f));
            GUILayout.Space(8f);
            string intro = activeIntroParagraph ?? string.Empty;
            int count = Mathf.Clamp(introVisibleChars, 0, intro.Length);
            float introButtonAreaHeight = 54f;
            float introScrollHeight = Mathf.Max(120f, windowRect.height - introButtonAreaHeight - 46f);
            introScrollPosition = GUILayout.BeginScrollView(introScrollPosition, false, true, GUILayout.Height(introScrollHeight));
            GUILayout.Label(intro.Substring(0, count), introParagraphStyle);
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Got it", GUILayout.Width(140f), GUILayout.Height(34f))) CloseChat();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 22f));
            return;
        }
        GUILayout.BeginVertical();

        float inputWidth = Mathf.Max(120f, windowRect.width - 58f);
        float dynamicInputHeight = GetDynamicInputHeight(inputWidth);
        float logHeight = Mathf.Max(80f, windowRect.height - 34f - (dynamicInputHeight + 52f));

        DrawSolidRect(new Rect(8f, 24f, windowRect.width - 16f, windowRect.height - 32f), new Color(0f, 0f, 0f, 0.72f));
        DrawSolidRect(new Rect(10f, 36f, windowRect.width - 20f, logHeight + 6f), new Color(0f, 0f, 0f, 0.48f));
        DrawSolidRect(new Rect(10f, windowRect.height - dynamicInputHeight - 46f, windowRect.width - 20f, dynamicInputHeight + 8f), new Color(0f, 0f, 0f, 0.62f));

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(logHeight));
        for (int i = 0; i < chatLines.Count; i++) DrawChatLine(chatLines[i]);
        GUILayout.EndScrollView();

        if (autoScrollToBottom && Event.current.type == EventType.Repaint)
        {
            scrollPosition.y = float.MaxValue;
            autoScrollToBottom = false;
        }

        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("GamesChatBotInput");
        inputBuffer = GUILayout.TextArea(inputBuffer ?? string.Empty, inputAreaStyle, GUILayout.ExpandWidth(true), GUILayout.Height(dynamicInputHeight));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.enabled = !waitingForModel;
        if (GUILayout.Button("Send", GUILayout.Width(80f))) { SendFromInput(); GUI.FocusControl("GamesChatBotInput"); }
        GUI.enabled = true;
        GUILayout.Label(waitingForModel ? "Pixie AI is thinking..." : "Press Esc to close", statusStyle);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 22f));
    }
    void DrawChoresPanel()
    {
        EnsureGuiStyles();
        List<ChoreEntry> chores = BuildChoreEntries();
        float panelWidth = Mathf.Clamp(Screen.width * 0.29f, 520f, 680f);
        float visibleTaskCount = Mathf.Clamp(chores.Count, 1, 3);
        float panelHeight = 98f + (visibleTaskCount * 72f);
        panelHeight = Mathf.Clamp(panelHeight, 265f, 360f);
        Rect r = new Rect(Screen.width - panelWidth - 20f, 20f, panelWidth, panelHeight);
        DrawSolidRect(r, new Color(0f, 0f, 0f, 0.78f));
        GUI.Box(r, "Pixie AI Chores", choreTitleStyle);

        GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 28f, r.width - 24f, r.height - 36f));
        if (TryGetChoreCooldownRemaining(out int topCooldownSeconds))
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Cooldown: " + FormatCooldownSeconds(topCooldownSeconds), statusStyle, GUILayout.ExpandWidth(false));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);
        }
        if (chores.Count == 0)
        {
            if (TryGetChoreCooldownRemaining(out int cooldownSeconds))
            {
                GUILayout.Label("Cooldown active. New chores in " + FormatCooldownSeconds(cooldownSeconds) + ".", choreLineStyle);
            }
            else
            {
                GUILayout.Label("No available quests right now.", choreLineStyle);
            }
            GUILayout.EndArea();
            return;
        }

        for (int i = 0; i < chores.Count; i++)
        {
            ChoreEntry c = chores[i];
            GUILayout.BeginHorizontal();
            float labelWidth = Mathf.Max(280f, r.width - 134f);
            GUILayout.Label((i + 1) + ". " + c.text + " (Reward " + c.reward + " Gold Coins)", choreLineStyle, GUILayout.Width(labelWidth));
            bool complete = IsChoreCompleted(c);
            bool canClaim = CanClaimChore(c.id, out int sec) && complete;
            Color prevBg = GUI.backgroundColor;
            if (canClaim)
                GUI.backgroundColor = new Color(0.22f, 0.72f, 0.28f, 1f);
            GUI.enabled = canClaim;
            if (GUILayout.Button("Done", choreButtonStyle, GUILayout.Width(90f), GUILayout.Height(34f))) ClaimChore(c);
            GUI.enabled = true;
            GUI.backgroundColor = prevBg;
            GUILayout.EndHorizontal();
            if (!complete) GUILayout.Label("Finish task to collect Gold Coins", statusStyle);
            else if (!CanClaimChore(c.id, out sec)) GUILayout.Label("Cooldown: " + sec + "s", statusStyle);
            GUILayout.Space(4f);
        }
        GUILayout.EndArea();
    }

    List<ChoreEntry> BuildChoreEntries()
    {
        RefreshActiveChoresIfNeeded();
        return activeChores;
    }

    void UpdateChoreProgressTracking()
    {
        if (petNeeds == null)
            return;

        int now = NowUnix();
        UpdateContinuousThreshold(ref energyAbove80SinceUnix, petNeeds.Energy >= 80f, now);
        UpdateContinuousThreshold(ref happinessAbove80SinceUnix, petNeeds.Happiness >= 80f, now);

        bool perfectDay = petNeeds.Hunger >= 75f &&
                          petNeeds.Energy >= 75f &&
                          petNeeds.Hygiene >= 75f &&
                          petNeeds.Happiness >= 75f &&
                          petNeeds.Health >= 75f;
        UpdateContinuousThreshold(ref perfectDaySinceUnix, perfectDay, now);
    }

    bool CanClaimChore(string choreId, out int secondsLeft)
    {
        if (!TryGetChoreCooldownRemaining(out secondsLeft))
            return true;
        return false;
    }

    bool TryGetChoreCooldownRemaining(out int secondsLeft)
    {
        secondsLeft = 0;
        string raw = PlayerPrefs.GetString(ChoreCooldownUntilKey, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime untilUtc))
        {
            PlayerPrefs.DeleteKey(ChoreCooldownUntilKey);
            return false;
        }

        TimeSpan remaining = untilUtc.ToUniversalTime() - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            PlayerPrefs.DeleteKey(ChoreCooldownUntilKey);
            PlayerPrefs.Save();
            return false;
        }

        secondsLeft = Mathf.CeilToInt((float)remaining.TotalSeconds);
        return true;
    }

    string FormatCooldownSeconds(int seconds)
    {
        if (seconds <= 0)
            return "0s";

        int mins = seconds / 60;
        int secs = seconds % 60;
        if (mins <= 0)
            return secs + "s";

        return mins + "m " + secs + "s";
    }

    void ClaimChore(ChoreEntry chore)
    {
        if (chore == null || !IsChoreCompleted(chore) || !CanClaimChore(chore.id, out _)) return;
        AppleCurrency.Add(chore.reward);
        PixelCareReport.RecordTaskCompleted();
        AppendLineTyped("Pixie AI: Chore completed. +" + chore.reward + " Gold Coins for \"" + chore.text + "\".");
        RemoveActiveChore(chore.id);
        if (activeChores.Count == 0 && pendingChoreIds.Count == 0)
            StartChoreCooldown();
        RefreshActiveChoresIfNeeded();
        SaveActiveChores();
        SaveMemory();
    }

    bool IsChoreCompleted(ChoreEntry chore)
    {
        if (chore == null) return false;
        string id = chore.id ?? string.Empty;

        if (id == "keep_energy_80_5m") return HasReachedContinuousDuration(energyAbove80SinceUnix, chore.assignedAtUnix, 300);
        if (id == "complete_level_3") return GamesChatBotStats.GetLastEventUnix("complete_level_3") >= chore.assignedAtUnix;
        if (id == "talk_ai_3") return GamesChatBotStats.CountEventsSince("talk_ai", chore.assignedAtUnix) >= 3;
        if (id == "go_to_sleep") return GamesChatBotStats.GetLastEventUnix("sleep") >= chore.assignedAtUnix || PlayerPrefs.GetInt(LastBedUseKey, 0) >= chore.assignedAtUnix;
        if (id == "buy_any_item") return CountPurchasesSince(chore.assignedAtUnix) >= 1;
        if (id == "perfect_day_5m") return HasReachedContinuousDuration(perfectDaySinceUnix, chore.assignedAtUnix, 300);
        if (id == "take_a_bath") return GamesChatBotStats.GetLastEventUnix("bath") >= chore.assignedAtUnix || PlayerPrefs.GetInt(LastShowerUseKey, 0) >= chore.assignedAtUnix;
        if (id == "complete_level_1") return GamesChatBotStats.GetLastEventUnix("complete_level_1") >= chore.assignedAtUnix;
        if (id == "last_second_save") return GamesChatBotStats.GetLastEventUnix("recover_health_low_to_full") >= chore.assignedAtUnix;
        if (id == "recover_health_low_to_full") return GamesChatBotStats.GetLastEventUnix("recover_health_low_to_full") >= chore.assignedAtUnix;
        if (id == "feed_pet_3") return CountFoodPurchasesSince(chore.assignedAtUnix) >= 3;
        if (id == "talk_ai_1") return GamesChatBotStats.CountEventsSince("talk_ai", chore.assignedAtUnix) >= 1;
        if (id == "complete_level_4") return GamesChatBotStats.GetLastEventUnix("complete_level_4") >= chore.assignedAtUnix;
        if (id == "keep_happiness_80_5m") return HasReachedContinuousDuration(happinessAbove80SinceUnix, chore.assignedAtUnix, 300);
        if (id == "buy_special_effect_potion") return CountSpecialEffectPurchasesSince(chore.assignedAtUnix) >= 1;
        if (id == "complete_level_2") return GamesChatBotStats.GetLastEventUnix("complete_level_2") >= chore.assignedAtUnix;

        return false;
    }

    string BuildLocalResponse(string query)
    {
        if (TryBuildGroundTruthReply(query, out string strictReply))
            return strictReply;

        string q = query.ToLowerInvariant();
        bool asksLevelGrind =
            q.Contains("how many levels") ||
            q.Contains("how many runs") ||
            q.Contains("remaining apples") || q.Contains("remaining gold coins") ||
            q.Contains("specific amount") ||
            (q.Contains("recommend") && q.Contains("level"));
        if (asksLevelGrind)
            return BuildLevelGrindRecommendation(query);
        if (IsReportQuery(q))
            return BuildPixelCareReportAnswer();
        if (q.Contains("last purch")) return PixiePurchaseMemory.GetLastPurchaseSummary();
        if (q.Contains("how many") && (q.Contains("bought") || q.Contains("buy"))) return BuildBoughtSummary();
        if (q.Contains("waste") || q.Contains("wasted") || q.Contains("spent")) return "You have spent " + GamesChatBotStats.TotalSpent + " Gold Coins total so far.";
        if (q.Contains("cost") || q.Contains("how much") || q.Contains("if i buy")) return BuildCostAnswer(query);
        if (q.Contains("boost") || q.Contains("potion timer") || q.Contains("time left") || q.Contains("remaining time")) return BuildBoostStatusAnswer(query);
        if (q.Contains("save") || q.Contains("save up") || q.Contains("what should i buy")) return BuildSavingsPlan();
        if (q.Contains("chore") || q.Contains("task")) return BuildChoreResponse();
        if (q.Contains("age") && q.Contains("pet") && TryBuildPetAgeReply(query, out string ageReply)) return ageReply;
        if (TryGetLearnedFactReply(query, out string learnedReply)) return learnedReply;
        if (q.Contains("help") || q.Contains("what can i ask") || q.Contains("what can you do")) return BuildHelpAnswer();
        if (q.Contains("low") || q.Contains("should buy") || q.Contains("instead")) return BuildNeedRecommendation();
        if (!IsGameRelatedQuestion(query))
            return "I can still help analyze this. If you want game-specific advice, include your scene, bars, and what happened.";
        return "Ask me something specific about your bars, shop items, levels, report, chores, or what stat to raise next.";
    }

    string BuildBoughtSummary()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Shop purchases: ");
        for (int i = 0; i < itemDefs.Count; i++)
        {
            if (i > 0) sb.Append(" | ");
            sb.Append(itemDefs[i].display).Append(": ").Append(GamesChatBotStats.GetBoughtCount(itemDefs[i].id));
        }
        sb.Append(" | Total items bought: ").Append(GamesChatBotStats.TotalBought);
        sb.Append(" | Total Gold Coins spent: ").Append(GamesChatBotStats.TotalSpent);
        sb.Append(" | ").Append(PixiePurchaseMemory.GetLastPurchaseSummary());
        return sb.ToString();
    }

    string BuildItemBoughtAnswer(ItemDef item)
    {
        int bought = GamesChatBotStats.GetBoughtCount(item.id);
        return "You have bought " + bought + " " + item.display + (bought == 1 ? "" : "s") + ".";
    }

    string BuildShopKnowledgeSnapshot()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Shop item knowledge:");
        for (int i = 0; i < itemDefs.Count; i++)
        {
            ItemDef item = itemDefs[i];
            string effect = string.IsNullOrWhiteSpace(item.effect) ? "It is a shop item." : item.effect;
            string aliases = item.aliases == null || item.aliases.Length == 0 ? string.Empty : " Aliases: " + string.Join(", ", item.aliases) + ".";
            sb.AppendLine("- " + item.display + ": costs " + item.cost + " Gold Coins. " + effect + aliases);
        }
        return sb.ToString();
    }

    string BuildPixelCareReportAnswer()
    {
        PixelCareReportData report = PixelCareReport.Build();
        return report.ToDisplayText();
    }

    string BuildPixelCareReportFallbackReply(string calculatedReport, PixelCareReportData report)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(calculatedReport);
        sb.AppendLine();
        sb.AppendLine("Pixie Analysis");
        sb.AppendLine(report.simpleStatus + ".");
        sb.AppendLine(report.nextAction);
        return sb.ToString().TrimEnd();
    }

    bool IsReportQuery(string queryLower)
    {
        string q = (queryLower ?? string.Empty).ToLowerInvariant().Replace("-", " ");
        return q.Contains("report") ||
               q.Contains("financial") ||
               q.Contains("care activity") ||
               q.Contains("spending breakdown") ||
               q.Contains("spending report") ||
               q.Contains("apple report") ||
               q.Contains("coin report") ||
               q.Contains("balance report") ||
               q.Contains("show me my stats") ||
               q.Contains("show my stats") ||
               q.Contains("what did i spend") ||
               q.Contains("what have i spent") ||
               q.Contains("am i saving") ||
               q.Contains("am i wasting");
    }

    string BuildSavingsPlan()
    {
        int goal = 0; List<string> targets = new List<string>();
        if (petNeeds != null)
        {
            if (petNeeds.Health <= 40f) { goal += GetItemCostById(GamesChatBotItemId.HealthPotion); targets.Add("Health Potion"); }
            if (petNeeds.Energy <= 40f) { goal += GetItemCostById(GamesChatBotItemId.EnergyPotion); targets.Add("Energy Potion"); }
            if (petNeeds.Hunger <= 55f) { goal += GetItemCostById(GamesChatBotItemId.SmallFood); targets.Add("Kiwi"); }
        }
        if (goal <= 0) { goal = GetItemCostById(GamesChatBotItemId.SmallFood) + GetItemCostById(GamesChatBotItemId.EnergyPotion); targets.Add("Kiwi"); targets.Add("Energy Potion"); }
        int have = AppleCurrency.Get(); int need = Mathf.Max(0, goal - have);
        if (need == 0) return "You already have enough Gold Coins. Spend around " + goal + " Gold Coins for: " + string.Join(", ", targets) + ".";
        return "Save up at least " + goal + " Gold Coins for: " + string.Join(", ", targets) + ". You still need " + need + " more Gold Coins.";
    }

    int GetItemCostById(string itemId) { for (int i = 0; i < itemDefs.Count; i++) if (itemDefs[i].id == itemId) return itemDefs[i].cost; return 0; }

    string BuildLevelGrindRecommendation(string query)
    {
        string q = (query ?? string.Empty).ToLowerInvariant();
        int have = AppleCurrency.Get();
        int target = -1;
        string targetLabel = "your goal";

        // Prefer known shop items if mentioned.
        if (q.Contains("health potion")) { target = GetItemCostById(GamesChatBotItemId.HealthPotion); targetLabel = "Health Potion"; }
        else if (q.Contains("energy potion") || q.Contains("energy bar")) { target = GetItemCostById(GamesChatBotItemId.EnergyPotion); targetLabel = "Energy Potion"; }
        else if (q.Contains("kiwi") || q.Contains("small food")) { target = GetItemCostById(GamesChatBotItemId.SmallFood); targetLabel = "Kiwi"; }
        else if (q.Contains("pineapple") || q.Contains("big food")) { target = GetItemCostById(GamesChatBotItemId.BigFood); targetLabel = "Pineapple"; }

        int parsedNumber = ExtractFirstPositiveInt(q);
        bool saysMore = q.Contains("more apples") || q.Contains("more gold coins") || q.Contains("remaining apples") || q.Contains("remaining gold coins") || q.Contains("short by") || q.Contains("missing");

        if (target <= 0 && parsedNumber > 0)
        {
            // If user says "I need 12 more Gold Coins", treat as shortfall; otherwise treat as total goal.
            target = saysMore ? have + parsedNumber : parsedNumber;
            targetLabel = saysMore ? "your shortfall target" : "your Gold Coin target";
        }

        if (target <= 0)
            return "Tell me a target, like: \"How many levels for 20 Gold Coins?\" or \"remaining Gold Coins for health potion\".";

        int shortfall = Mathf.Max(0, target - have);
        if (shortfall == 0)
            return "You already have enough Gold Coins for " + targetLabel + ". You have " + have + ".";

        string plan = BuildLevelPlan(shortfall);
        return "You need " + shortfall + " more Gold Coins for " + targetLabel + " (cost/goal " + target + ", you have " + have + "). " + plan;
    }

    int ExtractFirstPositiveInt(string text)
    {
        Match m = Regex.Match(text ?? string.Empty, "\\b(\\d+)\\b");
        if (!m.Success) return -1;
        if (int.TryParse(m.Groups[1].Value, out int v) && v > 0) return v;
        return -1;
    }

    string BuildLevelPlan(int shortfall)
    {
        if (shortfall <= 0) return "No level runs needed.";

        List<(string name, int apples)> levels = new List<(string, int)>();
        int count = Mathf.Min(recommendationLevelNames == null ? 0 : recommendationLevelNames.Length,
                              recommendationLevelAppleEstimates == null ? 0 : recommendationLevelAppleEstimates.Length);
        for (int i = 0; i < count; i++)
        {
            int a = recommendationLevelAppleEstimates[i];
            if (a <= 0) continue;
            string n = string.IsNullOrWhiteSpace(recommendationLevelNames[i]) ? ("Level " + (i + 1)) : recommendationLevelNames[i];
            levels.Add((n, a));
        }

        if (levels.Count == 0)
            return "I have no level Gold Coin estimates configured yet.";

        levels.Sort((x, y) => y.apples.CompareTo(x.apples)); // best apples first

        int remaining = shortfall;
        List<string> steps = new List<string>();
        for (int i = 0; i < levels.Count; i++)
        {
            int runCount = remaining / levels[i].apples;
            if (runCount <= 0) continue;
            remaining -= runCount * levels[i].apples;
            steps.Add(levels[i].name + " x" + runCount + " (~" + (runCount * levels[i].apples) + " Gold Coins)");
        }

        if (remaining > 0)
        {
            // one extra run of the highest-yield level to cover remainder
            steps.Add(levels[0].name + " x1 (~" + levels[0].apples + " Gold Coins)");
            remaining = 0;
        }

        // Also provide single-level shortcut when useful (e.g., Level 3 once)
        string bestSingle = string.Empty;
        for (int i = 0; i < levels.Count; i++)
        {
            int runs = Mathf.CeilToInt(shortfall / (float)levels[i].apples);
            int total = runs * levels[i].apples;
            if (string.IsNullOrEmpty(bestSingle) || total < ExtractFirstPositiveInt(bestSingle))
                bestSingle = total + "|" + levels[i].name + " x" + runs + " (~" + total + " Gold Coins)";
        }

        string singleLine = string.Empty;
        int split = bestSingle.IndexOf('|');
        if (split > 0) singleLine = bestSingle.Substring(split + 1);

        return "Recommended runs: " + string.Join(", ", steps) + ". Fast single-level option: " + singleLine + ".";
    }

    string BuildCostAnswer(string query)
    {
        Dictionary<string, int> basket = ExtractBasket(query);
        if (basket.Count == 0) return "Tell me items like: health potion and energy bar.";
        int total = 0; StringBuilder b = new StringBuilder();
        foreach (ItemDef d in itemDefs)
        {
            if (!basket.TryGetValue(d.id, out int qty)) continue;
            int c = qty * d.cost; total += c;
            b.Append(d.display).Append(" x").Append(qty).Append(" = ").Append(c).Append(" Gold Coins. ");
        }
        int remaining = AppleCurrency.Get() - total;
        return remaining >= 0 ? b + "Total cost: " + total + ". You will have " + remaining + " Gold Coins left." : b + "Total cost: " + total + ". You are short by " + Mathf.Abs(remaining) + " Gold Coins.";
    }

    string BuildBoostStatusAnswer(string query)
    {
        string q = (query ?? string.Empty).ToLowerInvariant();
        bool asksJump = q.Contains("jump");
        bool asksSpeed = q.Contains("speed");
        bool asksCherry = q.Contains("cherry");

        if (asksJump && !asksSpeed && !asksCherry)
            return GetSpecificBoostAnswer("Jump Boost", JumpBoostUntilKey);
        if (asksSpeed && !asksJump && !asksCherry)
            return GetSpecificBoostAnswer("Speed Boost", SpeedBoostUntilKey);
        if (asksCherry && !asksJump && !asksSpeed)
            return GetSpecificBoostAnswer("Cherry Boost", CherryBoostUntilKey);

        return GetSpecificBoostAnswer("Jump Boost", JumpBoostUntilKey) + " " +
               GetSpecificBoostAnswer("Speed Boost", SpeedBoostUntilKey) + " " +
               GetSpecificBoostAnswer("Cherry Boost", CherryBoostUntilKey);
    }

    string GetSpecificBoostAnswer(string displayName, string key)
    {
        if (!TryGetRemainingBoostTime(key, out TimeSpan remaining))
            return displayName + " is not active right now.";

        return displayName + " has " + FormatRemainingTime(remaining) + " left.";
    }

    string BuildHelpAnswer()
    {
        return "I can answer about Gold Coins, reports, shop costs, boost timers, bars, chores, controls, restart, quitting, level advice, and what to buy next. Ask me things like: show my report, how many Gold Coins do I need, what boost is active, what should I buy, or why a level is locked.";
    }

    bool TryFindItemDef(string queryLower, out ItemDef item)
    {
        item = default;
        if (string.IsNullOrWhiteSpace(queryLower))
            return false;

        if (Regex.IsMatch(queryLower, "(?:^|\\b)balls?(?:\\b|$)") && TryGetItemDefById(GamesChatBotItemId.Volleyball, out item))
            return true;

        if (Regex.IsMatch(queryLower, "(?:^|\\b)bear(?:\\b|$)") && TryGetItemDefById(GamesChatBotItemId.TeddyBear, out item))
            return true;

        for (int i = 0; i < itemDefs.Count; i++)
        {
            ItemDef candidate = itemDefs[i];
            if (candidate.aliases == null)
                continue;

            for (int a = 0; a < candidate.aliases.Length; a++)
            {
                string alias = candidate.aliases[a];
                if (string.IsNullOrWhiteSpace(alias))
                    continue;

                if (Regex.IsMatch(queryLower, "(?:^|\\b)" + Regex.Escape(alias) + "(?:\\b|$)"))
                {
                    item = candidate;
                    return true;
                }
            }
        }

        return false;
    }

    bool TryGetItemDefById(string itemId, out ItemDef item)
    {
        item = default;
        for (int i = 0; i < itemDefs.Count; i++)
        {
            if (itemDefs[i].id == itemId)
            {
                item = itemDefs[i];
                return true;
            }
        }

        return false;
    }

    string BuildItemAffordabilityAnswer(ItemDef item)
    {
        int have = AppleCurrency.Get();
        int need = Mathf.Max(0, item.cost - have);
        string effect = string.IsNullOrWhiteSpace(item.effect) ? string.Empty : " " + item.effect;
        if (need <= 0)
            return item.display + " costs " + item.cost + " Gold Coins." + effect + " You already have enough Gold Coins.";

        return item.display + " costs " + item.cost + " Gold Coins." + effect + " You have " + have + ", so you still need " + need + " more Gold Coins.";
    }

    string BuildItemEffectAnswer(ItemDef item)
    {
        string effect = string.IsNullOrWhiteSpace(item.effect) ? "It is a shop item." : item.effect;
        return item.display + " costs " + item.cost + " Gold Coins. " + effect;
    }

    string BuildShopItemsAnswer()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Shop items: ");
        for (int i = 0; i < itemDefs.Count; i++)
        {
            if (i > 0)
                sb.Append(" ");

            sb.Append(BuildItemEffectAnswer(itemDefs[i]));
        }
        return sb.ToString();
    }

    bool TryGetLevelAppleEstimate(string queryLower, out string levelName, out int apples)
    {
        levelName = string.Empty;
        apples = 0;

        int count = Mathf.Min(recommendationLevelNames == null ? 0 : recommendationLevelNames.Length,
                              recommendationLevelAppleEstimates == null ? 0 : recommendationLevelAppleEstimates.Length);
        if (count <= 0)
            return false;

        for (int i = 0; i < count; i++)
        {
            string configuredName = string.IsNullOrWhiteSpace(recommendationLevelNames[i]) ? ("Level " + (i + 1)) : recommendationLevelNames[i].Trim();
            string normalizedName = configuredName.ToLowerInvariant();
            string compactName = normalizedName.Replace(" ", string.Empty);
            if (queryLower.Contains(normalizedName) || queryLower.Contains(compactName))
            {
                levelName = configuredName;
                apples = Mathf.Max(0, recommendationLevelAppleEstimates[i]);
                return apples > 0;
            }
        }

        Match numericLevel = Regex.Match(queryLower, "\\blevel\\s*(\\d+)\\b");
        if (!numericLevel.Success)
            return false;

        if (!int.TryParse(numericLevel.Groups[1].Value, out int levelNumber))
            return false;

        int index = levelNumber - 1;
        if (index < 0 || index >= count)
            return false;

        levelName = string.IsNullOrWhiteSpace(recommendationLevelNames[index]) ? ("Level " + levelNumber) : recommendationLevelNames[index].Trim();
        apples = Mathf.Max(0, recommendationLevelAppleEstimates[index]);
        return apples > 0;
    }

    Dictionary<string, int> ExtractBasket(string query)
    {
        Dictionary<string, int> basket = new Dictionary<string, int>();
        string q = query.ToLowerInvariant();
        for (int i = 0; i < itemDefs.Count; i++)
        {
            ItemDef d = itemDefs[i]; int qty = 0;
            for (int a = 0; a < d.aliases.Length; a++)
            {
                string escaped = Regex.Escape(d.aliases[a]);
                MatchCollection m = Regex.Matches(q, "(?:^|\\b)(\\d+)\\s+" + escaped + "(?:\\b|$)");
                for (int k = 0; k < m.Count; k++) if (int.TryParse(m[k].Groups[1].Value, out int parsed)) qty += Mathf.Max(0, parsed);
                if (m.Count == 0 && Regex.IsMatch(q, "(?:^|\\b)" + escaped + "(?:\\b|$)")) qty += 1;
            }
            if (qty > 0) basket[d.id] = qty;
        }
        return basket;
    }

    void RefreshAdvisorStatus()
    {
        PixieDecisionResult decision = EvaluatePixieDecision(out _, out _);
        PixieResponse response = BuildDeterministicPixieResponse(decision);
        latestAdvisorResponse = response;
        latestAdvisorStatus = "Advice: " + response.recommended_action + " - " + response.message;
    }

    PixieDecisionResult EvaluatePixieDecision(out PixieGameState state, out PixieDecisionConfig config)
    {
        RefreshReferences();
        config = BuildPixieDecisionConfig();
        state = PixieGameState.Capture(petNeeds, shopButtons, config);
        return PixieDecisionEngine.Evaluate(state, config);
    }

    PixieDecisionConfig BuildPixieDecisionConfig()
    {
        PixieDecisionConfig config = PixieDecisionConfig.FromShop(shopButtons);
        if (pixieDecisionConfig == null)
            return config;

        config.HungerLowThreshold = pixieDecisionConfig.HungerLowThreshold;
        config.HungerCriticalThreshold = pixieDecisionConfig.HungerCriticalThreshold;
        config.EnergyLowThreshold = pixieDecisionConfig.EnergyLowThreshold;
        config.EnergyCriticalThreshold = pixieDecisionConfig.EnergyCriticalThreshold;
        config.EnergyGateThreshold = pixieDecisionConfig.EnergyGateThreshold;
        config.HealthLowThreshold = pixieDecisionConfig.HealthLowThreshold;
        config.HygieneLowThreshold = pixieDecisionConfig.HygieneLowThreshold;
        config.HappinessLowThreshold = pixieDecisionConfig.HappinessLowThreshold;
        config.HungerSlowThreshold = pixieDecisionConfig.HungerSlowThreshold;
        return config;
    }

    PixieResponse BuildDeterministicPixieResponse(PixieDecisionResult decision)
    {
        if (decision == null)
        {
            return new PixieResponse
            {
                recommended_action = PixieActionIds.SaveApples,
                priority_stat = "unknown",
                should_save_apples = true,
                reason = "No advisor decision is available yet.",
                message = "No advisor decision is available yet."
            };
        }

        PixieResponse response = decision.ToResponse();
        if (string.IsNullOrWhiteSpace(response.message))
            response.message = response.reason;
        return response;
    }

    bool TrySanitizePixieResponse(PixieResponse response, PixieDecisionResult fallbackDecision, List<string> allowedActions, out PixieResponse sanitized)
    {
        sanitized = BuildDeterministicPixieResponse(fallbackDecision);
        if (response == null)
            return false;

        string action = string.IsNullOrWhiteSpace(response.recommended_action) ? string.Empty : response.recommended_action.Trim();
        if (string.IsNullOrWhiteSpace(action))
            return false;

        bool allowed = allowedActions != null && allowedActions.Contains(action);
        if (!allowed)
            return false;

        sanitized.recommended_action = action;
        if (!string.IsNullOrWhiteSpace(response.priority_stat))
            sanitized.priority_stat = response.priority_stat.Trim();
        sanitized.should_save_apples = response.should_save_apples;
        if (!string.IsNullOrWhiteSpace(response.reason))
            sanitized.reason = response.reason.Trim();
        if (!string.IsNullOrWhiteSpace(response.message))
            sanitized.message = response.message.Trim();
        else
            sanitized.message = sanitized.reason;
        return true;
    }

    string BuildPixieReplyText(PixieResponse response, string debugError, bool mentionFallback)
    {
        if (response == null)
            return "I could not build advice yet.";

        StringBuilder sb = new StringBuilder();
        sb.Append("Recommended action: ").Append(FormatPixieAction(response.recommended_action)).Append(". ");
        sb.Append(string.IsNullOrWhiteSpace(response.message) ? response.reason : response.message);
        if (!string.IsNullOrWhiteSpace(response.priority_stat))
            sb.Append(" Priority stat: ").Append(response.priority_stat).Append(".");
        if (response.should_save_apples)
            sb.Append(" Save Gold Coins for the next urgent need.");
        if (mentionFallback && !string.IsNullOrWhiteSpace(debugError))
            sb.Append(" Using Unity fallback because Ollama was unavailable.");
        return Warmify(sb.ToString().Trim());
    }

    bool IsAdviceQuery(string userInput)
    {
        string q = (userInput ?? string.Empty).ToLowerInvariant();
        return q.Contains("what should i do") ||
               q.Contains("what should i raise") ||
               q.Contains("what to raise next") ||
               q.Contains("which stat") ||
               q.Contains("which bar") ||
               q.Contains("focus on first") ||
               q.Contains("fix first") ||
               q.Contains("what should i buy") ||
               q.Contains("recommend") ||
               q.Contains("advice") ||
               q.Contains("priority") ||
               q.Contains("prioritize") ||
               q.Contains("prioritise") ||
               q.Contains("should i raise") ||
               q.Contains("what next for my pet") ||
               q.Contains("energy is low") ||
               q.Contains("health is low") ||
               q.Contains("hunger is low") ||
               q.Contains("hygiene is low") ||
               q.Contains("happiness is low") ||
               q.Contains("save apples") ||
               q.Contains("save gold coins") ||
               q.Contains("levels blocked") ||
               q.Contains("slow pet");
    }

    string FormatPixieAction(string action)
    {
        switch (action)
        {
            case PixieActionIds.BuyKiwi: return "buy kiwi";
            case PixieActionIds.BuyPineapple: return "buy pineapple";
            case PixieActionIds.BuyHealthPotion: return "buy health potion";
            case PixieActionIds.BuyEnergyPotion: return "buy energy potion";
            case PixieActionIds.Sleep: return "sleep";
            case PixieActionIds.Bathe: return "bathe";
            case PixieActionIds.DoChore: return "do chore";
            case PixieActionIds.PlayUnlockedLevel: return "play unlocked level";
            case PixieActionIds.SaveApples: return "save Gold Coins";
            default: return string.IsNullOrWhiteSpace(action) ? "wait" : action.Replace('_', ' ');
        }
    }

    string BuildNeedRecommendation()
    {
        PixieDecisionResult decision = EvaluatePixieDecision(out _, out _);
        PixieResponse response = BuildDeterministicPixieResponse(decision);
        latestAdvisorResponse = response;
        latestAdvisorStatus = "Advice: " + response.recommended_action + " - " + response.message;
        string advice = BuildPixieReplyText(response, null, false);
        string scene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(LastAdvicePrefix + scene, advice);
        PlayerPrefs.Save();
        return advice;
    }

    void TryStartHouseIntro()
    {
        if (!enableFirstHouseIntroPopup) return;
        string sceneName = SceneManager.GetActiveScene().name;
        string introKey = null;
        string introText = null;

        if (string.Equals(sceneName, "House", StringComparison.OrdinalIgnoreCase))
        {
            introKey = HouseIntroSeenKey;
            introText = houseIntroParagraph;
        }
        else if (string.Equals(sceneName, "Tutorial", StringComparison.OrdinalIgnoreCase))
        {
            introKey = TutorialIntroSeenKey;
            introText = tutorialIntroParagraph;
        }

        if (string.IsNullOrWhiteSpace(introKey) || string.IsNullOrWhiteSpace(introText)) return;
        if (PlayerPrefs.GetInt(introKey, 0) == 1) return;

        preIntroWindowRect = windowRect;
        hasPreIntroWindowRect = true;
        showHouseIntro = true;
        isOpen = true;
        introVisibleChars = 0;
        introTypingProgress = 0f;
        introScrollPosition = Vector2.zero;
        activeIntroParagraph = PersonalizeIntroText(introText);
        houseIntroWindowRect = BuildIntroWindowRect();
        PlayerPrefs.SetInt(introKey, 1);
        PlayerPrefs.Save();

        if (pauseGameDuringHouseIntro)
        {
            preIntroTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    string PersonalizeIntroText(string introText)
    {
        if (string.IsNullOrWhiteSpace(introText))
            return introText;

        string playerName = GetStoredPlayerName();
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "player";

        return introText
            .Replace("{PLAYER_NAME}", playerName)
            .Replace("{PET_NAME}", playerName);
    }

    string BuildChoreResponse()
    {
        List<ChoreEntry> c = BuildChoreEntries();
        if (c.Count == 0)
        {
            if (TryGetChoreCooldownRemaining(out int cooldownSeconds))
                return "Pixie AI chore cooldown active. New chores will appear in " + FormatCooldownSeconds(cooldownSeconds) + ".";
            return "No available quests right now.";
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < c.Count; i++)
        {
            if (i > 0) sb.Append(" | ");
            sb.Append(c[i].text).Append(" (").Append(c[i].reward).Append(")");
        }
        return sb.ToString();
    }
    void DrawChatLine(ChatLine line)
    {
        float alpha = Mathf.Clamp01((Time.unscaledTime - line.createdAt) / 0.12f);
        Color lineColor = line != null ? line.color : Color.white;
        Color prev = GUI.color; GUI.color = new Color(lineColor.r, lineColor.g, lineColor.b, alpha);
        GUILayout.BeginHorizontal(); GUILayout.Label(line.VisibleText, chatLineStyle, GUILayout.ExpandWidth(true)); GUILayout.EndHorizontal();
        GUI.color = prev;
    }

    void AppendLineImmediate(string text) { AppendLineImmediate(text, Color.white); }
    void AppendLineTyped(string text) { AppendLineTyped(text, Color.white); }
    void AppendLineImmediate(string text, Color color) { chatLines.Add(new ChatLine { fullText = text, visibleChars = text.Length, createdAt = Time.unscaledTime, color = color }); TrimTranscriptLines(); autoScrollToBottom = true; }
    void AppendLineTyped(string text, Color color) { chatLines.Add(new ChatLine { fullText = text, visibleChars = 0, createdAt = Time.unscaledTime, color = color }); TrimTranscriptLines(); typingLineIndex = chatLines.Count - 1; autoScrollToBottom = true; }

    void TrimTranscriptLines()
    {
        const int max = 260;
        if (chatLines.Count <= max) return;
        int remove = chatLines.Count - max;
        chatLines.RemoveRange(0, remove);
        if (typingLineIndex >= 0) typingLineIndex = Mathf.Max(-1, typingLineIndex - remove);
    }

    void RefreshReferences()
    {
        if (shopButtons == null || !shopButtons.gameObject.activeInHierarchy) shopButtons = FindAnyObjectByType<ShopButtons>();
        if (petNeeds != null && petNeeds.gameObject.activeInHierarchy) return;
        PetNeeds[] all = FindObjectsByType<PetNeeds>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < all.Length; i++) if (all[i] != null && all[i].gameObject.activeInHierarchy) { petNeeds = all[i]; return; }
        petNeeds = FindAnyObjectByType<PetNeeds>();
    }

    void ConsumePendingSystemWarning()
    {
        string text = PlayerPrefs.GetString(PendingSystemWarningTextKey, string.Empty);
        if (string.IsNullOrWhiteSpace(text))
            return;

        string colorCode = PlayerPrefs.GetString(PendingSystemWarningColorKey, "white");
        bool autoOpen = PlayerPrefs.GetInt(PendingSystemWarningAutoOpenKey, 0) == 1;
        Color color = ParsePendingWarningColor(colorCode);

        AppendLineTyped(text, color);
        if (autoOpen)
            isOpen = true;

        PlayerPrefs.DeleteKey(PendingSystemWarningTextKey);
        PlayerPrefs.DeleteKey(PendingSystemWarningColorKey);
        PlayerPrefs.DeleteKey(PendingSystemWarningAutoOpenKey);
        PlayerPrefs.Save();
    }

    static Color ParsePendingWarningColor(string colorCode)
    {
        if (string.Equals(colorCode, "yellow", StringComparison.OrdinalIgnoreCase))
            return new Color(1f, 0.92f, 0.25f, 1f);
        return Color.white;
    }

    public static void QueueLevelSicknessWarning(string petName, string levelName)
    {
        string safePetName = string.IsNullOrWhiteSpace(petName) ? "Your pet" : petName.Trim();
        string safeLevelName = string.IsNullOrWhiteSpace(levelName) ? "this level" : levelName.Trim();
        string text = "Pixie AI warning: " + safePetName + " has gotten sick while in " + safeLevelName + " and his health will lower quicker! Buy a health potion to cure it.";

        PlayerPrefs.SetString(PendingSystemWarningTextKey, text);
        PlayerPrefs.SetString(PendingSystemWarningColorKey, "yellow");
        PlayerPrefs.SetInt(PendingSystemWarningAutoOpenKey, 1);
        PlayerPrefs.Save();
    }

    void RebuildItemDefs()
    {
        int hc = shopButtons != null ? shopButtons.healthPotionCost : 20;
        int ec = shopButtons != null ? shopButtons.energyPotionCost : 20;
        int sc = shopButtons != null ? shopButtons.smallFoodCost : 5;
        int bc = shopButtons != null ? shopButtons.bigFoodCost : 20;
        int jc = shopButtons != null ? shopButtons.jumpBoostCost : 10;
        int spc = shopButtons != null ? shopButtons.speedBoostCost : 10;
        int cc = shopButtons != null ? shopButtons.cherryBoostCost : 15;
        int tc = shopButtons != null ? shopButtons.teddyBearCost : 25;
        int vc = shopButtons != null ? shopButtons.volleyballCost : 10;
        itemDefs.Clear();
        itemDefs.Add(new ItemDef(GamesChatBotItemId.HealthPotion, "Health Potion", hc, "It restores Health to 100 and cures sickness.", "health potion", "potion", "medicine"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.EnergyPotion, "Energy Potion", ec, "It restores Energy to 100.", "energy potion", "energy bar"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.SmallFood, "Kiwi", sc, "It gives +25 Hunger.", "kiwi", "small food", "snack"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.BigFood, "Pineapple", bc, "It restores Hunger to 100.", "pineapple", "big food", "meal"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.JumpBoost, "Jump Boost", jc, "It increases jump height for about " + Mathf.RoundToInt(shopButtons != null ? shopButtons.boostDurationMinutes : 10f) + " minutes.", "jump boost", "jump potion", "jump boost potion"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.SpeedBoost, "Speed Boost", spc, "It increases movement speed for about " + Mathf.RoundToInt(shopButtons != null ? shopButtons.boostDurationMinutes : 10f) + " minutes.", "speed boost", "speed potion", "speed boost potion"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.CherryBoost, "Cherry", cc, "It gives +10 Hunger and slows all stat decay by 10% for 5 minutes.", "cherry", "cherry boost", "cherries"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.TeddyBear, "Teddy Bear", tc, "It gives +50 Happiness.", "teddy", "teddy bear", "bear", "bear toy"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.Volleyball, "Volleyball", vc, "It gives +5 Happiness and slows Happiness loss for 1 minute.", "volleyball", "voleyball", "vollyball", "volleybal", "volley ball", "beach ball", "beachball", "ball"));
    }

    bool IsSceneAllowed()
    {
        string current = SceneManager.GetActiveScene().name;
        if (allowedScenes == null || allowedScenes.Length == 0) return true;
        for (int i = 0; i < allowedScenes.Length; i++) if (string.Equals(current, allowedScenes[i], StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    bool WasOpenPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            if (openKey == KeyCode.G && k.gKey.wasPressedThisFrame) return true;
            if (openKey == KeyCode.BackQuote && k.backquoteKey.wasPressedThisFrame) return true;
        }
#endif
        return Input.GetKeyDown(openKey);
    }

    bool WasClosePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null && closeKey == KeyCode.Escape && k.escapeKey.wasPressedThisFrame) return true;
#endif
        return Input.GetKeyDown(closeKey);
    }

    static bool WasSubmitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) return true;
#endif
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }

    void ClampWindowToScreen()
    {
        const float m = 10f;
        windowRect.width = Mathf.Clamp(windowRect.width, 320f, Mathf.Max(320f, Screen.width - (m * 2f)));
        windowRect.height = Mathf.Clamp(windowRect.height, 280f, Mathf.Max(280f, Screen.height - (m * 2f)));
        windowRect.x = Mathf.Clamp(windowRect.x, m, Mathf.Max(m, Screen.width - windowRect.width - m));
        windowRect.y = Mathf.Clamp(windowRect.y, m, Mathf.Max(m, Screen.height - windowRect.height - m));
    }

    void EnsureResponsiveWindowLayout()
    {
        bool screenChanged = !windowLayoutInitialized || Screen.width != lastScreenWidth || Screen.height != lastScreenHeight;
        if (!screenChanged)
            return;

        Rect targetRect = showHouseIntro ? BuildIntroWindowRect() : BuildChatWindowRect();
        if (!windowLayoutInitialized)
        {
            windowRect = targetRect;
        }
        else if (showHouseIntro)
        {
            windowRect = targetRect;
        }
        else
        {
            float oldWidthRange = Mathf.Max(1f, lastScreenWidth - windowRect.width - 20f);
            float oldHeightRange = Mathf.Max(1f, lastScreenHeight - windowRect.height - 20f);
            float normalizedX = Mathf.Clamp01((windowRect.x - 10f) / oldWidthRange);
            float normalizedY = Mathf.Clamp01((windowRect.y - 10f) / oldHeightRange);

            windowRect.width = targetRect.width;
            windowRect.height = targetRect.height;

            float newWidthRange = Mathf.Max(1f, Screen.width - windowRect.width - 20f);
            float newHeightRange = Mathf.Max(1f, Screen.height - windowRect.height - 20f);
            windowRect.x = 10f + normalizedX * newWidthRange;
            windowRect.y = 10f + normalizedY * newHeightRange;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        windowLayoutInitialized = true;
    }

    Rect BuildChatWindowRect()
    {
        float width = Mathf.Clamp(Screen.width * Mathf.Clamp(chatWindowWidthPercent, 0.2f, 0.8f), chatWindowMinWidth, chatWindowMaxWidth);
        float height = Mathf.Clamp(Screen.height * Mathf.Clamp(chatWindowHeightPercent, 0.25f, 0.85f), chatWindowMinHeight, chatWindowMaxHeight);
        width = Mathf.Min(width, Screen.width - 20f);
        height = Mathf.Min(height, Screen.height - 20f);
        return new Rect(20f, 20f, width, height);
    }

    Rect BuildIntroWindowRect()
    {
        float width = Mathf.Clamp(Screen.width * Mathf.Clamp(introWindowWidthPercent, 0.25f, 0.9f), introWindowMinWidth, introWindowMaxWidth);
        float height = Mathf.Clamp(Screen.height * Mathf.Clamp(introWindowHeightPercent, 0.25f, 0.85f), introWindowMinHeight, introWindowMaxHeight);
        width = Mathf.Min(width, Screen.width - 20f);
        height = Mathf.Min(height, Screen.height - 20f);
        return new Rect((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
    }

    void EnsureGuiStyles()
    {
        if (chatLineStyle == null)
        {
            chatLineStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };
            chatLineStyle.margin = new RectOffset(6, 6, 5, 5);
            chatLineStyle.normal.textColor = Color.white;
        }
        chatLineStyle.fontSize = Mathf.Max(14, chatFontSize);
        if (inputAreaStyle == null)
        {
            inputAreaStyle = new GUIStyle(GUI.skin.textArea) { wordWrap = true };
            inputAreaStyle.padding = new RectOffset(8, 8, 6, 6);
            inputAreaStyle.normal.textColor = Color.white; inputAreaStyle.hover.textColor = Color.white; inputAreaStyle.focused.textColor = Color.white; inputAreaStyle.active.textColor = Color.white;
        }
        inputAreaStyle.fontSize = Mathf.Max(14, inputFontSize);
        if (statusStyle == null)
        {
            statusStyle = new GUIStyle(GUI.skin.label);
            statusStyle.normal.textColor = Color.white;
            statusStyle.wordWrap = true;
        }
        statusStyle.fontSize = Mathf.Max(13, statusFontSize);
        if (choreLineStyle == null)
        {
            choreLineStyle = new GUIStyle(chatLineStyle);
        }
        choreLineStyle.wordWrap = true;
        choreLineStyle.margin = new RectOffset(6, 6, 5, 5);
        choreLineStyle.normal.textColor = Color.white;
        choreLineStyle.fontSize = Mathf.Max(14, choreTextFontSize);
        if (choreTitleStyle == null)
        {
            choreTitleStyle = new GUIStyle(GUI.skin.box);
            choreTitleStyle.alignment = TextAnchor.UpperCenter;
            choreTitleStyle.normal.textColor = Color.white;
            choreTitleStyle.fontStyle = FontStyle.Bold;
        }
        choreTitleStyle.fontSize = Mathf.Max(14, choreTitleFontSize);
        if (choreButtonStyle == null)
        {
            choreButtonStyle = new GUIStyle(GUI.skin.button);
            choreButtonStyle.wordWrap = false;
            choreButtonStyle.alignment = TextAnchor.MiddleCenter;
            choreButtonStyle.padding = new RectOffset(8, 8, 4, 4);
        }
        choreButtonStyle.fontSize = Mathf.Max(13, choreButtonFontSize);
        if (introParagraphStyle == null)
        {
            introParagraphStyle = new GUIStyle(chatLineStyle);
            introParagraphStyle.wordWrap = true;
            introParagraphStyle.fontSize = Mathf.Max(14, houseIntroFontSize);
            introParagraphStyle.richText = false;
            introParagraphStyle.margin = new RectOffset(10, 10, 8, 8);
        }
        else if (introParagraphStyle.fontSize != Mathf.Max(14, houseIntroFontSize))
        {
            introParagraphStyle.fontSize = Mathf.Max(14, houseIntroFontSize);
        }
    }

    float GetDynamicInputHeight(float width)
    {
        float minH = Mathf.Max(20f, inputMinHeight);
        float maxH = Mathf.Max(minH, inputMaxHeight);
        string content = string.IsNullOrEmpty(inputBuffer) ? " " : inputBuffer;
        return Mathf.Clamp(inputAreaStyle.CalcHeight(new GUIContent(content), width), minH, maxH);
    }

    static void DrawSolidRect(Rect rect, Color color)
    {
        Color prev = GUI.color; GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = prev;
    }

    string GetBoostStatusLine(string label, string key)
    {
        if (!TryGetRemainingBoostTime(key, out TimeSpan remaining))
            return label + ": inactive";

        return label + ": " + FormatRemainingTime(remaining) + " remaining";
    }

    bool TryGetRemainingBoostTime(string key, out TimeSpan remaining)
    {
        remaining = TimeSpan.Zero;
        string raw = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime untilUtc))
            return false;

        remaining = untilUtc.ToUniversalTime() - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
            return false;

        return true;
    }

    string FormatRemainingTime(TimeSpan remaining)
    {
        if (remaining.TotalHours >= 1d)
        {
            int hours = Mathf.FloorToInt((float)remaining.TotalHours);
            int mins = Mathf.FloorToInt((float)remaining.TotalMinutes) % 60;
            if (mins > 0)
                return hours + " hour" + (hours == 1 ? "" : "s") + ", " + mins + " minute" + (mins == 1 ? "" : "s");
            return hours + " hour" + (hours == 1 ? "" : "s");
        }
        if (remaining.TotalMinutes >= 1d)
        {
            int mins = Mathf.FloorToInt((float)remaining.TotalMinutes);
            int secs = Mathf.Clamp(remaining.Seconds, 0, 59);
            return mins + " minute" + (mins == 1 ? "" : "s") + ", " + secs + " second" + (secs == 1 ? "" : "s");
        }

        int totalSecs = Mathf.Max(1, Mathf.CeilToInt((float)remaining.TotalSeconds));
        return totalSecs + " second" + (totalSecs == 1 ? "" : "s");
    }

    bool TryBuildGroundTruthReply(string query, out string reply)
    {
        string q = (query ?? string.Empty).ToLowerInvariant().Replace("-", " ");

        if (TryBuildCurrentNeedStatusReply(q, out reply))
            return true;

        if (TryBuildNeedPriorityReply(q, out reply))
            return true;

        if (IsReportQuery(q))
        {
            reply = BuildPixelCareReportAnswer();
            return true;
        }

        bool asksLastSecondSave =
            q.Contains("last second save");
        if (asksLastSecondSave)
        {
            reply =
                "Last Second Save means letting your pet's Health get down to 25%, then recovering it back to 100%. " +
                "It is a risky chore because you are close to danger, so you should do it after gathering enough Gold Coins for a Health Potion and other supplies.";
            return true;
        }

        bool asksDeathPenalty =
            (q.Contains("what happens") || q.Contains("what happen") || q.Contains("what if") || q.Contains("when")) &&
            (q.Contains("die") || q.Contains("dying") || q.Contains("death") || q.Contains("lose all hearts"));
        if (asksDeathPenalty)
        {
            reply =
                "When you lose all hearts in Level 1, Level 2, Level 3, or Level 4, you lose 3 Gold Coins and the level restarts. " +
                "In the Tutorial, you restart at the tutorial spawn without that Gold Coin penalty.";
            return true;
        }

        bool asksShopItems =
            q.Contains("all shop items") || q.Contains("shop items") || q.Contains("items in the shop") ||
            q.Contains("items in shop") || q.Contains("what can i buy") || q.Contains("what does the shop sell") ||
            q.Contains("what do they sell") || q.Contains("list the shop") || q.Contains("list shop");
        if (asksShopItems)
        {
            reply = BuildShopItemsAnswer();
            return true;
        }

        bool asksSpecificBoughtCount =
            (q.Contains("how many") || q.Contains("did i buy") || q.Contains("have i bought")) &&
            (q.Contains("bought") || q.Contains("buy"));
        if (asksSpecificBoughtCount && TryFindItemDef(q, out ItemDef boughtItem))
        {
            reply = BuildItemBoughtAnswer(boughtItem);
            return true;
        }

        bool asksItemLocation =
            q.Contains("where can i buy") || q.Contains("where do i buy") || q.Contains("where do i get") ||
            q.Contains("where can i get") || q.Contains("where do i find") || q.Contains("where is");
        if (asksItemLocation && TryFindItemDef(q, out ItemDef locatedItem))
        {
            reply = locatedItem.display + " is bought from the LEVELS menu in the top-right corner. Open LEVELS, then go to Pet Care/the shop to buy it.";
            return true;
        }

        ItemDef effectItem = default;
        bool asksItemEffect =
            (q.Contains("what does") || q.Contains("what do") || q.Contains("tell me what") ||
             q.Contains("effect") || q.Contains("ability") || q.Contains("use for") ||
             q.Contains("what is") || q.Contains("what are") || q.Contains("what")) &&
            TryFindItemDef(q, out effectItem);
        if (asksItemEffect)
        {
            reply = BuildItemEffectAnswer(effectItem);
            return true;
        }

        bool asksItemCostOrNeed =
            q.Contains("how many apples") || q.Contains("how many gold coins") || q.Contains("cost") || q.Contains("how much") || q.Contains("need");
        if (asksItemCostOrNeed && TryFindItemDef(q, out ItemDef pricedItem))
        {
            reply = BuildItemAffordabilityAnswer(pricedItem);
            return true;
        }

        bool asksLevelAppleCount =
            q.Contains("how many apples") || q.Contains("how many gold coins") || q.Contains("apple count") || q.Contains("coin count") || q.Contains("apples are in") || q.Contains("gold coins are in") || q.Contains("apples in") || q.Contains("gold coins in");
        if (asksLevelAppleCount && TryGetLevelAppleEstimate(q, out string levelEstimateName, out int levelEstimateApples))
        {
            reply = levelEstimateName + " is configured to give about " + levelEstimateApples + " Gold Coins per completion.";
            return true;
        }

        bool asksPriorityScenario =
            (q.Contains("hunger") && q.Contains("25") && q.Contains("energy") && q.Contains("40")) ||
            (q.Contains("before entering level 2") && q.Contains("prioritize")) ||
            (q.Contains("prioritise") && q.Contains("hunger") && q.Contains("energy"));
        if (asksPriorityScenario)
        {
            reply =
                "With Hunger around 25 and Energy around 40, prioritize food first. " +
                "Your best sequence is to earn enough Gold Coins for a Kiwi, complete chores and a level run, " +
                "then secure health support, and continue buying Kiwi until Hunger is fully stabilized. " +
                "That order protects survival first, then restores long-term performance.";
            return true;
        }

        bool asksBestLevelForCoins =
            (q.Contains("which level") && (q.Contains("earn coins") || q.Contains("earn apples") || q.Contains("earn gold coins") || q.Contains("quickly") || q.Contains("buy food"))) ||
            (q.Contains("best level") && (q.Contains("coins") || q.Contains("apples")));
        if (asksBestLevelForCoins)
        {
            reply = BuildLevelPlan(Mathf.Max(1, GetItemCostById(GamesChatBotItemId.BigFood))) +
                    " Based on your configured estimates, higher-yield levels reduce the number of runs you need.";
            return true;
        }

        bool asksLevelEnergyLock = (q.Contains("can't play") || q.Contains("cannot play") || q.Contains("cant play") || q.Contains("level locked") || q.Contains("why level"))
                                   && (q.Contains("energy") || q.Contains("why"));
        if (asksLevelEnergyLock)
        {
            reply = "Levels are blocked when Energy is below 50%. Sleep in your bed to refill Energy, then try again.";
            return true;
        }

        bool asksGrowth = (q.Contains("how many apples") || q.Contains("how many gold coins")) && q.Contains("grow") ||
                          q.Contains("need to grow") ||
                          q.Contains("apples to grow") ||
                          q.Contains("gold coins to grow") ||
                          q.Contains("how do i grow") ||
                          q.Contains("growth requirement");
        if (asksGrowth)
        {
            reply = "To grow, you need 20 Gold Coins and all bars above 70%.";
            return true;
        }

        bool asksControls = q.Contains("how do i walk") || q.Contains("how to walk") || q.Contains("walk") ||
                            q.Contains("move") || q.Contains("controls") || q.Contains("control") ||
                            q.Contains("jump") || q.Contains("double jump");
        if (asksControls)
        {
            reply =
                "Current controls in this build: move with A/D or Left/Right Arrow. " +
                "Jump uses the Unity Jump input (Space by default). " +
                "You can double-jump by pressing Jump again while in air.";
            return true;
        }

        bool asksPlayerName =
            q.Contains("what is my name") || q.Contains("whats my name") ||
            q.Contains("do you know my name") || q.Contains("tell me my name") ||
            q.Contains("who am i");
        if (asksPlayerName)
        {
            string playerName = GetStoredPlayerName();
            reply = string.IsNullOrWhiteSpace(playerName)
                ? "I do not have your player name saved yet."
                : "Your name is " + playerName + ".";
            return true;
        }

        bool asksRestart =
            q.Contains("restart") || q.Contains("reset progress") || q.Contains("start over") ||
            q.Contains("new save") || q.Contains("new game");
        if (asksRestart)
        {
            reply =
                "To restart your progress, use the reset shortcut: press Ctrl + Shift + S. " +
                "Use it only when you want a full fresh start, because it clears your saved progress.";
            return true;
        }

        bool asksCloseChatbot =
            q.Contains("close chatbot") || q.Contains("close the chatbot") || q.Contains("close pixie") ||
            q.Contains("quit chatbot") || q.Contains("exit chatbot") || q.Contains("how do i close the ai") ||
            q.Contains("how do i close pixie") || q.Contains("how do i close the chatbot") ||
            q.Contains("how do i quit the chatbot") || q.Contains("how to quit the chatbot") ||
            q.Contains("how do i exit the chatbot") || q.Contains("how to close pixie ai") ||
            q.Contains("how to close the chat") || q.Contains("how do i close the chat") ||
            q.Contains("how to leave the chat") || q.Contains("how do i leave the chat") ||
            q.Contains("leave the chat") || q.Contains("close the chat");
        if (asksCloseChatbot)
        {
            reply =
                "To close Pixie AI, press the Escape key. That will dismiss the chatbot window, and you can open it again at any time by pressing G.";
            return true;
        }

        bool asksQuit =
            (q.Contains("how do i quit") || q.Contains("how to quit") || q.Contains("quit game") ||
             q.Contains("exit game") || q.Contains("close game")) &&
            !q.Contains("chatbot") && !q.Contains("pixie");
        if (asksQuit)
        {
            reply =
                "To quit safely, type /saveandquit in Pixie AI, or use the in-game Quit button. " +
                "On PC you can also close the game window.";
            return true;
        }

        bool asksLocations =
            (q.Contains("where is level") || q.Contains("where are levels") || q.Contains("where is the shop") ||
             q.Contains("where is shop") || q.Contains("where is hospital") || q.Contains("where is pet care") ||
             q.Contains("where do i find level") || q.Contains("where do i find the shop") || q.Contains("where do i find hospital") ||
             q.Contains("how do i get to level") || q.Contains("how do i get to the shop") ||
             q.Contains("how do i get to hospital") || q.Contains("how do i get to pet care") ||
             q.Contains("how do i find level") || q.Contains("how do i find the hospital") ||
             q.Contains("where is level 1") || q.Contains("where is level 2") || q.Contains("where is level 3") || q.Contains("where is level 4") ||
             q.Contains("where do i go to the shop") || q.Contains("where do i go to shop") ||
             q.Contains("where do i go to pet care") || q.Contains("how do i go to the shop") ||
             q.Contains("how do i go to pet care") || q.Contains("where can i find pet care"));
        if (asksLocations)
        {
            reply =
                "You can access the shop, Pet Care, the hospital, and all levels from the LEVELS menu in the top-right corner of the screen. Open that menu, then select the destination you would like to visit.";
            return true;
        }

        bool asksBoostTimer =
            (q.Contains("jump boost") || q.Contains("speed boost") || q.Contains("cherry boost") || q.Contains("cherry") || q.Contains("boost potion") || q.Contains("speed potion") || q.Contains("jump potion")) &&
            (q.Contains("time") || q.Contains("timer") || q.Contains("left") || q.Contains("remaining") || q.Contains("active"));
        if (asksBoostTimer)
        {
            reply = BuildBoostStatusAnswer(query);
            return true;
        }

        bool asksBoostInfo =
            (q.Contains("what does jump boost do") || q.Contains("what does speed boost do") ||
             q.Contains("what does cherry do") || q.Contains("what does cherry boost do") ||
             q.Contains("what does teddy do") || q.Contains("what does teddy bear do") ||
             q.Contains("what does volleyball do") || q.Contains("what does volley ball do") || q.Contains("what does beach ball do") ||
             q.Contains("how much does jump boost") || q.Contains("how much does speed boost") ||
             q.Contains("how much does teddy") || q.Contains("how much is teddy") || q.Contains("how much is teddy bear") ||
             q.Contains("how much does volleyball") || q.Contains("how much is volleyball") ||
             q.Contains("how much does cherry") || q.Contains("how much is cherry") ||
             q.Contains("how long does teddy") || q.Contains("how long does teddy bear") ||
             q.Contains("how long does jump boost") || q.Contains("how long does speed boost") ||
             q.Contains("how long does volleyball") || q.Contains("how long does volley ball") ||
             q.Contains("how long does cherry") || q.Contains("how long does cherry boost"));
        if (asksBoostInfo)
        {
            if (q.Contains("teddy"))
            {
                float happiness = shopButtons != null ? shopButtons.teddyBearHappinessRestore : 50f;
                reply =
                    "Teddy Bear costs " + GetItemCostById(GamesChatBotItemId.TeddyBear) + " Gold Coins and restores " +
                    Mathf.RoundToInt(happiness) + " Happiness.";
                return true;
            }

            if (q.Contains("volleyball") || q.Contains("volley ball") || q.Contains("beach ball"))
            {
                float duration = shopButtons != null ? shopButtons.volleyballHappinessDecaySlowDurationMinutes : 1f;
                float happiness = shopButtons != null ? shopButtons.volleyballHappinessRestore : 5f;
                reply =
                    "Volleyball costs " + GetItemCostById(GamesChatBotItemId.Volleyball) + " Gold Coins, restores " +
                    Mathf.RoundToInt(happiness) + " Happiness, and slows Happiness loss for about " +
                    Mathf.RoundToInt(duration) + " minute.";
                return true;
            }

            if (q.Contains("cherry"))
            {
                float duration = shopButtons != null ? shopButtons.cherryDecayBoostDurationMinutes : 5f;
                float hunger = shopButtons != null ? shopButtons.cherryHungerRestore : 10f;
                reply =
                    "Cherry costs " + GetItemCostById(GamesChatBotItemId.CherryBoost) + " Gold Coins, restores " +
                    Mathf.RoundToInt(hunger) + " Hunger, and slows all stat decay by 10% for about " +
                    Mathf.RoundToInt(duration) + " minutes.";
                return true;
            }

            reply =
                "Jump Boost costs " + GetItemCostById(GamesChatBotItemId.JumpBoost) + " Gold Coins and increases jump height. " +
                "Speed Boost costs " + GetItemCostById(GamesChatBotItemId.SpeedBoost) + " Gold Coins and increases movement speed. " +
                "Their shop duration is currently set to about " + Mathf.RoundToInt(shopButtons != null ? shopButtons.boostDurationMinutes : 10f) + " minutes. " +
                "Teddy Bear costs " + GetItemCostById(GamesChatBotItemId.TeddyBear) + " Gold Coins and restores " + Mathf.RoundToInt(shopButtons != null ? shopButtons.teddyBearHappinessRestore : 50f) + " Happiness. " +
                "Cherry costs " + GetItemCostById(GamesChatBotItemId.CherryBoost) + " Gold Coins, restores 10 Hunger, and slows all stat decay by 10% for about 5 minutes.";
            return true;
        }

        bool asksApples = q.Contains("how do you get apple") || q.Contains("how to get apple") ||
                          q.Contains("how do you get gold coin") || q.Contains("how to get gold coin") ||
                          q.Contains("how get apple") || q.Contains("get appies") || q.Contains("get apples") ||
                          q.Contains("get gold coins") || q.Contains("where do i get apple") || q.Contains("where do i get gold coin") || q.Contains("how do i earn apple") || q.Contains("how do i earn gold coin") ||
                          q.Contains("how do i get appies") || q.Contains("how to get appies");
        if (asksApples)
        {
            int apples = AppleCurrency.Get();
            reply =
                "You get Gold Coins by collecting coin/fruit pickups in levels and by completing Pixie AI chores. " +
                "Gold Coins are your currency for items and level costs, and 1 Gold Coin = $1. " +
                "Current Gold Coins: " + apples + ".";
            return true;
        }

        bool asksAppleSources = q.Contains("where apples") || q.Contains("where gold coins") || q.Contains("apple source") || q.Contains("coin source") || q.Contains("gold coin source") || q.Contains("appies source");
        if (asksAppleSources)
        {
            reply = "Gold Coin sources in this game: level pickups and completed chores. No fake inventory/store currency generator.";
            return true;
        }

        if (!asksControls && !asksPlayerName && !asksApples && !asksAppleSources && !asksRestart && !asksQuit && !asksCloseChatbot && !asksLocations && !asksBoostTimer && !asksBoostInfo)
        {
            reply = null;
            return false;
        }

        reply = null;
        return false;
    }

    bool TryBuildCurrentNeedStatusReply(string q, out string reply)
    {
        reply = null;
        if (string.IsNullOrWhiteSpace(q))
            return false;

        bool asksCurrentValue =
            q.Contains("what percent") || q.Contains("what percentage") ||
            q.Contains("percentage") || q.Contains("percantage") ||
            q.Contains("percent") || q.Contains("percnt") ||
            q.Contains("how much") || q.Contains("how full") ||
            q.Contains("what is my") || q.Contains("whats my") ||
            q.Contains("what's my") || q.Contains("show me") ||
            q.Contains("current") || q.Contains("at");

        bool asksAnyNeed =
            q.Contains("health") || q.Contains("energy") || q.Contains("hunger") ||
            q.Contains("hygiene") || q.Contains("happiness") ||
            q.Contains("stats") || q.Contains("bars") || q.Contains("needs");

        if (!asksCurrentValue || !asksAnyNeed)
            return false;

        RefreshReferences();
        if (petNeeds == null)
        {
            reply = "I cannot read your pet bars right now because the active PetNeeds object is not available.";
            return true;
        }

        bool asksAll =
            q.Contains("stats") || q.Contains("bars") || q.Contains("needs") ||
            CountMentionedNeeds(q) != 1;

        if (asksAll)
        {
            reply =
                "Current pet bars: " +
                "Health " + FormatNeedPercent(petNeeds.Health) + ", " +
                "Energy " + FormatNeedPercent(petNeeds.Energy) + ", " +
                "Hunger " + FormatNeedPercent(petNeeds.Hunger) + ", " +
                "Hygiene " + FormatNeedPercent(petNeeds.Hygiene) + ", " +
                "Happiness " + FormatNeedPercent(petNeeds.Happiness) + ".";
            return true;
        }

        if (q.Contains("health"))
            reply = "Your pet's Health is at " + FormatNeedPercent(petNeeds.Health) + ".";
        else if (q.Contains("energy"))
            reply = "Your pet's Energy is at " + FormatNeedPercent(petNeeds.Energy) + ".";
        else if (q.Contains("hunger"))
            reply = "Your pet's Hunger is at " + FormatNeedPercent(petNeeds.Hunger) + ".";
        else if (q.Contains("hygiene"))
            reply = "Your pet's Hygiene is at " + FormatNeedPercent(petNeeds.Hygiene) + ".";
        else if (q.Contains("happiness"))
            reply = "Your pet's Happiness is at " + FormatNeedPercent(petNeeds.Happiness) + ".";

        return !string.IsNullOrWhiteSpace(reply);
    }

    bool TryBuildNeedPriorityReply(string q, out string reply)
    {
        reply = null;
        if (string.IsNullOrWhiteSpace(q))
            return false;

        bool asksPriorityNeed =
            q.Contains("what to raise next") ||
            q.Contains("what should i raise") ||
            q.Contains("what should i focus on") ||
            q.Contains("what should i fix first") ||
            q.Contains("which stat should i raise") ||
            q.Contains("which bar should i raise") ||
            q.Contains("which stat is most important") ||
            q.Contains("what needs to go up next") ||
            ((q.Contains("priority") || q.Contains("prioritize") || q.Contains("prioritise")) &&
             (q.Contains("health") || q.Contains("hunger") || q.Contains("energy") || q.Contains("hygiene") || q.Contains("happiness") || q.Contains("stat") || q.Contains("bar")));

        if (!asksPriorityNeed)
            return false;

        RefreshReferences();
        if (petNeeds == null)
        {
            reply = "I cannot check which stat to raise next because the active PetNeeds object is not available.";
            return true;
        }

        string needName = GetTopPriorityNeedName(out float currentValue, out string reason);
        if (string.IsNullOrWhiteSpace(needName))
        {
            reply = "Your bars look stable right now. Focus on progression or save Gold Coins for the next urgent need.";
            return true;
        }

        reply = needName + " should be raised next because it is at " + FormatNeedPercent(currentValue) + ". " + reason;
        return true;
    }

    string GetTopPriorityNeedName(out float value, out string reason)
    {
        value = 100f;
        reason = "It is your most urgent bar right now.";
        if (petNeeds == null)
            return null;

        float health = Mathf.Clamp(petNeeds.Health, 0f, 100f);
        float hunger = Mathf.Clamp(petNeeds.Hunger, 0f, 100f);
        float energy = Mathf.Clamp(petNeeds.Energy, 0f, 100f);
        float hygiene = Mathf.Clamp(petNeeds.Hygiene, 0f, 100f);
        float happiness = Mathf.Clamp(petNeeds.Happiness, 0f, 100f);

        float healthLow = pixieDecisionConfig != null ? pixieDecisionConfig.HealthLowThreshold : PixieDecisionConfig.DefaultHealthLowThreshold;
        float hungerCritical = pixieDecisionConfig != null ? pixieDecisionConfig.HungerCriticalThreshold : PixieDecisionConfig.DefaultHungerCriticalThreshold;
        float hungerLow = pixieDecisionConfig != null ? pixieDecisionConfig.HungerLowThreshold : PixieDecisionConfig.DefaultHungerLowThreshold;
        float energyCritical = pixieDecisionConfig != null ? pixieDecisionConfig.EnergyCriticalThreshold : PixieDecisionConfig.DefaultEnergyCriticalThreshold;
        float energyLow = pixieDecisionConfig != null ? pixieDecisionConfig.EnergyLowThreshold : PixieDecisionConfig.DefaultEnergyLowThreshold;
        float energyGate = pixieDecisionConfig != null ? pixieDecisionConfig.EnergyGateThreshold : PixieDecisionConfig.DefaultEnergyGateThreshold;
        float hygieneLow = pixieDecisionConfig != null ? pixieDecisionConfig.HygieneLowThreshold : PixieDecisionConfig.DefaultHygieneLowThreshold;
        float happinessLow = pixieDecisionConfig != null ? pixieDecisionConfig.HappinessLowThreshold : PixieDecisionConfig.DefaultHappinessLowThreshold;

        if (health <= healthLow)
        {
            value = health;
            reason = "Low Health is the most dangerous because it puts your pet closest to failing.";
            return "Health";
        }

        if (hunger <= hungerCritical)
        {
            value = hunger;
            reason = "Critical Hunger can snowball into slower movement and more Health pressure.";
            return "Hunger";
        }

        if (hunger <= hungerLow)
        {
            value = hunger;
            reason = "Hunger is lower than it should be, so stabilize food before less urgent bars.";
            return "Hunger";
        }

        if (energy <= energyCritical && energy < hunger)
        {
            value = energy;
            reason = "Energy is critically low, so sleep or recover it before taking on more level pressure.";
            return "Energy";
        }

        if (energy < energyGate && energy <= energyLow)
        {
            value = energy;
            reason = "Energy is low enough to block levels under 50%, so bring it back up soon.";
            return "Energy";
        }

        if (hygiene <= hygieneLow)
        {
            value = hygiene;
            reason = "Hygiene is your weakest remaining bar and can be fixed for free with a bath.";
            return "Hygiene";
        }

        if (happiness <= happinessLow)
        {
            value = happiness;
            reason = "Happiness is the weakest remaining bar right now.";
            return "Happiness";
        }

        return null;
    }

    int CountMentionedNeeds(string q)
    {
        int count = 0;
        if (q.Contains("health")) count++;
        if (q.Contains("energy")) count++;
        if (q.Contains("hunger")) count++;
        if (q.Contains("hygiene")) count++;
        if (q.Contains("happiness")) count++;
        return count;
    }

    string FormatNeedPercent(float value)
    {
        return Mathf.RoundToInt(Mathf.Clamp(value, 0f, 100f)) + "%";
    }

    bool LooksLikeNonGameReply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) return true;
        string r = reply.ToLowerInvariant();
        // Hallucination guard for obvious non-game patterns we saw.
        return r.Contains("real life") ||
               r.Contains("common gameplay mechanics") ||
               r.Contains("check the inventory") ||
               r.Contains("forage for wild") ||
               r.Contains("wild apples nearby") ||
               r.Contains("purchase some apples with your in-game currency");
    }

    void RefreshActiveChoresIfNeeded()
    {
        PruneInvalidActiveChores();
        if (pendingChoreIds.Count == 0 && carryoverChoreIds.Count == 0)
            LoadChoreBacklog();
        if (activeChores.Count >= 3) return;
        if (TryGetChoreCooldownRemaining(out _)) return;

        if (activeChores.Count == 0 && pendingChoreIds.Count == 0)
            EnsureNextChoreBatchReady();

        int now = NowUnix();
        int safety = 0;
        while (activeChores.Count < 3 && pendingChoreIds.Count > 0 && safety++ < 64)
        {
            string id = pendingChoreIds[0];
            pendingChoreIds.RemoveAt(0);
            ChoreDefinition def = FindChoreDefinition(id);
            if (def == null || string.IsNullOrWhiteSpace(def.id))
                continue;

            PlayerPrefs.SetInt(ChoreLastAssignedUnixPrefix + def.id, now);
            PlayerPrefs.SetString(ChoreLastAssignedTaskKey, def.id);
            activeChores.Add(new ChoreEntry { id = def.id, text = def.text, reward = def.reward, assignedAtUnix = now });
        }

        SaveChoreBacklog();
        SaveActiveChores();
        PlayerPrefs.Save();
    }

    List<ChoreDefinition> BuildCandidateChores()
    {
        return new List<ChoreDefinition>
        {
            new ChoreDefinition("keep_energy_80_5m", "Keep energy above 80% for 5 minutes", 3),
            new ChoreDefinition("complete_level_3", "Complete Level 3", 3),
            new ChoreDefinition("talk_ai_3", "Talk to the AI 3 times", 4),
            new ChoreDefinition("go_to_sleep", "Go to sleep", 2),
            new ChoreDefinition("buy_any_item", "Buy any item from the shop", 2),
            new ChoreDefinition("perfect_day_5m", "Perfect Day: keep all stats over 75% for 5 minutes", 6),
            new ChoreDefinition("take_a_bath", "Take a bath", 2),
            new ChoreDefinition("complete_level_1", "Complete Level 1", 3),
            new ChoreDefinition("last_second_save", "Last Second Save", 3),
            new ChoreDefinition("recover_health_low_to_full", "Recover your pet from 25% health to full", 8),
            new ChoreDefinition("feed_pet_3", "Feed your pet 3 times", 3),
            new ChoreDefinition("talk_ai_1", "Talk to the AI once", 2),
            new ChoreDefinition("complete_level_4", "Complete Level 4", 3),
            new ChoreDefinition("keep_happiness_80_5m", "Keep happiness above 80% for 5 minutes", 4),
            new ChoreDefinition("buy_special_effect_potion", "Buy a special effect potion", 3),
            new ChoreDefinition("complete_level_2", "Complete Level 2", 3)
        };
    }

    void PruneInvalidActiveChores()
    {
        for (int i = activeChores.Count - 1; i >= 0; i--)
        {
            ChoreEntry c = activeChores[i];
            if (c == null || string.IsNullOrWhiteSpace(c.id) || FindChoreDefinition(c.id) == null)
                activeChores.RemoveAt(i);
        }
    }

    bool HasActiveChore(string id)
    {
        for (int i = 0; i < activeChores.Count; i++)
            if (string.Equals(activeChores[i].id, id, StringComparison.Ordinal))
                return true;
        return false;
    }

    void RemoveActiveChore(string id)
    {
        for (int i = activeChores.Count - 1; i >= 0; i--)
            if (string.Equals(activeChores[i].id, id, StringComparison.Ordinal))
                activeChores.RemoveAt(i);
    }

    void LoadActiveChores()
    {
        activeChores.Clear();
        string json = PlayerPrefs.GetString(ActiveChoresKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json)) return;

        ActiveChoreDataList data = JsonUtility.FromJson<ActiveChoreDataList>(json);
        if (data == null || data.chores == null) return;

        for (int i = 0; i < data.chores.Count; i++)
        {
            ActiveChoreData c = data.chores[i];
            if (c == null || string.IsNullOrWhiteSpace(c.id)) continue;
            if (FindChoreDefinition(c.id) == null) continue;
            activeChores.Add(new ChoreEntry { id = c.id, text = c.text, reward = c.reward, assignedAtUnix = c.assignedAtUnix > 0 ? c.assignedAtUnix : NowUnix() });
        }

        if (activeChores.Count > 3)
        {
            LoadChoreBacklog();
            List<string> overflowIds = new List<string>();
            for (int i = 3; i < activeChores.Count; i++)
            {
                ChoreEntry c = activeChores[i];
                if (c != null && !string.IsNullOrWhiteSpace(c.id))
                    overflowIds.Add(c.id);
            }

            for (int i = overflowIds.Count - 1; i >= 0; i--)
            {
                string id = overflowIds[i];
                if (!pendingChoreIds.Contains(id))
                    pendingChoreIds.Insert(0, id);
            }

            activeChores.RemoveRange(3, activeChores.Count - 3);
            SaveChoreBacklog();
            SaveActiveChores();
        }
    }

    void SaveActiveChores()
    {
        ActiveChoreDataList data = new ActiveChoreDataList();
        for (int i = 0; i < activeChores.Count; i++)
        {
            ChoreEntry c = activeChores[i];
            if (c == null || string.IsNullOrWhiteSpace(c.id)) continue;
            data.chores.Add(new ActiveChoreData { id = c.id, text = c.text, reward = c.reward, assignedAtUnix = c.assignedAtUnix });
        }

        PlayerPrefs.SetString(ActiveChoresKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    void EnsureNextChoreBatchReady()
    {
        LoadChoreBacklog();
        if (pendingChoreIds.Count > 0)
            return;

        if (carryoverChoreIds.Count == 0)
        {
            List<ChoreDefinition> rotation = BuildSortedChoreRotation(new HashSet<string>());
            for (int i = 0; i < rotation.Count; i++)
                carryoverChoreIds.Add(rotation[i].id);
        }

        while (pendingChoreIds.Count < 10 && carryoverChoreIds.Count > 0)
        {
            pendingChoreIds.Add(carryoverChoreIds[0]);
            carryoverChoreIds.RemoveAt(0);
        }

        if (pendingChoreIds.Count < 10)
        {
            HashSet<string> exclude = new HashSet<string>(pendingChoreIds);
            List<ChoreDefinition> refill = BuildSortedChoreRotation(exclude);
            int take = Mathf.Min(10 - pendingChoreIds.Count, refill.Count);
            for (int i = 0; i < take; i++)
                pendingChoreIds.Add(refill[i].id);
            for (int i = take; i < refill.Count; i++)
                carryoverChoreIds.Add(refill[i].id);
        }

        SaveChoreBacklog();
    }

    List<ChoreDefinition> BuildSortedChoreRotation(HashSet<string> exclude)
    {
        List<ChoreDefinition> defs = BuildCandidateChores();
        if (exclude != null && exclude.Count > 0)
            defs.RemoveAll(d => exclude.Contains(d.id));

        Dictionary<string, int> tieBreak = new Dictionary<string, int>();
        for (int i = 0; i < defs.Count; i++)
            tieBreak[defs[i].id] = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

        defs.Sort((a, b) =>
        {
            int lastA = PlayerPrefs.GetInt(ChoreLastAssignedUnixPrefix + a.id, 0);
            int lastB = PlayerPrefs.GetInt(ChoreLastAssignedUnixPrefix + b.id, 0);
            if (lastA != lastB)
                return lastA.CompareTo(lastB);
            return tieBreak[a.id].CompareTo(tieBreak[b.id]);
        });

        string lastAssignedTaskId = PlayerPrefs.GetString(ChoreLastAssignedTaskKey, string.Empty);
        if (defs.Count > 1 && string.Equals(defs[0].id, lastAssignedTaskId, StringComparison.Ordinal))
        {
            ChoreDefinition first = defs[0];
            defs.RemoveAt(0);
            defs.Add(first);
        }

        return defs;
    }

    ChoreDefinition FindChoreDefinition(string id)
    {
        List<ChoreDefinition> defs = BuildCandidateChores();
        for (int i = 0; i < defs.Count; i++)
            if (string.Equals(defs[i].id, id, StringComparison.Ordinal))
                return defs[i];
        return null;
    }

    void LoadChoreBacklog()
    {
        pendingChoreIds.Clear();
        carryoverChoreIds.Clear();
        string json = PlayerPrefs.GetString(ChoreBacklogKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return;

        ChoreBacklogData data = JsonUtility.FromJson<ChoreBacklogData>(json);
        if (data == null)
            return;

        if (data.batchIds != null)
        {
            for (int i = 0; i < data.batchIds.Count; i++)
            {
                string id = data.batchIds[i];
                if (!string.IsNullOrWhiteSpace(id) && FindChoreDefinition(id) != null)
                    pendingChoreIds.Add(id);
            }
        }

        if (data.carryoverIds != null)
        {
            for (int i = 0; i < data.carryoverIds.Count; i++)
            {
                string id = data.carryoverIds[i];
                if (!string.IsNullOrWhiteSpace(id) && FindChoreDefinition(id) != null)
                    carryoverChoreIds.Add(id);
            }
        }
    }

    void SaveChoreBacklog()
    {
        ChoreBacklogData data = new ChoreBacklogData();
        for (int i = 0; i < pendingChoreIds.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(pendingChoreIds[i]))
                data.batchIds.Add(pendingChoreIds[i]);
        }
        for (int i = 0; i < carryoverChoreIds.Count; i++)
        {
            if (!string.IsNullOrWhiteSpace(carryoverChoreIds[i]))
                data.carryoverIds.Add(carryoverChoreIds[i]);
        }
        PlayerPrefs.SetString(ChoreBacklogKey, JsonUtility.ToJson(data));
    }

    void StartChoreCooldown()
    {
        string until = DateTime.UtcNow.AddSeconds(Mathf.Max(1f, choreCooldownSeconds)).ToString("O");
        PlayerPrefs.SetString(ChoreCooldownUntilKey, until);
        PlayerPrefs.Save();
    }

    int NowUnix()
    {
        long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (unix > int.MaxValue) return int.MaxValue;
        if (unix < int.MinValue) return int.MinValue;
        return (int)unix;
    }

    bool HasReachedContinuousDuration(int startUnix, int assignedAtUnix, int requiredSeconds)
    {
        if (startUnix <= 0)
            return false;
        int effectiveStart = Mathf.Max(startUnix, assignedAtUnix);
        return NowUnix() - effectiveStart >= requiredSeconds;
    }

    void UpdateContinuousThreshold(ref int startUnix, bool condition, int nowUnix)
    {
        if (condition)
        {
            if (startUnix <= 0)
                startUnix = nowUnix;
        }
        else
        {
            startUnix = 0;
        }
    }

    int CountPurchasesSince(int sinceUnix)
    {
        List<PixiePurchaseEntry> entries = PixiePurchaseMemory.GetPurchaseLog(200);
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (TryParseIsoUnix(entries[i].isoTime, out int whenUnix) && whenUnix >= sinceUnix)
                count += Mathf.Max(1, entries[i].quantity);
        }
        return count;
    }

    int CountFoodPurchasesSince(int sinceUnix)
    {
        List<PixiePurchaseEntry> entries = PixiePurchaseMemory.GetPurchaseLog(200);
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (!TryParseIsoUnix(entries[i].isoTime, out int whenUnix) || whenUnix < sinceUnix)
                continue;
            string item = (entries[i].item ?? string.Empty).ToLowerInvariant();
            if (item.Contains("kiwi") || item.Contains("pineapple"))
                count += Mathf.Max(1, entries[i].quantity);
        }
        return count;
    }

    int CountSpecialEffectPurchasesSince(int sinceUnix)
    {
        List<PixiePurchaseEntry> entries = PixiePurchaseMemory.GetPurchaseLog(200);
        int count = 0;
        for (int i = 0; i < entries.Count; i++)
        {
            if (!TryParseIsoUnix(entries[i].isoTime, out int whenUnix) || whenUnix < sinceUnix)
                continue;
            string item = (entries[i].item ?? string.Empty).ToLowerInvariant();
            if (item.Contains("jump boost") || item.Contains("speed boost"))
                count += Mathf.Max(1, entries[i].quantity);
        }
        return count;
    }

    bool TryParseIsoUnix(string isoTime, out int unix)
    {
        unix = 0;
        if (!DateTime.TryParse(isoTime, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime dt))
            return false;

        long value = new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeSeconds();
        if (value > int.MaxValue) unix = int.MaxValue;
        else if (value < int.MinValue) unix = int.MinValue;
        else unix = (int)value;
        return true;
    }

    bool TryHandleLearningCommand(string userInput, out string reply)
    {
        reply = null;
        if (string.IsNullOrWhiteSpace(userInput)) return false;

        string input = userInput.Trim();
        string lower = input.ToLowerInvariant();
        const string savePrefix = "save this into your computer:";
        const string rememberPrefix = "remember this:";

        string payload = null;
        if (lower.StartsWith(savePrefix)) payload = input.Substring(savePrefix.Length).Trim();
        else if (lower.StartsWith(rememberPrefix)) payload = input.Substring(rememberPrefix.Length).Trim();

        if (string.IsNullOrWhiteSpace(payload)) return false;

        string key = null;
        string value = null;
        Match kv = Regex.Match(payload, "^\\s*([^:=\\-]{2,60})\\s*[:=-]\\s*(.+)\\s*$");
        if (kv.Success)
        {
            key = kv.Groups[1].Value.Trim();
            value = kv.Groups[2].Value.Trim();
        }
        else
        {
            string compact = Regex.Replace(payload, "\\s+", " ").Trim();
            string[] words = compact.Split(' ');
            int take = Mathf.Clamp(words.Length, 1, 6);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < take; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(words[i]);
            }
            key = sb.ToString();
            value = payload;
        }

        SaveLearnedFact(key, value);
        reply = Warmify("Saved. I will remember: " + key + " -> " + value);
        return true;
    }

    void SaveLearnedFact(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value)) return;

        string cleanKey = Regex.Replace(key.Trim().ToLowerInvariant(), "\\s+", " ");
        string cleanValue = value.Trim();

        for (int i = 0; i < learnedFacts.Count; i++)
        {
            LearnedFactData f = learnedFacts[i];
            if (f == null || string.IsNullOrWhiteSpace(f.key)) continue;
            if (!string.Equals(f.key.Trim().ToLowerInvariant(), cleanKey, StringComparison.Ordinal)) continue;
            f.value = cleanValue;
            f.savedAtUtcIso = DateTime.UtcNow.ToString("o");
            SaveLongTermMemory();
            return;
        }

        learnedFacts.Add(new LearnedFactData
        {
            key = cleanKey,
            value = cleanValue,
            savedAtUtcIso = DateTime.UtcNow.ToString("o")
        });

        if (learnedFacts.Count > 400)
            learnedFacts.RemoveRange(0, learnedFacts.Count - 400);

        SaveLongTermMemory();
    }

    bool TryGetLearnedFactReply(string userInput, out string reply)
    {
        reply = null;
        if (string.IsNullOrWhiteSpace(userInput) || learnedFacts.Count == 0) return false;
        string q = userInput.Trim().ToLowerInvariant();

        for (int i = learnedFacts.Count - 1; i >= 0; i--)
        {
            LearnedFactData f = learnedFacts[i];
            if (f == null || string.IsNullOrWhiteSpace(f.key) || string.IsNullOrWhiteSpace(f.value)) continue;
            string key = f.key.Trim().ToLowerInvariant();
            if (q.Contains(key) || key.Contains(q))
            {
                reply = Warmify("From what you taught me: " + f.value);
                return true;
            }
        }

        return false;
    }

    bool TryBuildPetAgeReply(string userInput, out string reply)
    {
        reply = null;
        if (string.IsNullOrWhiteSpace(userInput)) return false;
        string q = userInput.ToLowerInvariant();
        if (!(q.Contains("how old") || q.Contains("age")) || !q.Contains("pet")) return false;

        if (!TryGetPetAge(out string age))
        {
            reply = Warmify("I do not have your pet pick date yet. Ask me to save it like: save this into your computer: pet picked date: 2026-03-03");
            return true;
        }

        reply = Warmify("Your pet is " + age + " old since you picked it.");
        return true;
    }

    bool TryGetPetAge(out string age)
    {
        age = string.Empty;
        if (string.IsNullOrWhiteSpace(petPickedUtcIso)) return false;
        if (!DateTime.TryParse(petPickedUtcIso, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime pickedUtc)) return false;

        TimeSpan span = DateTime.UtcNow - pickedUtc;
        if (span.TotalSeconds < 0) span = TimeSpan.Zero;
        int days = Mathf.Max(0, Mathf.FloorToInt((float)span.TotalDays));
        int hours = Mathf.Max(0, span.Hours);
        int mins = Mathf.Max(0, span.Minutes);

        if (days > 0) age = days + " day" + (days == 1 ? "" : "s") + ", " + hours + " hour" + (hours == 1 ? "" : "s");
        else if (hours > 0) age = hours + " hour" + (hours == 1 ? "" : "s") + ", " + mins + " minute" + (mins == 1 ? "" : "s");
        else age = Mathf.Max(1, mins) + " minute" + (mins == 1 ? "" : "s");

        return true;
    }

    bool IsGameRelatedQuestion(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput)) return true;
        string q = userInput.ToLowerInvariant();

        string[] gameWords =
        {
            "pet", "pixie", "apple", "appies", "coin", "currency", "level", "tutorial",
            "hunger", "energy", "hygiene", "health", "happiness", "bar", "bed", "shower",
            "shop", "kiwi", "pineapple", "potion", "hospital", "house", "chore", "quest",
            "boost", "timer", "time left", "remaining time", "jump boost", "speed boost",
            "grow", "progress", "save", "cost", "buy", "bought", "walk", "jump", "controls",
            "restart", "reset", "start over", "quit", "exit", "close game",
            "error", "bug", "stuck", "chatbot", "analyze", "analyse"
        };

        for (int i = 0; i < gameWords.Length; i++)
            if (q.Contains(gameWords[i]))
                return true;

        for (int i = 0; i < learnedFacts.Count; i++)
        {
            LearnedFactData f = learnedFacts[i];
            if (f == null || string.IsNullOrWhiteSpace(f.key)) continue;
            if (q.Contains(f.key.ToLowerInvariant())) return true;
        }

        return false;
    }

    string Warmify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "I am here with you.";
        string trimmed = text.Trim();
        string lower = trimmed.ToLowerInvariant();
        if (lower.StartsWith("hey") || lower.StartsWith("okay") || lower.StartsWith("i ") || lower.StartsWith("you "))
            return trimmed;
        return "Hey, " + trimmed;
    }

    string GetLongTermMemoryPath()
    {
        string folder = Path.Combine(Application.persistentDataPath, LongTermMemoryFolder);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        return Path.Combine(folder, LongTermMemoryFile);
    }

    void LoadLongTermMemory()
    {
        learnedFacts.Clear();
        petPickedUtcIso = string.Empty;
        try
        {
            string path = GetLongTermMemoryPath();
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json)) return;
            LongTermMemoryData data = JsonUtility.FromJson<LongTermMemoryData>(json);
            if (data == null) return;

            petPickedUtcIso = data.petPickedUtcIso ?? string.Empty;
            if (data.facts != null)
            {
                for (int i = 0; i < data.facts.Count; i++)
                {
                    LearnedFactData f = data.facts[i];
                    if (f == null || string.IsNullOrWhiteSpace(f.key) || string.IsNullOrWhiteSpace(f.value)) continue;
                    learnedFacts.Add(new LearnedFactData { key = f.key, value = f.value, savedAtUtcIso = f.savedAtUtcIso });
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GamesChatBot] Long-term memory load failed: " + ex.Message);
        }
    }

    void SaveLongTermMemory()
    {
        try
        {
            LongTermMemoryData data = new LongTermMemoryData();
            data.petPickedUtcIso = petPickedUtcIso;
            data.facts = new List<LearnedFactData>(learnedFacts.Count);
            for (int i = 0; i < learnedFacts.Count; i++)
            {
                LearnedFactData f = learnedFacts[i];
                if (f == null || string.IsNullOrWhiteSpace(f.key) || string.IsNullOrWhiteSpace(f.value)) continue;
                data.facts.Add(new LearnedFactData { key = f.key, value = f.value, savedAtUtcIso = f.savedAtUtcIso });
            }

            string path = GetLongTermMemoryPath();
            File.WriteAllText(path, JsonUtility.ToJson(data, true), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GamesChatBot] Long-term memory save failed: " + ex.Message);
        }
    }

    void EnsurePetPickedTime()
    {
        if (!string.IsNullOrWhiteSpace(petPickedUtcIso)) return;

        for (int i = 0; i < learnedFacts.Count; i++)
        {
            LearnedFactData f = learnedFacts[i];
            if (f == null || string.IsNullOrWhiteSpace(f.key) || string.IsNullOrWhiteSpace(f.value)) continue;
            if (!f.key.Contains("pet picked") && !f.key.Contains("adopted") && !f.key.Contains("picked date")) continue;
            if (DateTime.TryParse(f.value, out DateTime parsed))
            {
                petPickedUtcIso = parsed.ToUniversalTime().ToString("o");
                SaveLongTermMemory();
                return;
            }
        }

        petPickedUtcIso = DateTime.UtcNow.ToString("o");
        SaveLongTermMemory();
    }
}

public static class GamesChatBotBootstrap
{
    static bool installed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoBootstrap()
    {
        if (installed) return;
        installed = true;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) { TryInstall(scene); }

    static void TryInstall(Scene scene)
    {
        bool allowed =
            string.Equals(scene.name, "House", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scene.name, "Hospital", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(scene.name, "Tutorial", StringComparison.OrdinalIgnoreCase);
        if (!allowed) return;
        if (UnityEngine.Object.FindFirstObjectByType<GamesChatBot>() != null) return;
        new GameObject("GamesChatBotUIManager").AddComponent<GamesChatBot>();
    }
}



