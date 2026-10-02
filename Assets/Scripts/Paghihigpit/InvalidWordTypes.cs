using UnityEngine;

public static class InvalidWordTypes
{
    public enum InvalidWordType
    {
        NotInWordlist,
        AlreadyUsed,
        NotPartikularNaSalita,
        AbsentDiacritic
    }

    [System.Serializable]
    public class Messages
    {
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string notInWordlist = "Walang salitang \"{word}";

        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string alreadyUsed = "Salitang \"{word}\" has already been used!";

        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string notPartikularNaSalita = "Salitang \"{word}\" is not the particular word!";
        
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string absentDiacritic = "\"{word}\" has at least one letter without a diacritic!";

        public string GetInvalidWordMessage(InvalidWordType type, string word)
        {
            string template;
            switch (type)
            {
                case InvalidWordType.NotInWordlist:
                    template = notInWordlist;
                    break;
                case InvalidWordType.AlreadyUsed:
                    template = alreadyUsed;
                    break;
                case InvalidWordType.NotPartikularNaSalita:
                    template = notPartikularNaSalita;
                    break;
                case InvalidWordType.AbsentDiacritic:
                    template = absentDiacritic;
                    break;
                default:
                    return "";
            }

            return (template ?? "").Replace("{word}", word ?? "");
        }
    }
}
