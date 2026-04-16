using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;

public class AIOpponent : MonoBehaviour
{
    public static AIOpponent Instance;

    private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";
    private const string MODEL = "llama-3.1-8b-instant";

    private string apiKey = "";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        apiKey = ConfigReader.Get("GROQ_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
            Debug.LogWarning("Groq API key not found. AI will use fallback behavior.");
        else
            Debug.Log("Groq API key loaded successfully.");
    }

    public IEnumerator DecideAndExecuteTurn(
    List<Card> opponentDeck,
    List<CardUI> opponentUnits,
    List<CardUI> playerUnits,
    int opponentMana,
    int opponentAltarHealth,
    int playerAltarHealth,
    System.Action<List<string>> onPlanReceived)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            List<string> fallback = BuildFallbackPlan(opponentDeck, opponentUnits, playerUnits, opponentMana);
            onPlanReceived(fallback);
            yield break;
        }

        string gameState = BuildGameStatePrompt(
            opponentDeck, opponentUnits, playerUnits,
            opponentMana, opponentAltarHealth, playerAltarHealth);

        yield return StartCoroutine(CallGroqAPI(gameState, (response) =>
        {
            List<string> plan = ParsePlan(response);
            onPlanReceived(plan);
        }));
    }

    List<string> BuildFallbackPlan(
    List<Card> opponentDeck,
    List<CardUI> opponentUnits,
    List<CardUI> playerUnits,
    int opponentMana)
    {
        List<string> plan = new List<string>();

        // Play affordable cards
        foreach (Card card in opponentDeck)
        {
            if (card.isUnit && card.manaCost <= opponentMana && opponentUnits.Count < 6)
            {
                plan.Add($"PLAY: {card.cardName}");
                opponentMana -= card.manaCost;
            }
        }

        // Attack with all available units
        foreach (CardUI unit in opponentUnits)
        {
            if (!unit.hasAttackedThisTurn)
            {
                if (playerUnits.Count > 0)
                    plan.Add($"ATTACK: {unit.cardData.cardName} -> {playerUnits[0].cardData.cardName}");
                else
                    plan.Add($"ATTACK_ALTAR: {unit.cardData.cardName}");
            }
        }

        plan.Add("END_TURN");
        return plan;
    }

    string BuildGameStatePrompt(
    List<Card> opponentDeck,
    List<CardUI> opponentUnits,
    List<CardUI> playerUnits,
    int opponentMana,
    int opponentAltarHealth,
    int playerAltarHealth)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("You are playing a card game. Output ONLY a turn plan using the exact format below.");
        sb.AppendLine();

        // Playable cards with index
        sb.AppendLine($"MANA: {opponentMana}");
        sb.AppendLine("PLAYABLE CARDS (select by exact name only):");
        bool anyPlayable = false;
        foreach (Card card in opponentDeck)
        {
            if (card.isUnit && card.manaCost <= opponentMana)
            {
                sb.AppendLine($"  {card.cardName} (cost {card.manaCost}, {card.attack}/{card.health}) [{card.abilityDescription}]");
                anyPlayable = true;
            }
        }
        if (!anyPlayable) sb.AppendLine("  (none)");

        sb.AppendLine();
        sb.AppendLine("YOUR BOARD (attackers available this turn):");
        bool anyAttackers = false;
        foreach (CardUI unit in opponentUnits)
        {
            if (!unit.hasAttackedThisTurn)
            {
                sb.AppendLine($"  {unit.cardData.cardName} ({unit.currentAttack}/{unit.currentHealth})");
                anyAttackers = true;
            }
        }

        if (opponentUnits.Count >= 6)
            sb.AppendLine("NOTE: YOUR BOARD IS FULL. You cannot play any more units. Focus on ATTACK and ATTACK_ALTAR actions only.");
        else if (!anyPlayable)
            sb.AppendLine("NOTE: You have no playable cards this turn. Focus on ATTACK and ATTACK_ALTAR actions only.");

        if (!anyAttackers) sb.AppendLine("  (none)");

        sb.AppendLine();
        sb.AppendLine($"ENEMY ALTAR HP: {playerAltarHealth}");
        sb.AppendLine("ENEMY UNITS (valid ATTACK targets):");
        if (playerUnits.Count == 0)
            sb.AppendLine("  (none — altar attacks allowed)");
        else
            foreach (CardUI unit in playerUnits)
                sb.AppendLine($"  {unit.cardData.cardName} ({unit.currentAttack}/{unit.currentHealth})");

        sb.AppendLine();
        sb.AppendLine("=== STRICT RULES ===");
        sb.AppendLine("1. PLAY: use ONLY names from PLAYABLE CARDS. No other names allowed.");
        sb.AppendLine("2. ATTACK: [attacker] -> [target] — target MUST be an exact name from ENEMY UNITS. No other values.");
        sb.AppendLine("3. ATTACK_ALTAR: [attacker] — ONLY valid when ENEMY UNITS list shows (none) you can attack it if you clear the board.");
        sb.AppendLine("4. Do NOT write explanations, comments, or anything after a card name.");
        sb.AppendLine("5. Do NOT invent card names. Do NOT repeat a card you already played.");
        sb.AppendLine("6. Units played this turn have summoning sickness — do NOT use them as attackers.");
        sb.AppendLine("7. End your plan with END_TURN.");
        sb.AppendLine("8. If your board is full, do NOT include any PLAY actions. Only ATTACK and ATTACK_ALTAR.");
        sb.AppendLine("9. If ENEMY UNITS shows (none), use ATTACK_ALTAR for every unit that can attack.");
        sb.AppendLine();
        sb.AppendLine("=== OUTPUT FORMAT (one action per line, nothing else) ===");
        sb.AppendLine("PLAY: [exact card name]");
        sb.AppendLine("ATTACK: [your attacker name] -> [enemy unit name]");
        sb.AppendLine("ATTACK_ALTAR: [your attacker name]");
        sb.AppendLine("END_TURN");

        return sb.ToString();
    }

    IEnumerator CallGroqAPI(string prompt, System.Action<string> callback)
    {
        string escapedPrompt = prompt
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");

        string jsonBody = "{\"model\":\"" + MODEL + "\",\"max_tokens\":200,\"messages\":[{\"role\":\"user\",\"content\":\"" + escapedPrompt + "\"}]}";

        using (UnityWebRequest request = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"Groq response: {responseText}");
                string content = ParseAPIResponse(responseText);
                Debug.Log($"Parsed decision: {content}");
                callback(content);
            }
            else
            {
                Debug.LogError($"Groq API error: {request.error} — {request.downloadHandler.text}");
                callback("END_TURN");
            }
        }
    }

    string ParseAPIResponse(string jsonResponse)
    {
        try
        {
            int contentIndex = jsonResponse.IndexOf("\"content\":");
            if (contentIndex >= 0)
            {
                int start = jsonResponse.IndexOf("\"", contentIndex + 10) + 1;
                int end = jsonResponse.IndexOf("\"", start);
                string raw = jsonResponse.Substring(start, end - start);

                // Convert escape sequences to real characters
                raw = raw
                    .Replace("\\r\\n", "\n")
                    .Replace("\\n", "\n")
                    .Replace("\\r", "\n")
                    .Replace("\\u003e", ">")
                    .Replace("\u003e", ">")
                    .Replace("–", "->")
                    .Replace("—", "->");

                return raw.Trim();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing response: {e.Message}");
        }
        return "END_TURN";
    }

    List<string> ParsePlan(string response)
    {
        List<string> actions = new List<string>();

        string[] lines = response.Split('\n');

        foreach (string line in lines)
        {
            // Clean the line
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Strip stat annotations like "(2/3)" or "(empty space, enemy has no other units)"
            trimmed = System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s*\(.*?\)", "").Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Normalize ATTACK -> Altar variants to ATTACK_ALTAR
            if (trimmed.StartsWith("ATTACK:", System.StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = trimmed.Substring(7).Split(new string[] { "->" }, System.StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string attacker = parts[0].Trim();
                    string target = parts[1].Trim();

                    // Skip invalid targets like "None"
                    if (target.Equals("none", System.StringComparison.OrdinalIgnoreCase) ||
                        string.IsNullOrEmpty(target))
                    {
                        Debug.Log($"AI: Ignoring ATTACK with invalid target '{target}'");
                        continue;
                    }

                    if (target.IndexOf("altar", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                        target.IndexOf("shrine", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Convert to ATTACK_ALTAR
                        trimmed = $"ATTACK_ALTAR: {attacker}";
                    }
                }
            }

            // Normalize END_TURN variants (including "END TURN")
            if (trimmed.Replace(" ", "").Equals("ENDTURN", System.StringComparison.OrdinalIgnoreCase))
                trimmed = "END_TURN";
            else if (trimmed.IndexOf("END", System.StringComparison.OrdinalIgnoreCase) >= 0 &&
                     trimmed.IndexOf("TURN", System.StringComparison.OrdinalIgnoreCase) >= 0)
                trimmed = "END_TURN";

            // Skip WAIT or other unknown filler words the model sometimes adds
            if (!trimmed.StartsWith("PLAY:", System.StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("ATTACK:", System.StringComparison.OrdinalIgnoreCase) &&
                !trimmed.StartsWith("ATTACK_ALTAR:", System.StringComparison.OrdinalIgnoreCase) &&
                !trimmed.Equals("END_TURN", System.StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"AI: Ignoring unrecognized line '{trimmed}'");
                continue;
            }

            actions.Add(trimmed);

            if (trimmed.Equals("END_TURN", System.StringComparison.OrdinalIgnoreCase))
                break;

            if (actions.Count >= 12) break;
        }

        if (actions.Count == 0 || !actions[actions.Count - 1]
            .Equals("END_TURN", System.StringComparison.OrdinalIgnoreCase))
            actions.Add("END_TURN");

        Debug.Log($"AI plan ({actions.Count} actions): {string.Join(" | ", actions)}");
        return actions;
    }
}