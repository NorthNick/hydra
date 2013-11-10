﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shastra.Hydra.Messaging.Attachments;

namespace Shastra.Hydra.Messaging.Storage
{
    public class CouchDbClient
    {
        const string JsonContentType = "application/json";
        private readonly string _dbUrl;
        private readonly HttpClient _client = new HttpClient();

        public CouchDbClient(string server, int port, string database)
        {
            _dbUrl = string.Format("http://{0}:{1}/{2}/", server, port, database);
        }

        public JObject GetDoc(string id)
        {
            var response = _client.GetAsync(_dbUrl + id).Result;
            response.EnsureSuccessStatusCode();
            var stream = response.Content.ReadAsStreamAsync().Result;
            string doc = new StreamReader(stream).ReadToEnd();
            return JObject.Parse(doc);
        }

        public JObject GetDoc(string id, IViewOptions options)
        {
            return GetDoc(string.Format("{0}?{1}", id, options));
        }

        public async Task<HttpContent> GetDocContentsAsync(string id)
        {
            var response = await _client.GetAsync(_dbUrl + id).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response.Content;
        }

        public async Task<JObject> SaveDocAsync(JObject json, IEnumerable<Attachment> attachments)
        {
            HttpContent content;

            if (attachments == null || !attachments.Any()) {
                // No attachments. Just send the JSON
                content = new StringContent(json.ToString(Formatting.None), new UTF8Encoding(), JsonContentType);
            } else {
                // Turn the attachments into HttpContent, and add them to the document
                content = CreateMultipartContent(json, attachments);
            }
            var response = await _client.PostAsync(_dbUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string reply = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JObject.Parse(reply);
        }

        public IEnumerable<JToken> View(string viewName, IViewOptions options, string designDoc)
        {
            return GetDoc(string.Format("_design/{0}/_view/{1}?{2}", designDoc, viewName, options))["rows"];
        }

        public JObject DeleteDocuments(JArray docs)
        {
            var content = new StringContent(new JObject(new JProperty("docs", docs)).ToString(Formatting.None), new UTF8Encoding(), "application/json");
            var response = _client.PostAsync(_dbUrl + "_bulk_docs", content).Result;
            response.EnsureSuccessStatusCode();
            string reply = response.Content.ReadAsStringAsync().Result;
            return JObject.Parse(reply);
        }

        private static MultipartContent CreateMultipartContent(JObject json, IEnumerable<Attachment> attachments)
        {
            // The attachments are turned into an _attachments property on the JSON. The value is an object having one property
            // per attachment, whose name is the attachment name and whose value is as in JsonAttachment below. The document is sent as
            // as multipart/related MIME HTTP message, whose first part is the JSON, and whose subsequent parts are the attachments, in the
            // same order as the properties in the _attachments property. There is no guarantee that JSON.NET serialises properties in the order
            // they are created, but it seems to do so.
            var mpContent = new MultipartContent("related");
            var parts = new List<HttpContent>();
            var jsonParts = new JObject();
            foreach (var attachment in attachments) {
                parts.Add(attachment.ToHttpContent());
                jsonParts.Add(new JProperty(attachment.Name, JsonAttachment(attachment.ContentType, attachment.DataLength())));
            }
            json.Add(new JProperty("_attachments", jsonParts));
            mpContent.Add(new StringContent(json.ToString(Formatting.None), new UTF8Encoding(), JsonContentType));
            foreach (var httpContent in parts) {
                mpContent.Add(httpContent);
            }
            return mpContent;
        }

        private static JObject JsonAttachment(string contentType, long length)
        {
            return new JObject(
                new JProperty("follows", true),
                new JProperty("content_type", contentType),
                new JProperty("length", length));
        }
    }
}
