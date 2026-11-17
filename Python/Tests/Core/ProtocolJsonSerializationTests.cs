#if DEV18_OR_LATER
using System.Text.Json;
using Microsoft.PythonTools.LanguageServerClient;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StreamJsonRpc;

namespace PythonToolsTests {
    [TestClass]
    public class ProtocolJsonSerializationTests {
        [TestMethod]
        public void ConvertsCompletionDocumentationUnion() {
            var formatter = new SystemTextJsonFormatter();
            ProtocolJsonSerialization.Configure(formatter.JsonSerializerOptions);

            using (var json = JsonDocument.Parse(
                "{\"isIncomplete\":false,\"items\":[{\"label\":\"item\",\"documentation\":{\"kind\":\"markdown\",\"value\":\"details\"}}]}")) {
                var completionLists = ResultConverter.ConvertResult(json) as CompletionList[];

                Assert.IsNotNull(completionLists);
                var markupContent = completionLists[0].Items[0].Documentation.Value as MarkupContent;

                Assert.IsNotNull(markupContent);
                Assert.AreEqual("details", markupContent.Value);
            }
        }
    }
}
#endif
