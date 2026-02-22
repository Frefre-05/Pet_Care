using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
}

public static class GamesChatBotStats
{
    const string TotalSpentKey = "CHATBOT_TOTAL_APPLES_SPENT";
    const string TotalBoughtKey = "CHATBOT_TOTAL_ITEMS_BOUGHT";
    static string BoughtKey(string itemId) => "CHATBOT_BOUGHT_" + itemId;

    public static int TotalSpent => PlayerPrefs.GetInt(TotalSpentKey, 0);
    public static int TotalBought => PlayerPrefs.GetInt(TotalBoughtKey, 0);
    public static int GetBoughtCount(string itemId) => PlayerPrefs.GetInt(BoughtKey(itemId), 0);
}

[DisallowMultipleComponent]
public class GamesChatBot : MonoBehaviour
{
    const string MemoryKey = "PIXIE_AI_CHAT_MEMORY_V3";
    const string LastAdvicePrefix = "PIXIE_AI_LAST_ADVICE_";
    const string ChoreClaimPrefix = "PIXIE_AI_CHORE_LASTCLAIM_";
    const string ChoreDonePrefix = "PIXIE_AI_CHORE_DONE_";
    const string ActiveChoresKey = "PIXIE_AI_ACTIVE_CHORES_V1";
    const string LastBedUseKey = "PIXIE_AI_LAST_BED_USE_UTC";
    const string LastShowerUseKey = "PIXIE_AI_LAST_SHOWER_USE_UTC";
    const string HouseIntroSeenKey = "PIXIE_AI_HOUSE_INTRO_SEEN_V1";

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
    [SerializeField] string[] allowedScenes = { "House", "Hospital" };
    [SerializeField] float inputMinHeight = 28f;
    [SerializeField] float inputMaxHeight = 120f;
    [SerializeField] float typingCharsPerSecond = 55f;
    [SerializeField] bool enableFirstHouseIntroPopup = true;
    [SerializeField] bool pauseGameDuringHouseIntro = true;
    [SerializeField] bool keepHouseIntroCentered = true;
    [SerializeField] Rect houseIntroWindowRect = new Rect(0f, 0f, 760f, 390f);
    [SerializeField] float introTypingCharsPerSecond = 22f;
    [SerializeField] int houseIntroFontSize = 26;
    [TextArea(4, 8)] [SerializeField] string houseIntroParagraph =
        "Welcome to your House. This is your safe place to take care of your pet: the bed restores Energy, the bath restores Hygiene, and your Levels menu in the top-right includes Pet Care where you buy important items like Kiwi, Pineapple, and Health Potion. If your Energy is below 50%, levels stay locked until you rest. Keep an eye on your bars and recover low needs early. Open your Pixie AI chatbot with G, and close it with Escape. Thank you, and enjoy playing.";

    [Header("Gameplay")]
    [SerializeField] bool persistConversationAcrossSessions = true;
    [SerializeField] bool forceGameOnlyResponses = true;
    [SerializeField] float lowNeedThreshold = 35f;
    [SerializeField] float alertCooldownSeconds = 20f;
    [SerializeField] float choreCooldownSeconds = 120f;
    [SerializeField] ShopButtons shopButtons;
    [SerializeField] PetNeeds petNeeds;

    [Header("Recommendation Tuning")]
    [SerializeField] string[] recommendationLevelNames = { "Level 1", "Level 2", "Level 3", "Level 4" };
    [SerializeField] int[] recommendationLevelAppleEstimates = { 3, 5, 8, 12 };

    readonly List<ChatLine> chatLines = new List<ChatLine>();
    readonly List<OllamaMessage> history = new List<OllamaMessage>();
    readonly List<ItemDef> itemDefs = new List<ItemDef>();
    readonly List<string> pendingAlerts = new List<string>();
    readonly List<ChoreEntry> activeChores = new List<ChoreEntry>();

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
    float preIntroTimeScale = 1f;
    Rect preIntroWindowRect;
    bool hasPreIntroWindowRect;
    int introVisibleChars;
    float introTypingProgress;

