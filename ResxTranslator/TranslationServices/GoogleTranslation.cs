using Google.Cloud.Translate.V3;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System;
using Google.Api.Gax.ResourceNames;

namespace ResXTranslator.TranslationServices
{
    public class GoogleTranslation : ITranslator
    {
        TranslationServiceClient _client;
        string projectId;

        public GoogleTranslation(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("Api key not found.");
                throw new FileNotFoundException($"File {path} was not found.");
            }
            var secretKeyText = File.ReadAllText(path);
            var secretKey = JsonConvert.DeserializeObject<Dictionary<string, string>>(secretKeyText);
            projectId = secretKey["project_id"];
            _client = new TranslationServiceClientBuilder
            {
                CredentialsPath = path
            }.Build();
        }

        public GoogleTranslation()
        {
            _client = TranslationServiceClient.Create();
        }

        public IList<SupportedLanguage> ListLanguages()
        {
            var request = new GetSupportedLanguagesRequest
            {
                Parent = new ProjectName(projectId).ToString()
            };
            var languages = _client.GetSupportedLanguages(request).Languages.ToList();
            foreach (var language in languages)
            {
                Console.WriteLine("{0}\t{1}", language.LanguageCode, language.DisplayName);
            }
            return languages;
        }
        public async Task<string> TranslateAsync(string text, string language, string sourceLanguage = null)
        {
            string translation = string.Empty;
            try
            {
                Console.WriteLine($"Translating {text} to {language}");

                if (string.IsNullOrEmpty(text))
                {
                    return string.Empty;
                }

                TranslateTextRequest request = new TranslateTextRequest
                {
                    Contents = { text },
                    TargetLanguageCode = language,
                    Parent = new ProjectName(projectId).ToString()
                };

                var result = await _client.TranslateTextAsync(request);
                var translationResult = result.Translations.FirstOrDefault();

                Console.WriteLine($"Translated {text} from {translationResult?.DetectedLanguageCode} to {language}");
                translation = translationResult.TranslatedText;
                await Task.Delay(1000).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured while translating. {e.Message}");
                Environment.Exit(-1);
            }

            return translation;
        }

        public async Task<IEnumerable<string>> TranslateAsync(IEnumerable<string> texts, string language, string sourceLanguage = null)
        {
            List<string> translations = new List<string>();
            foreach (var t in  texts)
            {
                var res = await TranslateAsync(t, language, sourceLanguage);
                translations.Add(res);
            }

            return translations;
        }
    }
}