using UnityEngine;

public static class InvalidWordTypes
{
    public enum InvalidWordType
    {
        NotInWordlist,
        AlreadyUsed
    }

    [System.Serializable]
    public class Messages
    {
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string notInWordlist = "Walang salitang \"{word}";

        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string alreadyUsed = "Salitang \"{word}\" has already been used!";

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
                default:
                    return "";
            }

            return (template ?? "").Replace("{word}", word ?? "");
        }
    }
}
