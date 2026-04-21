using System.Collections.Generic;
using UnityEngine;

public class Rule
{
    public string npc;
    public string[] keywords;
    public string reply;
}

public class NpcRuleEngine : MonoBehaviour
{

    [SerializeField] private float delayMin = 0.5f;
    [SerializeField] private float delayMax = 1.5f;

    public static NpcRuleEngine Instance;

    private List<Rule> rules = new List<Rule>();
    private string lastPlayerInput = "";

    private void Awake()
    {
        Instance = this;
        LoadRules();
    }

    private void LoadRules()
    {
        TextAsset csv = Resources.Load<TextAsset>("npc_rules");

        var lines = csv.text.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("npc"))
                continue;

            var parts = line.Split(',');

            if (parts.Length < 3) continue;

            rules.Add(new Rule
            {
                npc = parts[0].Trim(),
                keywords = parts[1].ToLower().Split('|'),
                reply = parts[2].Trim()
            });
        }
    }

    public string GetReply(string npcName, string input)
    {

        string lower = input.ToLower();

        Rule bestRule = null;
        int bestScore = 0;

        foreach (var rule in rules)
        {
            if (rule.npc != npcName)
                continue;

            int score = 0;

            foreach (var keyword in rule.keywords)
            {
                if (lower.Contains(keyword))
                    score++;
            }
            if (rule.reply.Contains("?"))
                score += 1;

            if (score > bestScore)
            {
                bestScore = score;
                bestRule = rule;
            }
        }
        
        string repeat = "";
        if (input == lastPlayerInput)
            repeat = "Ugh. I already said that. ";


        lastPlayerInput = input;

        if (bestRule != null && bestScore > 1) {
            string[] options = bestRule.reply.Split('|');
            Debug.Log($"Best rule for '{input}' is '{bestRule.reply}' with score {bestScore}");
            return repeat + options[Random.Range(0, options.Length)];
        } else {
            // return random reply
            int numRules = rules.Count;
            int randomIndex = Random.Range(0, numRules);
            int variations = rules[randomIndex].reply.Split('|').Length;
            Debug.Log($"No good rule for '{input}', returning random reply '{rules[randomIndex].reply}'");
            return rules[randomIndex].reply.Split('|')[Random.Range(0, variations)];
        }

        return DefaultReply(npcName);
    }

    private string DefaultReply(string npcName)
    {
        if (npcName == "Hiker")
            return "I can't see you clearly, but keep moving and look for landmarks.";

        if (npcName == "Ranger")
            return "Stay on the trail and follow the markers.";

        return $"[{npcName}] ...";
    }
}
