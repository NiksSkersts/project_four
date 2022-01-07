namespace LLU.Models;

/// <summary>
///     Defines secrets that are not bundled with the rest of the code.
/// </summary>
public class Secrets {
    public string MailServer { get; set; }
    public int MailPort { get; set; }
    public int SMTPPort { get; set; }
}