using UnityEngine;

public static class InvalidWordTypes
{
    public enum InvalidWordType
    {
        NotInWordlist,
        AlreadyUsed,
        NotPartikularNaSalita,
        AbsentDiacritic,
        MahabangSalita,
        MaiklingSalita
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
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string mahabangSalita = "\"{word}\" has less than 4 tiles!";
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string maiklingSalita = "\"{word}\" has more than 4 tiles!";

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
                case InvalidWordType.MahabangSalita:
                    template = mahabangSalita;
                    break;
                case InvalidWordType.MaiklingSalita:
                    template = maiklingSalita;
                    break;
                default:
                    return "";
            }

            return (template ?? "").Replace("{word}", word ?? "");
        }
    }
}
