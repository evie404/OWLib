using System.Runtime.Serialization;
using DataTool.Helper;

namespace DataTool.DataModels {
    /// <summary>
    ///     UXDisplayText data model
    /// </summary>
    [DataContract]
    public class UXDisplayText {
        /// <summary>
        ///     String value
        /// </summary>
        [DataMember]
        public string Value;

        public UXDisplayText(ulong guid) { Value = IO.GetString(guid); }
    }
}
