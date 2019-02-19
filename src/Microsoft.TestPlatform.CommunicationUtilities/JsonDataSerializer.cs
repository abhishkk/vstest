// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CommunicationUtilities
{
    using System.Text.Json.Serialization;
    using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Interfaces;

    /// <summary>
    /// JsonDataSerializes serializes and deserializes data using Json format
    /// </summary>
    public class JsonDataSerializer : IDataSerializer
    {
        private static JsonDataSerializer instance;

        /// <summary>
        /// Prevents a default instance of the <see cref="JsonDataSerializer"/> class from being created.
        /// </summary>
        private JsonDataSerializer()
        {
            // TODO: json converter settings like date format, datetiemoffset, utc, typenamehandling, referenceloophandling needs to be added. Confirm about the support.
        }

        /// <summary>
        /// Gets the JSON Serializer instance.
        /// </summary>
        public static JsonDataSerializer Instance
        {
            get
            {
                return instance ?? (instance = new JsonDataSerializer());
            }
        }

        /// <summary>
        /// Deserialize a <see cref="Message"/> from raw JSON text.
        /// </summary>
        /// <param name="rawMessage">JSON string.</param>
        /// <returns>A <see cref="Message"/> instance.</returns>
        public Message DeserializeMessage(string rawMessage)
        {
            return this.Deserialize<VersionedMessage>(rawMessage);
        }

        /// <summary>
        /// Deserialize the <see cref="Message.Payload"/> for a message.
        /// </summary>
        /// <param name="message">A <see cref="Message"/> object.</param>
        /// <typeparam name="T">Payload type.</typeparam>
        /// <returns>The deserialized payload.</returns>
        public T DeserializePayload<T>(Message message)
        {
            var versionedMessage = message as VersionedMessage;
            return this.Deserialize<T>(message.Payload);
        }

        /// <summary>
        /// Deserialize raw JSON to an object using the default serializer.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <param name="version">Version of serializer to be used.</param>
        /// <typeparam name="T">Target type to deserialize.</typeparam>
        /// <returns>An instance of <see cref="T"/>.</returns>
        public T Deserialize<T>(string json, int version = 1)
        {
            return this.Deserialize<T>(json);
        }

        /// <summary>
        /// Serialize an empty message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>Serialized message.</returns>
        public string SerializeMessage(string messageType)
        {
            return this.Serialize(new Message { MessageType = messageType });
        }

        /// <summary>
        /// Serialize a message with payload.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="payload">Payload for the message.</param>
        /// <returns>Serialized message.</returns>
        public string SerializePayload(string messageType, object payload)
        {
            return this.SerializePayload(messageType, payload, 1);
        }

        /// <summary>
        /// Serialize a message with payload.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="payload">Payload for the message.</param>
        /// <param name="version">Version for the message.</param>
        /// <returns>Serialized message.</returns>
        public string SerializePayload(string messageType, object payload, int version)
        {
            var serializedPayload = JToken.FromObject(payload, payloadSerializer);

            return version > 1 ?
                this.Serialize(new VersionedMessage { MessageType = messageType, Version = version, Payload = serializedPayload }) :
                this.Serialize(new Message { MessageType = messageType, Payload = serializedPayload });
        }

        /// <summary>
        /// Serialize an object to JSON using default serialization settings.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize.</typeparam>
        /// <param name="data">Instance of the object to serialize.</param>
        /// <param name="version">Version to be stamped.</param>
        /// <returns>JSON string.</returns>
        public string Serialize<T>(T data, int version = 1)
        {
            return this.Serialize(data);
        }

        /// <inheritdoc/>
        public T Clone<T>(T obj)
        {
            if (obj == null)
            {
                return default(T);
            }

            var stringObj = this.Serialize<T>(obj, 2);
            return this.Deserialize<T>(stringObj, 2);
        }

        /// <summary>
        /// Serialize data.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="data">Data to be serialized.</param>
        /// <returns>Serialized data.</returns>
        private string Serialize<T>(T data)
        {
            return JsonConverter.ToJsonString(data);
        }

        /// <summary>
        /// Deserialize data.
        /// </summary>
        /// <typeparam name="T">Type of data.</typeparam>
        /// <param name="data">Data to be deserialized.</param>
        /// <returns>Deserialized data.</returns>
        private T Deserialize<T>(string data)
        {
            return JsonConverter.FromJson<T>(data);
        }
    }
}
