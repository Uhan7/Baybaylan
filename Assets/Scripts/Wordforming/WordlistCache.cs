using System;
using System.Collections.Generic;
using UnityEngine;

// Word validation only reads this set. Keep one copy for the current wordlists
// instead of rebuilding millions of entries whenever a new scene opens.
public static class WordlistCache
{
    private static TextAsset[] cachedSources;
    private static HashSet<string> cachedWords;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetForNewSession()
    {
        cachedSources = null;
        cachedWords = null;
    }

    // The returned set is shared: callers must not add/remove words from it.
    // Per-level history belongs in GameManager.wordsUsed instead.
    public static HashSet<string> GetOrLoad(TextAsset[] sources)
    {
        sources = sources ?? Array.Empty<TextAsset>();
        if (MatchesCachedSources(sources)) return cachedWords;

        // Read each TextAsset once: its text property itself creates a string.
        string[] texts = new string[sources.Length];
        int capacity = 0;
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null) continue;
            string text = sources[i].text;
            texts[i] = text;
            for (int j = 0; j < text.Length; j++)
                if (text[j] == '\n') capacity++;
            if (text.Length > 0 && text[text.Length - 1] != '\n') capacity++;
        }

        // Avoid the old hash-table arrays left behind by repeated resizing.
        HashSet<string> words = new HashSet<string>(capacity, StringComparer.Ordinal);
        char[] buffer = new char[64];
        foreach (string text in texts)
        {
            if (text == null) continue;
            int lineStart = 0;
            for (int end = 0; end <= text.Length; end++)
            {
                if (end < text.Length && text[end] != '\n') continue;

                int first = lineStart;
                int last = end;
                lineStart = end + 1;
                while (first < last && char.IsWhiteSpace(text[first])) first++;
                while (last > first && char.IsWhiteSpace(text[last - 1])) last--;
                int length = last - first;
                if (length == 0) continue;

                if (buffer.Length < length) buffer = new char[length];
                text.AsSpan(first, length).ToLowerInvariant(buffer.AsSpan(0, length));
                for (int j = 0; j < length; j++)
                {
                    if (buffer[j] == 'e') buffer[j] = 'i';
                    else if (buffer[j] == 'o') buffer[j] = 'u';
                }

                // Allocate only the final normalized word, without Split's
                // line array, raw line strings, or intermediate replacements.
                words.Add(new string(buffer, 0, length));
            }
        }

        cachedSources = (TextAsset[])sources.Clone();
        cachedWords = words;
        return words;
    }

    private static bool MatchesCachedSources(TextAsset[] sources)
    {
        if (cachedWords == null || cachedSources.Length != sources.Length) return false;
        for (int i = 0; i < sources.Length; i++)
            if (cachedSources[i] != sources[i]) return false;
        return true;
    }
}
