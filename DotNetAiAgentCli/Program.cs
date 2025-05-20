using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

namespace DotNetAiAgentCli
{
    public static class Program
    {
        private const string Welcome = "\e[94mThis is a AI CLI REPL in C#/.NET using the Anthropic (Claude) API.\e[0m";
        private const string Prompt = "\e[95m? \e[96m(press \e[93mENTER \e[96mor \e[93mCTRL+C \e[96mto exit): \e[0m";

        public static async Task Main()
        {
            var anthropicClient = new AnthropicClient();
            var conversation = new List<Message>();

            Console.WriteLine(Welcome);

            var messageParameters = new MessageParameters
            {
                Messages = conversation,
                MaxTokens = 1024,
                Model = AnthropicModels.Claude37Sonnet,
                Stream = true,
                Temperature = 1.0m,
            };

            while (true)
            {
                Console.Write($"{Environment.NewLine}{Prompt}");
                var input = Console.ReadLine();
                Console.WriteLine();

                if (input == string.Empty)
                {
                    Console.WriteLine("\e[91mExiting...");
                    return;
                }

                var userMessage = new Message(RoleType.User, input);
                conversation.Add(userMessage);

                var responses = new List<MessageResponse>();
                await foreach (var messageResponse in anthropicClient.Messages.StreamClaudeMessageAsync(messageParameters))
                {
                    if (messageResponse != null)
                    {
                        responses.Add(messageResponse);

                        if (messageResponse.Delta != null)
                        {
                            Console.Write(messageResponse.Delta.Text);
                        }
                    }
                }

                var responseChars = responses
                    .Where(r => r.Delta is { Text: not null })
                    .SelectMany(r => r.Delta.Text)
                    .ToArray();

                var responseText = new string(responseChars);
                var responseMessage = new Message(RoleType.Assistant, responseText);
                conversation.Add(responseMessage);
                Console.WriteLine();
            }
        }
    }
}
