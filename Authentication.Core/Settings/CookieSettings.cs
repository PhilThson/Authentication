namespace Authentication.Core.Settings;

public class CookieSettings
{
    public bool IsEssential { get; set; }
    public bool HttpOnly { get; set; }
    public bool Secure { get; set; }
    public string Domain { get; set; }
}