﻿using Locksmith.Core.Factory;
using Locksmith.Core.Model.Core;
using Locksmith.Core.Model.Platform;
using Locksmith.Core.Model.Template;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Locksmith.CLI.Command
{
    public class TestSecretCommand : Command<TestSecretCommand.Settings>
    {
        private readonly IAnsiConsole _console;

        public class Settings : CommandSettings
        {
            [CommandArgument(0, "[EXAMPLE]")]
            [Description("The example to run.\nIf none is specified, all examples will be listed")]
            public string Name { get; set; }

            [CommandOption("-s|--secret")]
            [Description("Secret to test")]
            public bool Secret { get; set; }

            [CommandOption("-t|--Test")]
            [Description("Select test do you want")]
            public int Test { get; set; }

        }


        public TestSecretCommand(IAnsiConsole console)
        {
            _console = console;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
           Boot().GetAwaiter().GetResult();
            return 0;
        }



        private async Task Boot()
        {
            var selectedTestOption = PromptTest();

            var userParameters = PromptUserParameter(selectedTestOption);

            TestSecret(selectedTestOption, userParameters);

        }

        private void TestSecret(TemplateModel testTemplate, Dictionary<int, Dictionary<int, string>> secretParameters)
        {
            List<KeychainTestResult> testResultList = new List<KeychainTestResult>();

            _console.MarkupLine($"[bold yellow][[+]] [/][bold yellow]Testing {testTemplate.title}:[/]");

            _console.Status()
                            .Start("[bold yellow] | [/][yellow] Testing...[/]", ctx =>
                            {
                                ctx.Status("[bold yellow] Testing secret agains api, please wait...[/]");
                                Thread.Sleep(1000);
                                testResultList = KeyChainTesterFactory.TestSecret(testTemplate, secretParameters);
                            });

            _console.MarkupLine($"[bold yellow] | [/][bold] See the results bellow:[/]");


            foreach (var result in testResultList)
            {
                string SuccessLabel = result.Success ? "Success" : "Failed";
                string Color = result.Success ? "blue" : "red";
                string Content = result.RawResponse.Replace("[", "]]").Replace("]", "]]");

                _console.MarkupLine($"[bold yellow] | [/][bold] Status:[/] [{Color} bold]{SuccessLabel}[/]");
                _console.MarkupLine($"[bold yellow] | [/][bold] Output:[/]");


                foreach (string line in Content.Split('\n'))
                {
                    _console.MarkupLine($"[bold gray]{line.Replace("[", "]]").Replace("]", "]]")}[/]");
                }

            }
            
        }

        private Dictionary<int, Dictionary<int, string>> PromptUserParameter(TemplateModel testTemplate)
        {
            Dictionary<int, Dictionary<int, string>> result = new Dictionary<int, Dictionary<int, string>>();
            foreach (var commands in testTemplate.commands)
            {
                Dictionary<int, string> parameters = new Dictionary<int, string>();
                foreach (var parameter in commands.parameters)
                {
                    var userInput = _console.Prompt(
                         new TextPrompt<string>($"[grey][[required]][/] [bold red]{parameter.description}[/]? [grey]{parameter.default_value}[/]"));

                    parameters.Add(parameters.Count, userInput);
                }
                result.Add(result.Count, parameters);
            }

            return result;


        }

        private TemplateModel PromptTest()
        {

            var templates = Locksmith.Core.Factory.TemplatesFactory.GetTemplatesFromFolder(".\\templates").OrderBy(x => x.title).ToList();

            var options = templates.Select((obj, index) => $"{index + 1 } - {obj.title}");


            var selectedTest = _console.Prompt(
                            new SelectionPrompt<string>()
                                .HighlightStyle("red")
                                .Title("[grey][[required]][/] [bold red]Which test do you want to execute[/]?")
                                .PageSize(20)
                                .MoreChoicesText("[grey](Move up and down to reveal more test type)[/]")
                                .AddChoiceGroup("Available tests:", options));

            _console.MarkupLine($"[grey][[required]][/] [bold red]Which test do you want to execute[/]? {selectedTest}");

            return templates[Convert.ToInt32(selectedTest.Split(' ')[0]) - 1];
            //
        }
    }
}