    GUIStyle chatLineStyle, inputAreaStyle, statusStyle, introParagraphStyle;
    const string WarningColorHex = "#FFD54A";

    struct ItemDef
    {
        public string id, display;
        public int cost;
        public string[] aliases;
        public ItemDef(string id, string display, int cost, params string[] aliases)
        { this.id = id; this.display = display; this.cost = cost; this.aliases = aliases; }
    }

    class ChatLine
    {
        public string fullText;
        public int visibleChars;
        public float createdAt;
        public string VisibleText => string.IsNullOrEmpty(fullText) ? string.Empty : fullText.Substring(0, Mathf.Clamp(visibleChars, 0, fullText.Length));
    }

    class ChoreEntry { public string id; public string text; public int reward; public int assignedAtUnix; }
    [Serializable] class ActiveChoreData { public string id; public string text; public int reward; public int assignedAtUnix; }
    [Serializable] class ActiveChoreDataList { public List<ActiveChoreData> chores = new List<ActiveChoreData>(); }

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
        LoadActiveChores();
        LoadMemory();
        RefreshActiveChoresIfNeeded();
        SyncNeedStateImmediate();
        TryStartHouseIntro();
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; SaveMemory(); }
    void OnApplicationQuit() { SaveMemory(); }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        isOpen = false;
        showHouseIntro = false;
        waitingForModel = false;
        if (requestRoutine != null) StopCoroutine(requestRoutine);
        requestRoutine = null;
        typingLineIndex = -1;
        RefreshReferences();
        RebuildItemDefs();
        RefreshActiveChoresIfNeeded();
        SyncNeedStateImmediate();
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
        }
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
        string intro = houseIntroParagraph ?? string.Empty;
        if (introVisibleChars >= intro.Length) return;
        introTypingProgress += Mathf.Max(1f, introTypingCharsPerSecond) * Time.unscaledDeltaTime;
        introVisibleChars = Mathf.Clamp(Mathf.FloorToInt(introTypingProgress), 0, intro.Length);
    }

    void OpenChat()
    {
        if (isOpen) return;
        isOpen = true;
        if (showHouseIntro) return;
        if (chatLines.Count == 0)
        {
            AppendLineImmediate("Pixie AI: Ask about purchases, savings goals, costs, bars, and chores.");
            AppendLineImmediate("Pixie AI: " + BuildNeedRecommendation());
        }
        for (int i = 0; i < pendingAlerts.Count; i++) AppendLineTyped(FormatWarningLine(pendingAlerts[i]));
        pendingAlerts.Clear();
        GUI.FocusControl("GamesChatBotInput");
    }

    static string FormatWarningLine(string warningText)
    {
        return "<color=" + WarningColorHex + ">Pixie AI alert: " + warningText + "</color>";
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
        RefreshReferences();
        RebuildItemDefs();
        AppendLineImmediate("You: " + userInput);

        if (TryBuildGroundTruthReply(userInput, out string strictReply))
        {
            history.Add(new OllamaMessage { role = "user", content = userInput });
            history.Add(new OllamaMessage { role = "assistant", content = strictReply });
            TrimHistory();
            AppendLineTyped("Pixie AI: " + strictReply);
            SaveMemory();
            return;
        }

        if (requestRoutine != null) StopCoroutine(requestRoutine);
        requestRoutine = StartCoroutine(RequestOllamaReply(userInput));
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
            else if (forceGameOnlyResponses && LooksLikeNonGameReply(reply))
                reply = BuildLocalResponse(userInput);

            history.Add(new OllamaMessage { role = "user", content = userInput });
            history.Add(new OllamaMessage { role = "assistant", content = reply });
            TrimHistory();
            AppendLineTyped("Pixie AI: " + reply);
            SaveMemory();
        }

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
                      "- Be specific and avoid repeated lines.\n" +
                      "- If asked last purchase, use purchase memory exactly.\n" +
                      "- If asked savings, suggest amount and item priorities.\n" +
                      "- Never invent game mechanics or controls.\n" +
                      "- If data is unknown, say you do not know yet."
        });

        int window = Mathf.Max(0, maxHistoryMessages) * 2;
        int start = window <= 0 ? history.Count : Mathf.Max(0, history.Count - window);
        for (int i = start; i < history.Count; i++) messages.Add(history[i]);
        messages.Add(new OllamaMessage { role = "user", content = userInput });
        return messages;
    }

    string BuildGameSnapshot()
    {
        StringBuilder sb = new StringBuilder();
        string scene = SceneManager.GetActiveScene().name;
        sb.AppendLine("Scene: " + scene);
        sb.AppendLine("Apples: " + AppleCurrency.Get());
        sb.AppendLine("Total spent apples: " + GamesChatBotStats.TotalSpent);
        sb.AppendLine("Total items bought: " + GamesChatBotStats.TotalBought);
        sb.AppendLine(PixiePurchaseMemory.GetLastPurchaseSummary());
        sb.AppendLine("Recent purchases: " + PixiePurchaseMemory.GetPurchaseLogSummary(8));

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
        CheckSingleNeedAlert("Critical Hunger", petNeeds.Hunger <= 10f, "Hunger is critical. Recover it soon.", ref criticalHungerPrev);
        CheckSingleNeedAlert("Critical Health", petNeeds.Health <= 10f, "Health is critical. Recover it soon.", ref criticalHealthPrev);
    }

    void CheckSingleNeedAlert(string needName, bool isLowNow, string message, ref bool wasLow)
    {
        if (isLowNow && !wasLow && Time.unscaledTime >= nextAlertAt)
        {
            nextAlertAt = Time.unscaledTime + Mathf.Max(1f, alertCooldownSeconds);
            string text = needName + " warning: " + message;
            if (isOpen) AppendLineTyped(FormatWarningLine(text));
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
        if (showHouseIntro && keepHouseIntroCentered)
        {
            houseIntroWindowRect.x = (Screen.width - houseIntroWindowRect.width) * 0.5f;
            houseIntroWindowRect.y = (Screen.height - houseIntroWindowRect.height) * 0.5f;
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
            string intro = houseIntroParagraph ?? string.Empty;
            int count = Mathf.Clamp(introVisibleChars, 0, intro.Length);
            GUILayout.Label(intro.Substring(0, count), introParagraphStyle);
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
        GUILayout.Label(waitingForModel ? "Pixie AI is thinking..." : "Press G to toggle", statusStyle);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0f, 0f, windowRect.width, 22f));
    }
    void DrawChoresPanel()
    {
        List<ChoreEntry> chores = BuildChoreEntries();
        Rect r = new Rect(Screen.width - 460f, 20f, 440f, 260f);
        DrawSolidRect(r, new Color(0f, 0f, 0f, 0.78f));
        GUI.Box(r, "Pixie AI Chores");

        GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 28f, r.width - 24f, r.height - 36f));
        if (chores.Count == 0)
        {
            GUILayout.Label("No available quests right now.", chatLineStyle);
            GUILayout.EndArea();
            return;
        }

        for (int i = 0; i < chores.Count; i++)
        {
            ChoreEntry c = chores[i];
            GUILayout.BeginHorizontal();
            GUILayout.Label((i + 1) + ". " + c.text + " (Reward " + c.reward + " apples)", chatLineStyle, GUILayout.Width(330f));
            bool complete = IsChoreCompleted(c);
            bool canClaim = CanClaimChore(c.id, out int sec) && complete;
            GUI.enabled = canClaim;
            if (GUILayout.Button("Done", GUILayout.Width(78f))) ClaimChore(c);
            GUI.enabled = true;
            GUILayout.EndHorizontal();
            if (!complete) GUILayout.Label("Finish task first.", statusStyle);
            else if (!CanClaimChore(c.id, out sec)) GUILayout.Label("Cooldown: " + sec + "s", statusStyle);
        }
        GUILayout.EndArea();
    }

    List<ChoreEntry> BuildChoreEntries()
    {
        RefreshActiveChoresIfNeeded();
        return activeChores;
    }

    static void AddNeedChore(List<ChoreEntry> chores, string idPrefix, string name, float value, string task)
    {
        if (value > 80f) return;
        if (value <= 30f) chores.Add(new ChoreEntry { id = idPrefix + "_critical", text = name + " critical: " + task, reward = 9 });
        else if (value <= 55f) chores.Add(new ChoreEntry { id = idPrefix + "_low", text = name + " low: " + task, reward = 6 });
        else chores.Add(new ChoreEntry { id = idPrefix + "_dropping", text = name + " dropping: " + task, reward = 3 });
    }

    bool CanClaimChore(string choreId, out int secondsLeft)
    {
        int lastClaim = PlayerPrefs.GetInt(ChoreClaimPrefix + choreId, 0);
        int now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() > int.MaxValue ? int.MaxValue : (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int next = lastClaim + Mathf.Max(1, Mathf.RoundToInt(choreCooldownSeconds));
        if (now >= next) { secondsLeft = 0; return true; }
        secondsLeft = next - now;
        return false;
    }

    void ClaimChore(ChoreEntry chore)
    {
        if (chore == null || WasChoreClaimedEver(chore.id) || !IsChoreCompleted(chore) || !CanClaimChore(chore.id, out _)) return;
        AppleCurrency.Add(chore.reward);
        int now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() > int.MaxValue ? int.MaxValue : (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetInt(ChoreClaimPrefix + chore.id, now);
        PlayerPrefs.SetInt(ChoreDonePrefix + chore.id, 1);
        PlayerPrefs.Save();
        AppendLineTyped("Pixie AI: Chore completed. +" + chore.reward + " apples for \"" + chore.text + "\".");
        RemoveActiveChore(chore.id);
        RefreshActiveChoresIfNeeded();
        SaveActiveChores();
        SaveMemory();
    }

    bool IsChoreCompleted(ChoreEntry chore)
    {
        if (chore == null) return false;
        string id = chore.id ?? string.Empty;

        // Chores that do not require bar bindings should still complete even if petNeeds is not bound yet.
        if (id == "buy_pineapple_once") return GamesChatBotStats.GetBoughtCount(GamesChatBotItemId.BigFood) >= 1;
        if (id == "generic_bath_sleep")
        {
            int gate = chore.assignedAtUnix;
            int lastBed = PlayerPrefs.GetInt(LastBedUseKey, 0);
            int lastShower = PlayerPrefs.GetInt(LastShowerUseKey, 0);
            return lastBed >= gate || lastShower >= gate;
        }
        if (id == "prevent_sleep")
        {
            int gate = chore.assignedAtUnix;
            int lastBed = PlayerPrefs.GetInt(LastBedUseKey, 0);
            return lastBed >= gate;
        }
        if (id == "prevent_shower")
        {
            int gate = chore.assignedAtUnix;
            int lastShower = PlayerPrefs.GetInt(LastShowerUseKey, 0);
            return lastShower >= gate;
        }

        if (petNeeds == null)
        {
            RefreshReferences();
            if (petNeeds == null) return false;
        }

        float minBar = Mathf.Min(petNeeds.Hunger, petNeeds.Energy, petNeeds.Hygiene, petNeeds.Happiness, petNeeds.Health);

        if (id.StartsWith("hunger_", StringComparison.Ordinal)) return petNeeds.Hunger >= 70f;
        if (id.StartsWith("energy_", StringComparison.Ordinal)) return petNeeds.Energy >= 75f;
        if (id.StartsWith("hygiene_", StringComparison.Ordinal)) return petNeeds.Hygiene >= 80f;
        if (id.StartsWith("happiness_", StringComparison.Ordinal)) return petNeeds.Happiness >= 70f;
        if (id.StartsWith("health_", StringComparison.Ordinal)) return petNeeds.Health >= 80f;

        if (id == "stable_all") return minBar >= 75f;
        if (id == "generic_bars" || id.StartsWith("stabilize_extra_", StringComparison.Ordinal)) return minBar >= 70f;
        if (id == "generic_health_up") return petNeeds.Health >= 80f;

        return false;
    }

    string BuildLocalResponse(string query)
    {
        if (TryBuildGroundTruthReply(query, out string strictReply))
            return strictReply;

        string q = query.ToLowerInvariant();
        if (q.Contains("remaining apples") || q.Contains("specific amount") || q.Contains("need") && q.Contains("apples") || q.Contains("for health potion") || q.Contains("recommend") && q.Contains("level"))
            return BuildLevelGrindRecommendation(query);
        if (q.Contains("last purch")) return PixiePurchaseMemory.GetLastPurchaseSummary();
        if (q.Contains("how many") && (q.Contains("bought") || q.Contains("buy"))) return BuildBoughtSummary();
        if (q.Contains("waste") || q.Contains("wasted") || q.Contains("spent")) return "You have spent " + GamesChatBotStats.TotalSpent + " apples total so far.";
        if (q.Contains("cost") || q.Contains("how much") || q.Contains("if i buy")) return BuildCostAnswer(query);
        if (q.Contains("save") || q.Contains("save up") || q.Contains("what should i buy")) return BuildSavingsPlan();
        if (q.Contains("chore") || q.Contains("task")) return BuildChoreResponse();
        if (q.Contains("low") || q.Contains("should buy") || q.Contains("instead")) return BuildNeedRecommendation();
        return BuildNeedRecommendation();
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
        sb.Append(" | Total apples spent: ").Append(GamesChatBotStats.TotalSpent);
        sb.Append(" | ").Append(PixiePurchaseMemory.GetLastPurchaseSummary());
        return sb.ToString();
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
        if (need == 0) return "You already have enough apples. Spend around " + goal + " apples for: " + string.Join(", ", targets) + ".";
        return "Save up at least " + goal + " apples for: " + string.Join(", ", targets) + ". You still need " + need + " more apples.";
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
        bool saysMore = q.Contains("more apples") || q.Contains("remaining apples") || q.Contains("short by") || q.Contains("missing");

        if (target <= 0 && parsedNumber > 0)
        {
            // If user says "I need 12 more apples", treat as shortfall; otherwise treat as total goal.
            target = saysMore ? have + parsedNumber : parsedNumber;
            targetLabel = saysMore ? "your shortfall target" : "your apple target";
        }

        if (target <= 0)
            return "Tell me a target, like: \"How many levels for 20 apples?\" or \"remaining apples for health potion\".";

        int shortfall = Mathf.Max(0, target - have);
        if (shortfall == 0)
            return "You already have enough apples for " + targetLabel + ". You have " + have + ".";

        string plan = BuildLevelPlan(shortfall);
        return "You need " + shortfall + " more apples for " + targetLabel + " (cost/goal " + target + ", you have " + have + "). " + plan;
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
            return "I have no level apple estimates configured yet.";

        levels.Sort((x, y) => y.apples.CompareTo(x.apples)); // best apples first

        int remaining = shortfall;
        List<string> steps = new List<string>();
        for (int i = 0; i < levels.Count; i++)
        {
            int runCount = remaining / levels[i].apples;
            if (runCount <= 0) continue;
            remaining -= runCount * levels[i].apples;
            steps.Add(levels[i].name + " x" + runCount + " (~" + (runCount * levels[i].apples) + " apples)");
        }

        if (remaining > 0)
        {
            // one extra run of the highest-yield level to cover remainder
            steps.Add(levels[0].name + " x1 (~" + levels[0].apples + " apples)");
            remaining = 0;
        }

        // Also provide single-level shortcut when useful (e.g., Level 3 once)
        string bestSingle = string.Empty;
        for (int i = 0; i < levels.Count; i++)
        {
            int runs = Mathf.CeilToInt(shortfall / (float)levels[i].apples);
            int total = runs * levels[i].apples;
            if (string.IsNullOrEmpty(bestSingle) || total < ExtractFirstPositiveInt(bestSingle))
                bestSingle = total + "|" + levels[i].name + " x" + runs + " (~" + total + " apples)";
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
            b.Append(d.display).Append(" x").Append(qty).Append(" = ").Append(c).Append(" apples. ");
        }
        int remaining = AppleCurrency.Get() - total;
        return remaining >= 0 ? b + "Total cost: " + total + ". You will have " + remaining + " apples left." : b + "Total cost: " + total + ". You are short by " + Mathf.Abs(remaining) + " apples.";
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

    string BuildNeedRecommendation()
    {
        string scene = SceneManager.GetActiveScene().name; string advice;
        if (petNeeds == null) advice = "I cannot read bars yet. Spawn player first.";
        else if (petNeeds.Energy < 50f) advice = "Energy is below 50%. Sleep first, then levels will unlock.";
        else if (petNeeds.Hygiene <= 35f) advice = "Hygiene is low. Use the shower.";
        else if (petNeeds.Hunger <= 35f) advice = "Hunger is low. Buy Kiwi first; it is the cheaper food option.";
        else if (petNeeds.Health <= 35f) advice = "Health is low. If hunger is low too, buy Kiwi first; otherwise buy Health Potion.";
        else if (petNeeds.Happiness <= 35f) advice = "Happiness is low. Play a level or do a fun activity.";
        else advice = "Bars are stable right now.";
        PlayerPrefs.SetString(LastAdvicePrefix + scene, advice); PlayerPrefs.Save();
        return advice;
    }

    void TryStartHouseIntro()
    {
        if (!enableFirstHouseIntroPopup) return;
        if (!string.Equals(SceneManager.GetActiveScene().name, "House", StringComparison.OrdinalIgnoreCase)) return;
        if (PlayerPrefs.GetInt(HouseIntroSeenKey, 0) == 1) return;

        preIntroWindowRect = windowRect;
        hasPreIntroWindowRect = true;
        showHouseIntro = true;
        isOpen = true;
        introVisibleChars = 0;
        introTypingProgress = 0f;
        PlayerPrefs.SetInt(HouseIntroSeenKey, 1);
        PlayerPrefs.Save();

        if (pauseGameDuringHouseIntro)
        {
            preIntroTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    string BuildChoreResponse()
    {
        List<ChoreEntry> c = BuildChoreEntries();
        if (c.Count == 0) return "No available quests right now.";
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
        Color prev = GUI.color; GUI.color = new Color(1f, 1f, 1f, alpha);
        GUILayout.BeginHorizontal(); GUILayout.Label(line.VisibleText, chatLineStyle, GUILayout.ExpandWidth(true)); GUILayout.EndHorizontal();
        GUI.color = prev;
    }

    void AppendLineImmediate(string text) { chatLines.Add(new ChatLine { fullText = text, visibleChars = text.Length, createdAt = Time.unscaledTime }); TrimTranscriptLines(); autoScrollToBottom = true; }
    void AppendLineTyped(string text) { chatLines.Add(new ChatLine { fullText = text, visibleChars = 0, createdAt = Time.unscaledTime }); TrimTranscriptLines(); typingLineIndex = chatLines.Count - 1; autoScrollToBottom = true; }

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

    void RebuildItemDefs()
    {
        int hc = shopButtons != null ? shopButtons.healthPotionCost : 20;
        int ec = shopButtons != null ? shopButtons.energyPotionCost : 20;
        int sc = shopButtons != null ? shopButtons.smallFoodCost : 5;
        int bc = shopButtons != null ? shopButtons.bigFoodCost : 20;
        itemDefs.Clear();
        itemDefs.Add(new ItemDef(GamesChatBotItemId.HealthPotion, "Health Potion", hc, "health potion", "potion", "medicine"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.EnergyPotion, "Energy Potion", ec, "energy potion", "energy bar"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.SmallFood, "Kiwi", sc, "kiwi", "small food", "snack"));
        itemDefs.Add(new ItemDef(GamesChatBotItemId.BigFood, "Pineapple", bc, "pineapple", "big food", "meal"));
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

    void EnsureGuiStyles()
    {
        if (chatLineStyle == null) { chatLineStyle = new GUIStyle(GUI.skin.label) { wordWrap = true }; chatLineStyle.margin = new RectOffset(6, 6, 3, 3); chatLineStyle.normal.textColor = Color.white; }
        chatLineStyle.richText = true;
        if (inputAreaStyle == null)
        {
            inputAreaStyle = new GUIStyle(GUI.skin.textArea) { wordWrap = true };
            inputAreaStyle.normal.textColor = Color.white; inputAreaStyle.hover.textColor = Color.white; inputAreaStyle.focused.textColor = Color.white; inputAreaStyle.active.textColor = Color.white;
        }
        if (statusStyle == null) { statusStyle = new GUIStyle(GUI.skin.label); statusStyle.normal.textColor = Color.white; }
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

    bool TryBuildGroundTruthReply(string query, out string reply)
    {
        string q = (query ?? string.Empty).ToLowerInvariant();

        bool asksPriorityScenario =
            (q.Contains("hunger") && q.Contains("25") && q.Contains("energy") && q.Contains("40")) ||
            (q.Contains("before entering level 2") && q.Contains("prioritize")) ||
            (q.Contains("prioritise") && q.Contains("hunger") && q.Contains("energy"));
        if (asksPriorityScenario)
        {
            reply =
                "With Hunger around 25 and Energy around 40, prioritize food first. " +
                "Your best sequence is to earn enough apples for a Kiwi, complete chores and a level run, " +
                "then secure health support, and continue buying Kiwi until Hunger is fully stabilized. " +
                "That order protects survival first, then restores long-term performance.";
            return true;
        }

        bool asksBestLevelForCoins =
            (q.Contains("which level") && (q.Contains("earn coins") || q.Contains("earn apples") || q.Contains("quickly") || q.Contains("buy food"))) ||
            (q.Contains("best level") && (q.Contains("coins") || q.Contains("apples")));
        if (asksBestLevelForCoins)
        {
            reply =
                "Recommended apple yields by stage: Tutorial about 5, Level 1 about 7, Level 2 about 3, " +
                "Level 3 about 7, and Level 4 about 11 apples per completion. " +
                "For fastest food funding, prioritize Level 4 first, then Level 1 or Level 3 as reliable alternatives.";
            return true;
        }

        bool asksLevelEnergyLock = (q.Contains("can't play") || q.Contains("cannot play") || q.Contains("cant play") || q.Contains("level locked") || q.Contains("why level"))
                                   && (q.Contains("energy") || q.Contains("why"));
        if (asksLevelEnergyLock)
        {
            reply = "Levels are blocked when Energy is below 50%. Sleep in your bed to refill Energy, then try again.";
            return true;
        }

        bool asksGrowth = q.Contains("how many apples") && q.Contains("grow") ||
                          q.Contains("need to grow") ||
                          q.Contains("apples to grow") ||
                          q.Contains("how do i grow") ||
                          q.Contains("growth requirement");
        if (asksGrowth)
        {
            reply = "To grow, you need 20 appies and all bars above 70%.";
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

        bool asksApples = q.Contains("how do you get apple") || q.Contains("how to get apple") ||
                          q.Contains("how get apple") || q.Contains("get appies") || q.Contains("get apples") ||
                          q.Contains("where do i get apple") || q.Contains("how do i earn apple") ||
                          q.Contains("how do i get appies") || q.Contains("how to get appies");
        if (asksApples)
        {
            int apples = AppleCurrency.Get();
            reply =
                "You get apples by collecting apple/fruit pickups in levels and by completing Pixie AI chores. " +
                "Apples are your currency for items and level costs. " +
                "Current apples: " + apples + ".";
            return true;
        }

        bool asksAppleSources = q.Contains("where apples") || q.Contains("apple source") || q.Contains("appies source");
        if (asksAppleSources)
        {
            reply = "Apple sources in this game: level fruit pickups and completed chores. No fake inventory/store apple generator.";
            return true;
        }

        if (!asksControls && !asksApples && !asksAppleSources)
        {
            reply = null;
            return false;
        }

        reply = null;
        return false;
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
        if (activeChores.Count >= 3) return;

        List<ChoreEntry> candidates = BuildCandidateChores();
        for (int i = 0; i < candidates.Count && activeChores.Count < 3; i++)
        {
            ChoreEntry c = candidates[i];
            if (c == null || string.IsNullOrWhiteSpace(c.id)) continue;
            if (HasActiveChore(c.id) || WasChoreClaimedEver(c.id)) continue;
            c.assignedAtUnix = NowUnix();
            if (IsChoreCompleted(c)) continue; // do not assign already-completed quests
            activeChores.Add(c);
        }

        SaveActiveChores();
    }

    List<ChoreEntry> BuildCandidateChores()
    {
        List<ChoreEntry> chores = new List<ChoreEntry>();
        if (petNeeds == null)
        {
            chores.Add(new ChoreEntry { id = "generic_bath_sleep", text = "Use bed or shower once", reward = 2 });
            chores.Add(new ChoreEntry { id = "buy_pineapple_once", text = "Buy 1 Pineapple from shop", reward = 3 });
            chores.Add(new ChoreEntry { id = "generic_health_up", text = "Raise health above 80", reward = 3 });
            return chores;
        }

        AddNeedChore(chores, "hunger", "Hunger", petNeeds.Hunger, "Raise hunger above 70");
        AddNeedChore(chores, "energy", "Energy", petNeeds.Energy, "Sleep and raise energy above 75");
        AddNeedChore(chores, "hygiene", "Hygiene", petNeeds.Hygiene, "Shower and raise hygiene above 80");
        AddNeedChore(chores, "happiness", "Happiness", petNeeds.Happiness, "Raise happiness above 70");
        AddNeedChore(chores, "health", "Health", petNeeds.Health, "Raise health above 80");

        chores.Add(new ChoreEntry { id = "stable_all", text = "Keep all bars above 75 for a while", reward = 2 });
        chores.Add(new ChoreEntry { id = "prevent_sleep", text = "Sleep once before next level", reward = 3 });
        chores.Add(new ChoreEntry { id = "prevent_shower", text = "Take a shower to stay clean", reward = 2 });
        chores.Add(new ChoreEntry { id = "generic_bath_sleep", text = "Use bed or shower once", reward = 2 });
        chores.Add(new ChoreEntry { id = "generic_health_up", text = "Raise health above 80", reward = 3 });
        chores.Add(new ChoreEntry { id = "buy_pineapple_once", text = "Buy 1 Pineapple from shop", reward = 3 });

        return chores;
    }

    void PruneInvalidActiveChores()
    {
        for (int i = activeChores.Count - 1; i >= 0; i--)
        {
            ChoreEntry c = activeChores[i];
            // Keep completed chores visible so player can press Done and claim apples.
            if (c == null || string.IsNullOrWhiteSpace(c.id) || WasChoreClaimedEver(c.id) || c.id == "generic_stable")
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

    bool WasChoreClaimedEver(string choreId)
    {
        if (string.IsNullOrWhiteSpace(choreId)) return false;
        return PlayerPrefs.GetInt(ChoreDonePrefix + choreId, 0) == 1;
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
            if (c == null || string.IsNullOrWhiteSpace(c.id) || WasChoreClaimedEver(c.id)) continue;
            activeChores.Add(new ChoreEntry { id = c.id, text = c.text, reward = c.reward, assignedAtUnix = c.assignedAtUnix > 0 ? c.assignedAtUnix : NowUnix() });
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

    int NowUnix()
    {
        long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (unix > int.MaxValue) return int.MaxValue;
        if (unix < int.MinValue) return int.MinValue;
        return (int)unix;
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
        bool allowed = string.Equals(scene.name, "House", StringComparison.OrdinalIgnoreCase) || string.Equals(scene.name, "Hospital", StringComparison.OrdinalIgnoreCase);
        if (!allowed) return;
        if (UnityEngine.Object.FindFirstObjectByType<GamesChatBot>() != null) return;
        new GameObject("GamesChatBotUIManager").AddComponent<GamesChatBot>();
    }
}
