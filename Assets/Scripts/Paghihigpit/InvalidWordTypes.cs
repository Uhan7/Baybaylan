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
        [SerializeField] string notInWordlist = "Wala sa talaan ang salitang “{word}”!";

        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string alreadyUsed = "Naisumite na ang salitang “{word}”!";

        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string notPartikularNaSalita = "Hindi “{word}” ang salitang kailangan buuin!";
        
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string absentDiacritic = "May titik sa “{word}” na walang Kudlit o Krus!";
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string mahabangSalita = "Masyadong maikli ang “{word}”. Kailangan ng 4 na titik o higit pa!";
        [TextArea, Tooltip("Use {word} where the submitted word should appear.")]
        [SerializeField] string maiklingSalita = "Masyadong mahaba ang “{word}”. Hanggang 4 na titik lamang!";

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
