using System;
using LLU.Models;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;

namespace LLU.Controllers;

/// <summary>
///     Provides a common shell for standard email controller creation.
/// </summary>
internal interface IController : IDisposable {
    /// <summary>
    ///     Common function to authentificate user in the server. Takes username and password.
    ///     Disconnecting just because user inputted wrong credentials is a waste of resources, and can get you banned from the
    ///     server for a period of time.
    ///     Keep connection with the server, but try auth again instead.
    ///     <para>WARNING: Client should be connected before calling this function!</para>
    /// </summary>
    /// <param name="userData">Consists of Username and Password fields.</param>
    /// <param name="userData.Username">
    ///     Username would normally be written like "example@gmail.com", but LLU uses only the part before
    ///     @.
    /// </param>
    /// <param name="userData.password">Password....</param>
    /// <param name="client"></param>
    /// <returns></returns>
    object ClientAuth(UserData userData,object client);
    
    /// <summary>
    ///     Quick intro into resultCode:
    ///     <list type="bullet">
    ///         <item>0 - All good</item>
    ///         <item>1 - Client connection failed</item>
    ///         <item>2 - Auth failed</item>
    ///     </list>
    /// </summary>
    /// <returns>byte resultCode</returns>
    object Connect(object data);
}