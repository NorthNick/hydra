using System;
using System.Runtime.Serialization;
using Shastra.Hydra.Messaging;

namespace Shastra.Hydra.Tests.Mocks
{
    /// <summary>
    /// Extension of HydraMessage with testing-specific fields
    /// </summary>
    class TestHydraMessage : HydraMessage
    {
        /// <summary>
        /// Generate the DocId from this date instead of the current date.
        /// </summary>
        [DataMember]
        public DateTime IdDate { get; set; }
    }
}
