using ResXTranslator.Parsers;
using ResXTranslator.ResxHandler;
using ResXTranslator.TranslationServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ResxTranslator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Func<CLIOptions, Task> RunAction = async (options) => await Run(options);
            CLIParser.Parse(new List<string>(args), RunAction);
        }

        static async Task Run(CLIOptions options)
        {
            IResxHandler resxHandler = new SimpleResxHandler();
            ITranslator translator;
            if (options.APIKeyPath == null)
                translator = new GoogleTranslation();
            else
                translator = new GoogleTranslation(options.APIKeyPath);

            var resources = resxHandler.Read(options.FilePath);
            foreach (var lge in options.TranslationLanguages)
            {
                var outputFileName = $"{Path.GetFileNameWithoutExtension(options.FilePath)}.{lge}{Path.GetExtension(options.FilePath)}";
                var outputFile = Path.Combine(options.OutPutPath, outputFileName);
                var dic = new Dictionary<string, string>();
                if (options.UseExistingTranslation && File.Exists(outputFile))
                {
                    var existingValues = resxHandler.Read(outputFile);
                    foreach (var resource in resources.ToList())
                    {
                        var key = resource.Key;
                        if (existingValues.ContainsKey(key))
                        {
                            dic.Add(key, existingValues[key]);
                            resources.Remove(resource.Key);
                        }
                    }
                }

                var translationResults = await translator.TranslateAsync(resources.Values, lge, options.ResourceLanguage).ConfigureAwait(false);
                for (int i = 0; i < translationResults.Count(); i++)
                {
                    var key = resources.Keys.ElementAt(i);
                    dic.Add(key, translationResults.ElementAt(i));
                }
                resxHandler.Create(dic, outputFileName, options.OutPutPath);
                Console.WriteLine($"Translations finished and file created: ${outputFileName}");
            }
        }
    }
}
