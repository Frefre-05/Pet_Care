using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaPixieClient
{
    const string DefaultEndpoint = "http://localhost:11434/api/generate";

    readonly string endpoint;
    readonly string model;
    readonly int timeoutSeconds;

    public OllamaPixieClient(string modelName, string endpointUrl = DefaultEndpoint, int requestTimeoutSeconds = 45)
    {
        model = string.IsNullOrWhiteSpace(modelName) ? "llama3.2:3b" : modelName;
        endpoint = string.IsNullOrWhiteSpace(endpointUrl) ? DefaultEndpoint : endpointUrl;
        timeoutSeconds = Mathf.Max(1, requestTimeoutSeconds);
    }

    public IEnumerator RequestAdvice(MonoBehaviour runner, string systemPrompt, string userPrompt, Action<PixieResponse, string> onCompleted)
    {
        if (runner == null)
        {
            onCompleted?.Invoke(null, "Pixie client needs a MonoBehaviour runner.");
            yield break;
        }

        OllamaGenerateRequest body = new OllamaGenerateRequest
        {
            model = model,
            prompt = systemPrompt + "\n\n" + userPrompt,
            stream = false,
            format = "json"
        };

        string payload = JsonUtility.ToJson(body);
        using (UnityWebRequest req = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout = timeoutSeconds;
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("[PixieAI] Sending structured advice request to Ollama.");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                string error = "Ollama request failed: " + req.error;
                Debug.LogWarning("[PixieAI] " + error);
                onCompleted?.Invoke(null, error);
                yield break;
            }

            string raw = req.downloadHandler.text;
            Debug.Log("[PixieAI] Ollama response received.");
            OllamaGenerateResponse wrapper = null;
            try
            {
                wrapper = JsonUtility.FromJson<OllamaGenerateResponse>(raw);
            }
            catch (Exception ex)
            {
                string error = "Could not parse Ollama wrapper response: " + ex.Message;
                Debug.LogWarning("[PixieAI] " + error);
                onCompleted?.Invoke(null, error);
                yield break;
            }

            string json = ExtractJson(wrapper != null ? wrapper.response : string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                string error = "Ollama returned an empty Pixie response.";
                Debug.LogWarning("[PixieAI] " + error);
                onCompleted?.Invoke(null, error);
                yield break;
            }

            PixieResponse response = null;
            try
            {
                response = JsonUtility.FromJson<PixieResponse>(json);
            }
            catch (Exception ex)
            {
                string error = "Could not parse Pixie JSON payload: " + ex.Message;
                Debug.LogWarning("[PixieAI] " + error + " Raw payload: " + json);
                onCompleted?.Invoke(null, error);
                yield break;
            }

            onCompleted?.Invoke(response, null);
        }
    }

    static string ExtractJson(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        string trimmed = raw.Trim();
        if (trimmed.StartsWith("```"))
        {
            int firstBrace = trimmed.IndexOf('{');
            int lastBrace = trimmed.LastIndexOf('}');
            if (firstBrace >= 0 && lastBrace > firstBrace)
                return trimmed.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return trimmed;
    }

    [Serializable]
    class OllamaGenerateRequest
    {
        public string model;
        public string prompt;
        public bool stream;
        public string format;
    }

#pragma warning disable 0649
    [Serializable]
    class OllamaGenerateResponse
    {
        public string response;
    }
#pragma warning restore 0649
}
