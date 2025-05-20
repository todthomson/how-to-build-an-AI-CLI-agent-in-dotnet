// import (
//     "bufio"
//     "context"
//     "fmt"
//     "os"
//     "github.com/anthropics/anthropic-sdk-go"
// )
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

// package main
namespace DotNetAiAgentCli
{
    public static class Program
    {
        private const string Welcome = "\e[94mThis is a REPL in C#/.NET with the Anthropic (Claude) API.\e[0m";
        private const string Prompt = "\e[95m? \e[96m(press \e[93mENTER \e[96mor \e[93mCTRL+C \e[96mto exit): \e[0m";

        // func main() {
        public static async Task Main()
        {
            // client := anthropic.NewClient()
            var anthropicClient = new AnthropicClient();

            // conversation := []anthropic.MessageParam{}
            var conversation = new List<Message>
            {
                // new(RoleType.User, "Who won the world series in 2020?"),
                // new(RoleType.Assistant, "The Los Angeles Dodgers won the World Series in 2020."),
                // new(RoleType.User, "Where was it played?"),
            };

            // fmt.Println("Chat with Claude (use 'ctrl-c' to quit)")
            Console.WriteLine(Welcome);

            var messageParameters = new MessageParameters
            {
                Messages = conversation,
                MaxTokens = 1024,
                Model = AnthropicModels.Claude37Sonnet,
                Stream = true,
                Temperature = 1.0m,
            };

            // for {
            while (true)
            {
                // fmt.Print("\u001b[94mYou\u001b[0m: ")
                Console.Write($"{Environment.NewLine}{Prompt}");

                // userInput, ok := a.getUserMessage()
                var input = Console.ReadLine();
                Console.WriteLine();

                if (input == string.Empty)
                {
                    Console.WriteLine("\e[91mExiting...");
                    return;
                }

                // userMessage := anthropic.NewUserMessage(anthropic.NewTextBlock(userInput))
                var userMessage = new Message(RoleType.User, input);

                // conversation = append(conversation, userMessage)
                conversation.Add(userMessage);

                var responses = new List<MessageResponse>();

                // message, err := a.runInference(ctx, conversation)
                // if err != nil {
                //     return err
                // }
                await foreach (var messageResponse in anthropicClient.Messages.StreamClaudeMessageAsync(messageParameters))
                {
                    // for _, content := range message.Content {
                    //     switch content.Type {
                    //         case "text":
                    //         fmt.Printf("\u001b[93mClaude\u001b[0m: %s\n", content.Text)
                    //     }
                    // }

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
