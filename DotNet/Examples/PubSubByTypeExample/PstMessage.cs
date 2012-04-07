using System;
using System.Runtime.Serialization;

namespace PubSubByTypeExample
{
    [DataContract(Namespace = "")]
    class PstMessage
    {
        [DataMember] public string StringField { get; set; }
        [DataMember] public long LongField { get; set; }
        [DataMember] public DateTime DateField { get; set; }

        public override string ToString()
        {
            return string.Format("String: {0}\nLong: {1}\nDate: {2}", StringField, LongField, DateField);
        }
    }
}
