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

    public static bool CanAppendTile(IReadOnlyList<Tile> currentTiles, Tile candidate)
    {
        if (candidate == null) return false;
        if (!candidate.isVowel || currentTiles == null || currentTiles.Count == 0)
            return true;

        return EndsWithVowel(currentTiles[currentTiles.Count - 1]?.latinText);
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
