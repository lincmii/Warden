﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Warden.Integrations.Cachet
{
    /// <summary>
    /// Custom Cachet client for executing HTTP requests to the API.
    /// </summary>
    public interface ICachetService
    {
        /// <summary>
        /// Creates a component using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the component.</param>
        /// <param name="status">Status of the component (1-4).</param>
        /// <param name="description">Description of the component.</param>
        /// <param name="link">A hyperlink to the component.</param>
        /// <param name="order">Order of the component (0 by default)</param>
        /// <param name="groupId">The group id that the component is within (0 by default).</param>
        /// <param name="enabled">Whether the component is enabled (true by default).</param>
        /// <returns>Details of created component if operation has succeeded.</returns>
        Task<Component> CreateComponentAsync(string name, int status, string description = null,
            string link = null, int order = 0, int groupId = 0, bool enabled = true);

        /// <summary>
        /// Updates a component using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the component.</param>
        /// <param name="name">Name of the component.</param>
        /// <param name="status">Status of the component (1-4).</param>
        /// <param name="link">A hyperlink to the component.</param>
        /// <param name="order">Order of the component (0 by default)</param>
        /// <param name="groupId">The group id that the component is within (0 by default).</param>
        /// <param name="enabled">Whether the component is enabled (true by default).</param>
        /// <returns>Details of created component if operation has succeeded.</returns>
        Task<Component> UpdateComponentAsync(int id, string name = null, int status = 1,
            string link = null, int order = 0, int groupId = 0, bool enabled = true);

        /// <summary>
        /// Deletes a component using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the component.</param>
        /// <returns>True if operation has succeeded, otherwise false.</returns>
        Task<bool> DeleteComponentAsync(int id);

        /// <summary>
        /// Creates an incident using the Cachet API.
        /// </summary>
        /// <param name="name">Name of the incident.</param>
        /// <param name="message">A message (supporting Markdown) to explain more.</param>
        /// <param name="status">Status of the incident (1-4).</param>
        /// <param name="visible">Whether the incident is publicly visible (1 = true by default).</param>
        /// <param name="componentId">Component to update. (Required with component_status).</param>
        /// <param name="componentStatus">The status to update the given component with (1-4).</param>
        /// <param name="notify">Whether to notify subscribers (false by default).</param>
        /// <param name="createdAt">When the incident was created (actual UTC date by default).</param>
        /// <param name="template">The template slug to use.</param>
        /// <param name="vars">The variables to pass to the template.</param>
        /// <returns>Details of created incident if operation has succeeded.</returns>
        Task<Incident> CreateIncidentAsync(string name, string message, int status, int visible = 1,
            string componentId = null, int componentStatus = 1, bool notify = false,
            DateTime? createdAt = null, string template = null, params string[] vars);


        /// <summary>
        /// Updates an incident using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the incident.</param>
        /// <param name="name">Name of the incident.</param>
        /// <param name="message">A message (supporting Markdown) to explain more.</param>
        /// <param name="status">Status of the incident (1-4).</param>
        /// <param name="visible">Whether the incident is publicly visible (1 = true by default).</param>
        /// <param name="componentId">Component to update. (Required with component_status).</param>
        /// <param name="componentStatus">The status to update the given component with (1-4).</param>
        /// <param name="notify">Whether to notify subscribers (false by default).</param>
        /// <returns>Details of updated incident if operation has succeeded.</returns>
        Task<Incident> UpdateIncidentAsync(int id, string name = null, string message = null,
            int status = 1, int visible = 1, string componentId = null, int componentStatus = 1,
            bool notify = false);

        /// <summary>
        /// Deletes an incident using the Cachet API.
        /// </summary>
        /// <param name="id">Id of the incident.</param>
        /// <returns>True if operation has succeeded, otherwise false.</returns>
        Task<bool> DeleteIncidentAsync(int id);
    }

    /// <summary>
    /// Default implementation of the ICachetService based on HttpClient.
    /// </summary>
    public class CachetService : ICachetService
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly HttpClient _client = new HttpClient();
        private static readonly string ComponentsEndpoint = "/components";
        private static readonly string IncidentsEndpoint = "/incidents";

        public CachetService(Uri apiUrl, JsonSerializerSettings jsonSerializerSettings)
        {
            _jsonSerializerSettings = jsonSerializerSettings;
            _client.BaseAddress = apiUrl;
        }

        public async Task<Component> CreateComponentAsync(string name, int status, string description = null,
            string link = null, int order = 0, int groupId = 0, bool enabled = true)
        {
            var component = Component.Create(name, status, description, link, order, groupId, enabled);
            var response = await PostAsync(ComponentsEndpoint, component);

            return await ProcessResponseAsync<Component>(response);
        }

        public async Task<Component> UpdateComponentAsync(int id, string name = null, int status = 1,
            string link = null, int order = 0, int groupId = 0, bool enabled = true)
        {
            var component = Component.Create(name, status, string.Empty, link, order, groupId, enabled);
            var response = await PutAsync($"{ComponentsEndpoint}/{id}", component);

            return await ProcessResponseAsync<Component>(response);
        }

        public async Task<bool> DeleteComponentAsync(int id)
        {
            var response = await DeleteAsync($"{ComponentsEndpoint}/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<Incident> CreateIncidentAsync(string name, string message, int status, int visible = 1,
            string componentId = null, int componentStatus = 1, bool notify = false,
            DateTime? createdAt = null, string template = null, params string[] vars)
        {
            var incident = Incident.Create(name, message, status, visible, componentId, componentStatus, notify,
                createdAt, template, vars);
            var response = await PostAsync(IncidentsEndpoint, incident);

            return await ProcessResponseAsync<Incident>(response);
        }

        public async Task<Incident> UpdateIncidentAsync(int id, string name = null, string message = null,
            int status = 1, int visible = 1, string componentId = null, int componentStatus = 1,
            bool notify = false)
        {
            var incident = Incident.Create(name, message, status, visible, componentId, componentStatus, notify);
            var response = await PutAsync($"{IncidentsEndpoint}/{id}", incident);

            return await ProcessResponseAsync<Incident>(response);
        }

        public async Task<bool> DeleteIncidentAsync(int id)
        {
            var response = await DeleteAsync($"{IncidentsEndpoint}/{id}");

            return response.IsSuccessStatusCode;
        }

        private static async Task<T> ProcessResponseAsync<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
                return default(T);

            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Envelope<T>>(content).Data;
        }

        private async Task<HttpResponseMessage> PostAsync(string endpoint, object data,
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
            => await ExecuteAsync(() => _client.PostAsync(GetFullUrl(endpoint), SerializeData(data)),
                headers, timeout, failFast);

        private async Task<HttpResponseMessage> PutAsync(string endpoint, object data,
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
            => await ExecuteAsync(() => _client.PutAsync(GetFullUrl(endpoint), SerializeData(data)),
                headers, timeout, failFast);

        private async Task<HttpResponseMessage> DeleteAsync(string endpoint,
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
            => await ExecuteAsync(() => _client.DeleteAsync(GetFullUrl(endpoint)),
                headers, timeout, failFast);

        private StringContent SerializeData(object data)
            => new StringContent(data?.ToJson(_jsonSerializerSettings), Encoding.UTF8, "application/json");

        private async Task<HttpResponseMessage> ExecuteAsync(Func<Task<HttpResponseMessage>> request,
            IDictionary<string, string> headers = null, TimeSpan? timeout = null, bool failFast = false)
        {
            SetRequestHeaders(headers);
            SetTimeout(timeout);
            try
            {
                var response = await request();
                if (response.IsSuccessStatusCode)
                    return response;
                if (!failFast)
                    return response;

                throw new Exception("Received invalid HTTP response from Cachet API" +
                                    $" with status code: {response.StatusCode}. " +
                                    $"Reason phrase: {response.ReasonPhrase}");
            }
            catch (Exception exception)
            {
                if (!failFast)
                    return null;

                throw new Exception("There was an error while executing the HTTP request to the Cachet API: " +
                                    $"{exception}", exception);
            }
        }

        private string GetFullUrl(string endpoint) => $"{_client.BaseAddress}/{endpoint}";

        private void SetTimeout(TimeSpan? timeout)
        {
            if (timeout > TimeSpan.Zero)
                _client.Timeout = timeout.Value;
        }

        private void SetRequestHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
                return;

            foreach (var header in headers)
            {
                var existingHeader = _client.DefaultRequestHeaders
                    .FirstOrDefault(x => string.Equals(x.Key, header.Key, StringComparison.CurrentCultureIgnoreCase));
                if (existingHeader.Key != null)
                    _client.DefaultRequestHeaders.Remove(existingHeader.Key);

                _client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }
    }
}