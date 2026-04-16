using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class ConfigReader
{
    private static Dictionary<string, string> config = null;

    public static string Get(string key)
    {
        if (config == null)
            LoadConfig();

        if (config != null && config.ContainsKey(key))
            return config[key];

        Debug.LogWarning($"Config key '{key}' not found.");
        return "";
    }

    static void LoadConfig()
    {
        config = new Dictionary<string, string>();

        string path = Path.Combine(Application.dataPath, "../config.env");
        Debug.Log("Looking for config at: " + path);

        if (!File.Exists(path))
        {
            Debug.LogWarning("config.env not found. AI opponent will use fallback behavior.");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            int separatorIndex = line.IndexOf('=');
            if (separatorIndex > 0)
            {
                string k = line.Substring(0, separatorIndex).Trim();
                string v = line.Substring(separatorIndex + 1).Trim();
                config[k] = v;
            }
        }

        Debug.Log("Config loaded successfully.");
    }
}