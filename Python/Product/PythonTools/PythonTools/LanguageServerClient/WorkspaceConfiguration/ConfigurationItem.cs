using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#if !DEV18_OR_LATER
using Newtonsoft.Json;
#endif

namespace Microsoft.PythonTools.LanguageServerClient.WorkspaceConfiguration {
    /// <summary>
    /// Until VSSDK supports this, this param is for receiving workspace/configuration events
    /// </summary>
    [DataContract]
    internal class ConfigurationItem {
        [DataMember]
#if !DEV18_OR_LATER
        [JsonConverter(typeof(Microsoft.VisualStudio.LanguageServer.Protocol.DocumentUriConverter))]
#endif
        public Uri scopeUri;

        [DataMember]
        public string section;
    }
}
