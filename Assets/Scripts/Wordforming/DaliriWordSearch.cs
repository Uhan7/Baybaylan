using System;
using System.Collections.Generic;

// A sorted prefix index shares the dictionary's strings rather than allocating
// millions of trie nodes. Binary searches narrow the range at every tile.
public static class DaliriWordSearch
{
    private static HashSet<string> source;
    private static string[] words;

    public static void Prepare(HashSet<string> dictionary)
    {
        if (ReferenceEquals(source, dictionary)) return;
        source = dictionary;
        words = new string[dictionary.Count];
        dictionary.CopyTo(words);
        Array.Sort(words, StringComparer.Ordinal);
    }

    public sealed class Choice
    {
        public Tile Tile;
        public Tile.Diacritic Mark;
        public string Text;
    }

    public static bool Find(List<Tile> tiles, int length, LevelConfig config,
        string particularWord, Tile sacredTile, HashSet<string> usedWords,
        out List<Choice> result, out string word)
    {
        var options = new List<Choice>[tiles.Count];
        bool marked = config != null && config.HasPaghihigpit(PaghihigpitTypes.MarkaNgBaybayin);
        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            options[i] = new List<Choice>();
            for (int mark = 0; mark < (tile.isVowel ? 1 : 4); mark++)
            {
                if (marked && !tile.isVowel && mark == 0) continue;
                string text = tile.isVowel ? tile.latinText : tile.rootConsonant +
                    (mark == 0 ? "a" : mark == 1 ? "i" : mark == 2 ? "u" : "");
                options[i].Add(new Choice { Tile = tile, Mark = (Tile.Diacritic)mark,
                    Text = BaybayinTileSequenceRules.ToTileSpelling(text) });
            }
        }
        // Randomize traversal without shuffling the dictionary.
        for (int i = options.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var swap = options[i]; options[i] = options[j]; options[j] = swap;
        }
        result = new List<Choice>();
        word = null;
        return Search(options, new bool[tiles.Count], result, "", length, 0, words.Length,
            config, particularWord, sacredTile, usedWords, out word);
    }

    private static bool Search(List<Choice>[] options, bool[] used, List<Choice> path,
        string prefix, int length, int low, int high, LevelConfig config,
        string particular, Tile sacred, HashSet<string> history, out string word)
    {
        word = null;
        if (path.Count == length)
        {
            if (low >= high || words[low] != prefix) return false;
            if (particular != null && particular != prefix) return false;
            if (config != null && config.HasPaghihigpit(PaghihigpitTypes.BawalUmulit) && history.Contains(prefix)) return false;
            if (config != null && config.HasPaghihigpit(PaghihigpitTypes.SagradongTitik) &&
                !path.Exists(c => c.Tile.HasSameTilePrefab(sacred))) return false;
            word = prefix;
            return true;
        }
        var tried = new List<Tile>();
        for (int i = 0; i < options.Length; i++)
        {
            if (used[i] || options[i].Count == 0) continue;
            Tile tile = options[i][0].Tile;
            if (tried.Exists(t => t.HasSameTilePrefab(tile) && t.rootConsonant == tile.rootConsonant && t.vowel == tile.vowel)) continue;
            tried.Add(tile);
            if (config != null && config.HasPaghihigpit(PaghihigpitTypes.PagiibangTitik) &&
                path.Exists(c => c.Tile.HasSameTilePrefab(tile))) continue;
            foreach (Choice choice in options[i])
            {
                if (path.Count > 0)
                {
                    Choice previous = path[path.Count - 1];
                    char last = previous.Text[previous.Text.Length - 1];
                    if (tile.isVowel && last != 'a' && last != 'i' && last != 'u') continue;
                    if (!tile.isVowel && previous.Text == "n" && tile.rootConsonant == "g") continue;
                }
                string next = prefix + choice.Text;
                if (particular != null && !particular.StartsWith(next, StringComparison.Ordinal)) continue;
                int first = LowerBound(next, low, high);
                if (first >= high || !words[first].StartsWith(next, StringComparison.Ordinal)) continue;
                int end = LowerBound(next + '\uffff', first, high);
                used[i] = true; path.Add(choice);
                if (Search(options, used, path, next, length, first, end, config, particular, sacred, history, out word)) return true;
                path.RemoveAt(path.Count - 1); used[i] = false;
            }
        }
        return false;
    }

    private static int LowerBound(string value, int low, int high)
    {
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            if (string.CompareOrdinal(words[middle], value) < 0) low = middle + 1;
            else high = middle;
        }
        return low;
    }
}
