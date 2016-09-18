// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces
{
    using Newtonsoft.Json;
    using System.Threading.Tasks;

    /// <summary>
    /// The Communication Manager interface.
    /// </summary>
    public interface ICommunicationManager
    {
        /// <summary>
        /// Host a server and listens on endpoint for requests
        /// </summary>
        /// <returns>Port number of the listening endpoint</returns>
        int HostServer();

        /// <summary>
        /// Accepts client connection asynchronously
        /// </summary>
        Task AcceptClientAsync();

        /// <summary>
        /// Waits for client to be connected to this server
        /// Whoever hosting the server should use this method to 
        /// wait for a client to connect
        /// </summary>
        /// <param name="connectionTimeout">Time to wait for the connection</param>
        /// <returns>True, if Server got a connection from client</returns>
        bool WaitForClientConnection(int connectionTimeout);


        /// <summary>
        /// Waits for server to be connected
        /// Whoever creating the client and trying to connect to a server 
        /// should use this method to wait for connection to be established with server
        /// </summary>
        /// <param name="connectionTimeout">Time to wait for the connection</param>
        /// <returns>True, if Server got a connection from client</returns>
        bool WaitForServerConnection(int connectionTimeout);

        /// <summary>
        /// Stops any hosted server
        /// </summary>
        void StopServer();

        /// <summary>
        /// Creates a Client Channel and connects to server on given port number
        /// </summary>
        /// <param name="portNumber"></param>
        Task SetupClientAsync(int portNumber);

        /// <summary>
        /// Stops any client connected to server
        /// </summary>
        void StopClient();

        /// <summary>
        /// Writes message to the binary writer.
        /// </summary>
        /// <param name="messageType">Type of Message to be sent, for instance TestSessionStart</param>
        void SendMessage(string messageType);

        /// <summary>
        /// Reads message from the binary reader
        /// </summary>
        /// <returns>Returns message read from the binary reader</returns>
        Message ReceiveMessage();

        /// <summary>
        /// Reads message from the binary reader
        /// </summary>
        /// <returns> Raw message string </returns>
        string ReceiveRawMessage();

        /// <summary>
        ///  Writes message to the binary writer with payload
        /// </summary>
        /// <param name="messageType">Type of Message to be sent, for instance TestSessionStart</param>
        /// <param name="payload">payload to be sent</param>
        void SendMessage(string messageType, object payload);

        /// <summary>
        /// Send serialized raw message
        /// </summary>
        /// <param name="rawMessage">serialized message</param>
        void SendRawMessage(string rawMessage);

        /// <summary>
        /// The send hand shake message.
        /// </summary>
        void SendHandShakeMessage();

        /// <summary>
        /// Deserializes the Message into actual TestPlatform objects
        /// </summary>
        /// <typeparam name="T"> The type of object to deserialize to. </typeparam>
        /// <param name="message"> Message object </param>
        /// <returns> TestPlatform object </returns>
        T DeserializePayload<T>(Message message);
    }
}