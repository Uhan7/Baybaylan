using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AlahasSelectionWarningCondition
{
    NoAlahasSelected,
    PartiallyFilledSlots
}

[Serializable]
public sealed class AlahasSelectionWarningRule
{
    [SerializeField] private bool enabled = true;
    [SerializeField] private AlahasSelectionWarningCondition condition;
    [TextArea(2, 4), SerializeField] private string message;

    public string Message => message;

    public AlahasSelectionWarningRule(
        AlahasSelectionWarningCondition warningCondition,
        string warningMessage)
    {
        condition = warningCondition;
        message = warningMessage;
    }

    public bool ShouldWarn(IReadOnlyList<Alahas> slots)
    {
        if (!enabled || slots == null || slots.Count == 0) return false;

        bool hasSelectedAlahas = slots.Any(alahas => alahas != null);
        switch (condition)
        {
            case AlahasSelectionWarningCondition.NoAlahasSelected:
                return !hasSelectedAlahas;

            case AlahasSelectionWarningCondition.PartiallyFilledSlots:
                return hasSelectedAlahas && slots.Any(alahas => alahas == null);

            default:
                return false;
        }
    }

    public static List<AlahasSelectionWarningRule> CreateDefaults()
    {
        return new List<AlahasSelectionWarningRule>
        {
            new AlahasSelectionWarningRule(
                AlahasSelectionWarningCondition.NoAlahasSelected,
                "You haven't selected any Alahas. Are you sure you want to proceed?"),
            new AlahasSelectionWarningRule(
                AlahasSelectionWarningCondition.PartiallyFilledSlots,
                "Some Alahas slots are still empty. Are you sure you want to proceed?")
        };
    }
}
