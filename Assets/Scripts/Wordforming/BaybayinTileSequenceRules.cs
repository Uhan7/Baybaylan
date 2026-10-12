using System.Collections.Generic;

public static class BaybayinTileSequenceRules
{
    public static bool HasValidStandaloneVowelPlacement(IReadOnlyList<Tile> tiles)
    {
        if (tiles == null) return false;

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            if (tile == null) return false;
            if (!tile.isVowel || i == 0) continue;
            if (!EndsWithVowel(tiles[i - 1]?.latinText)) return false;
        }

        return true;
    }

    public static bool UsesDedicatedNgTile(IReadOnlyList<Tile> tiles)
    {
        if (tiles == null) return false;

        for (int i = 1; i < tiles.Count; i++)
        {
            if (FormsSplitNg(tiles[i - 1], tiles[i])) return false;
        }

        return true;
    }

    public static bool CanAppendTile(IReadOnlyList<Tile> currentTiles, Tile candidate)
    {
        if (candidate == null) return false;
        if (currentTiles == null || currentTiles.Count == 0) return true;

        Tile previousTile = currentTiles[currentTiles.Count - 1];
        if (FormsSplitNg(previousTile, candidate)) return false;
        if (!candidate.isVowel) return true;

        return EndsWithVowel(previousTile?.latinText);
    }

    private static bool FormsSplitNg(Tile previousTile, Tile currentTile)
    {
        if (previousTile == null || currentTile == null) return false;
        if (previousTile.isVowel || currentTile.isVowel) return false;

        return string.Equals(
                previousTile.latinText,
                "n",
                System.StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                currentTile.rootConsonant,
                "g",
                System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool EndsWithVowel(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;

        char finalCharacter = char.ToLowerInvariant(text[text.Length - 1]);
        return finalCharacter == 'a' ||
            finalCharacter == 'e' ||
            finalCharacter == 'i' ||
            finalCharacter == 'o' ||
            finalCharacter == 'u';
    }
}
