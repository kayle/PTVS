using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if DEV18_OR_LATER
using System.Text.Json;
#endif
using LSP = Microsoft.VisualStudio.LanguageServer.Protocol;

namespace Microsoft.PythonTools.LanguageServerClient {
#if DEV18_OR_LATER
    internal static class ProtocolJsonSerialization {
        private static JsonSerializerOptions _options;

        internal static JsonSerializerOptions Options =>
            _options ?? throw new InvalidOperationException("The language server JSON formatter has not been configured.");

        internal static void Configure(JsonSerializerOptions options) {
            if (options == null) {
                throw new ArgumentNullException(nameof(options));
            }
            Microsoft.VisualStudio.LanguageServer.Protocol.ProtocolJsonSerializerOptions.Configure(options);
            _options = options;
        }
    }
#endif

    /// <summary>
    /// Class used for converting LSP results from JSON structures to LSP objects
    /// </summary>
    internal static class ResultConverter {
        internal static object[] ConvertResult(object result) {
#if DEV18_OR_LATER
            if (result is JsonDocument jsonDocument) {
                return ConvertResult(jsonDocument.RootElement);
            }
            if (result is JsonElement jsonElement) {
                return ConvertResult(jsonElement);
            }
            if (result is JToken token) {
                using (var parsedDocument = JsonDocument.Parse(token.ToString(Formatting.None))) {
                    return ConvertResult(parsedDocument.RootElement);
                }
            }
            return Array.Empty<object>();
#else
            return result is JToken token ? ConvertResult(token) : Array.Empty<object>();
#endif
        }

#if DEV18_OR_LATER
        private static object[] ConvertResult(JsonElement jsonElement) {
            if (jsonElement.ValueKind == JsonValueKind.Array) {
                var first = jsonElement.EnumerateArray().FirstOrDefault();
                if (IsDocumentSymbol(first)) {
                    return Deserialize<LSP.DocumentSymbol[]>(jsonElement);
                } else if (IsSymbolInformation(first)) {
                    return Deserialize<LSP.SymbolInformation[]>(jsonElement);
                } else if (IsLocation(first)) {
                    return Deserialize<LSP.Location[]>(jsonElement);
                }
            } else if (jsonElement.ValueKind == JsonValueKind.Object) {
                if (IsDocumentSymbol(jsonElement)) {
                    return new LSP.DocumentSymbol[] { Deserialize<LSP.DocumentSymbol>(jsonElement) };
                } else if (IsSymbolInformation(jsonElement)) {
                    return new LSP.SymbolInformation[] { Deserialize<LSP.SymbolInformation>(jsonElement) };
                } else if (IsLocation(jsonElement)) {
                    return new LSP.Location[] { Deserialize<LSP.Location>(jsonElement) };
                } else if (IsCompletionList(jsonElement)) {
                    return new LSP.CompletionList[] { Deserialize<LSP.CompletionList>(jsonElement) };
                }
            }

            return Array.Empty<object>();
        }

        private static T Deserialize<T>(JsonElement jsonElement) =>
            System.Text.Json.JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), ProtocolJsonSerialization.Options);

        private static bool IsDocumentSymbol(JsonElement jobject) =>
            jobject.ValueKind == JsonValueKind.Object &&
            jobject.TryGetProperty("range", out _) &&
            jobject.TryGetProperty("kind", out _);

        private static bool IsSymbolInformation(JsonElement jobject) =>
            jobject.ValueKind == JsonValueKind.Object &&
            jobject.TryGetProperty("location", out _) &&
            jobject.TryGetProperty("kind", out _);

        private static bool IsLocation(JsonElement jobject) =>
            jobject.ValueKind == JsonValueKind.Object &&
            jobject.TryGetProperty("uri", out _);

        private static bool IsCompletionList(JsonElement jobject) =>
            jobject.ValueKind == JsonValueKind.Object &&
            jobject.TryGetProperty("isIncomplete", out _);
#else
        private static object[] ConvertResult(JToken jToken) {
            if (jToken != null) {
                var jsonSerializer = new JsonSerializer();

                if (jToken.Type == JTokenType.Array) {
                    var jArray = (JArray)jToken;
                    if (jArray.Any()) {
                        if (IsDocumentSymbol(jArray.First)) {
                            return jArray.ToObject<LSP.DocumentSymbol[]>(jsonSerializer);
                        } else if (IsSymbolInformation(jArray.First)) {
                            return jArray.ToObject<LSP.SymbolInformation[]>(jsonSerializer);
                        } else if (IsLocation(jArray.First)) {
                            return jArray.ToObject<LSP.Location[]>(jsonSerializer);
                        }
                    }
                } else if (jToken.Type == JTokenType.Object) {
                    var jObject = (JObject)jToken;
                    if (IsDocumentSymbol(jObject)) {
                        return new LSP.DocumentSymbol[] { jObject.ToObject<LSP.DocumentSymbol>(jsonSerializer) };
                    } else if (IsSymbolInformation(jObject)) {
                        return new LSP.SymbolInformation[] { jObject.ToObject<LSP.SymbolInformation>(jsonSerializer) };
                    } else if (IsLocation(jObject)) {
                        return new LSP.Location[] { jObject.ToObject<LSP.Location>(jsonSerializer) };
                    } else if (IsCompletionList(jObject)) {
                        return new LSP.CompletionList[] { jObject.ToObject<LSP.CompletionList>(jsonSerializer) };
                    }
                }
            }

            return Array.Empty<object>();
        }

        private static bool IsDocumentSymbol(JToken jobject) {
            return (jobject != null && jobject["range"] != null && jobject["kind"] != null);
        }
        private static bool IsSymbolInformation(JToken jobject) {
            return (jobject != null && jobject["location"] != null && jobject["kind"] != null);
        }
        private static bool IsLocation(JToken jobject) {
            return (jobject != null && jobject["uri"] != null);
        }

        private static bool IsCompletionList(JToken jobject) {
            return (jobject != null && jobject["isIncomplete"] != null);
        }
#endif
    }
}
