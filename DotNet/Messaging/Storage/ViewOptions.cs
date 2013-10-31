using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Shastra.Hydra.Messaging.Storage
{
    public class ViewOptions : IViewOptions
    {
        #region Implementation of IViewOptions

        public bool IncludeDocs { get; set; }
        public IKeyOptions StartKey { get; set; }
        public IKeyOptions EndKey { get; set; }
        public IEnumerable<IKeyOptions> Keys { get; set; }
        public int? Limit { get; set; }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            var res = new StringBuilder();
            res.AppendFormat("&include_docs={0}", IncludeDocs.ToString().ToLower());
            if (Limit.HasValue) res.AppendFormat("&limit={0}", Limit.Value);
            if (StartKey != null)
                res.AppendFormat("&startkey={0}", StartKey);
            if (EndKey != null)
                res.AppendFormat("&endkey={0}", EndKey);
            if (Keys != null)
                res.AppendFormat("&keys=[{0}]", String.Join(",", Keys.Select(k => k.ToString()).ToArray()));
            return res.ToString();
        }

        #endregion
    }

    public class KeyOptions : IKeyOptions
    {
        private readonly object[] _options;

        public static readonly object MaxValue = new object();

        public KeyOptions(params object[] options)
        {
            _options = options;
        }

        #region Overrides of Object

        public override string ToString()
        {
            switch (_options.Length) {
                case 0:
                    return "";
                case 1:
                    return HttpUtility.UrlEncode(new JValue(_options[0]).ToString(Formatting.None, new IsoDateTimeConverter()));
                default:
                    return string.Format("[{0}]", string.Join(",",
                        _options.Select(option => option == MaxValue ? "{}" : HttpUtility.UrlEncode(new JValue(option).ToString(Formatting.None, new IsoDateTimeConverter())))));
            }
        }

        #endregion
    }

}