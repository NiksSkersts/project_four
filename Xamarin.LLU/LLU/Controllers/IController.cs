using System;
using LLU.Models;

namespace LLU.Controllers;

/// <summary>
///     Provides a common shell for email controller creation.
/// </summary>
internal interface IController : IDisposable {
    /// <summary>
    ///     Common function to authenticate user in the server. Takes username and password.
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
    /// <param name="userData.Password">Password....</param>
    /// <param name="client"></param>
    /// <returns></returns>
    object ClientAuth(UserData userData, object client);

    /// <summary>
    ///     Common function to create a connection with the server.
    ///     <param name="data"> Client object. Either IMAP or SMTP.</param>
    ///     <returns>Returns a created client object that has been connected with the server.</returns>
    /// </summary>
    object Connect(object data);
}