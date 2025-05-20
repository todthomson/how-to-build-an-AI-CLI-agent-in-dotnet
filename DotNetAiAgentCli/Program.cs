using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;

namespace DotNetAiAgentCli
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var client = new AnthropicClient();
            var agent = new Agent(client, GetUserMessage);

            try
            {
                await agent.RunAsync(CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return;

            (string, bool) GetUserMessage()
            {
                var input = Console.ReadLine();
                return input != null ? (input, true) : (string.Empty, false);
            }
        }
    }

    public class Agent
    {
        private readonly AnthropicClient _client;
        private readonly Func<(string message, bool success)> _getUserMessage;

        public Agent(AnthropicClient client, Func<(string, bool)> getUserMessage)
        {
            _client = client;
            _getUserMessage = getUserMessage;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var conversation = new List<MessageParam>();

            Console.WriteLine("Chat with Claude (use 'ctrl-c' to quit)");

            while (true)
            {
                Console.Write("\u001b[94mYou\u001b[0m: ");
                var (userInput, ok) = _getUserMessage();
                if (!ok)
                {
                    break;
                }

                var userMessage = new UserMessage(new TextBlock(userInput));
                conversation.Add(userMessage);

                var message = await RunInferenceAsync(conversation, cancellationToken);
                if (message == null)
                {
                    return;
                }

                conversation.Add(message.ToParam());

                foreach (var content in message.Content)
                {
                    switch (content.Type)
                    {
                        case "text":
                            Console.WriteLine($"\u001b[93mClaude\u001b[0m: {content.Text}");
                            break;
                    }
                }
            }
        }

        private async Task<Message?> RunInferenceAsync(List<MessageParam> conversation, CancellationToken cancellationToken)
        {
            try
            {
                var message = await _client.Messages.CreateAsync(new MessageCreateParams
                {
                    Model = "claude-3-7-sonnet-20250219", // Using the model identifier directly
                    MaxTokens = 1024,
                    Messages = conversation
                }, cancellationToken);

                return message;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Inference error: {ex.Message}");
                return null;
            }
        }
    }
}
