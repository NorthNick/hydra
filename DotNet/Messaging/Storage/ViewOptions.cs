using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Shastra.Hydra.Messaging.Storage
{
    public class ViewOptions : IViewOptions
    {
        #region Implementation of IViewOptions

        public bool? IncludeDocs { get; set; }
        public int? Limit { get; set; }
        public IKeyOptions StartKey { get; set; }
        public IKeyOptions EndKey { get; set; }
        public IEnumerable<IKeyOptions> Keys { get; set; }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            var options = new List<string>();
            if (IncludeDocs.HasValue) options.Add(string.Format("include_docs={0}", IncludeDocs.Value.ToString().ToLower()));
            if (Limit.HasValue) options.Add(string.Format("limit={0}", Limit.Value));
            if (StartKey != null) options.Add(string.Format("startkey={0}", StartKey));
            if (EndKey != null) options.Add(string.Format("endkey={0}", EndKey));
            if (Keys != null) options.Add(string.Format("keys=[{0}]", String.Join(",", Keys.Select(k => k.ToString()).ToArray())));
            return string.Join("&", options);
        }

        #endregion
    }

    public class KeyOptions : IKeyOptions
    {
        private readonly string[] _options;

        public static readonly object MaxValue = new object();

        public KeyOptions(params object[] options)
        {
            _options = options.Select(option => option == MaxValue ? "{}" : HttpUtility.UrlEncode(new JValue(option).ToString(Formatting.None, new IsoDateTimeConverter()))).ToArray();
        }

        #region Overrides of Object

        public override string ToString()
        {
            switch (_options.Length) {
                case 0:
                    return "";
                case 1:
                    return _options[0];
                default:
                    return string.Format("[{0}]", string.Join(",", _options));
            }
        }

        #endregion
    }

}