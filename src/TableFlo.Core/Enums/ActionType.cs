namespace TableFlo.Core.Enums;

/// <summary>
/// Types of actions for audit logging
/// </summary>
public enum ActionType
{
    Login,
    Logout,
    DealerAssigned,
    DealerRemoved,
    DealerSentToBreak,
    DealerReturnedFromBreak,
    DealerSentHome,
    PushExecuted,
    ScheduleGenerated,
    ManualOverride,
    TableOpened,
    TableClosed,
    SettingsChanged,
    ReportGenerated
}

