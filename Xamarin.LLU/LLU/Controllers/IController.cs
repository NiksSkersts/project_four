using System;

namespace LLU.Controllers;

internal interface IController : IDisposable {
    /// <summary>
    /// Common function to authentificate user in the server. Takes username and password.
    /// <para>WARNING: Client should be connected before calling this function!</para>
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    bool ClientAuth(string username, string password);
    /// <summary>Quick intro into resultCode:
    /// <list type="bullet">
    /// <item>0 - All good</item>
    /// <item>1 - Client connection failed</item>
    /// <item>2 - Auth failed</item>
    /// </list>
    /// </summary>
    /// <returns>byte resultCode</returns>
    byte Connect();
    /// <summary>
    /// Handles disposing of the class.
    /// <para>Cancels token and makes sure Client is disconnected and disposed off cleanly to avoid extra strain on server.</para>
    /// </summary>
    new void Dispose();
}
