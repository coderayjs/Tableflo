using TableFlo.Core.Models;

namespace TableFlo.UI;

/// <summary>
/// Manages the current user session
/// </summary>
public static class SessionManager
{
    public static Employee? CurrentEmployee { get; set; }
    
    public static bool IsLoggedIn => CurrentEmployee != null;
    
    public static void Logout()
    {
        CurrentEmployee = null;
    }
}

