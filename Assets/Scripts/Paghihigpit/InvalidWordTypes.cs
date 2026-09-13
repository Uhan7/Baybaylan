using UnityEngine;

public static class InvalidWordTypes
{
    public enum InvalidWordType
    {
        NotInWordlist,
        AlreadyUsed
    }

    public static string GetInvalidWordMessage(InvalidWordType type, string word)
    {
        word = word.ToUpper(); //caps all letters

        switch (type)
        {
            case InvalidWordType.NotInWordlist:
                return "Walang salitang \"" + word + "\"!";
            case InvalidWordType.AlreadyUsed:
                return "Salitang \"" + word + "\" has already been used!";
            default:
                return "";
        }
    }
}