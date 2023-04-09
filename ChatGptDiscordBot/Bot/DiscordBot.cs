using System.Net;
using Discord;
using Discord.WebSocket;
using GPTSharp;
using GPTSharp.Models;
using GPTSharp.Models.Chat.Requests;
using Attachment = System.Net.Mail.Attachment;

namespace ChatGptDiscordBot.Bot;

public class DiscordBot
{
    private DiscordSocketClient _client;
    private const string Token = "your_token_discord";

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.All
        });
        _client.MessageReceived += HandleMessageReceived;

        await _client.LoginAsync(TokenType.Bot, Token);
        await _client.StartAsync();

        // ожидание команд
        await Task.Delay(-1);
    }

    private async Task HandleMessageReceived(SocketMessage message)
    {
        // проверка, что сообщение не отправлено ботом
        if (message.Author.Id == _client.CurrentUser.Id)
            return;

        // обрабатываем сообщение асинхронно
        await Task.Run(async () =>
        {
            var typingState = message.Channel.EnterTypingState();
            await MessageHadler(message);
            typingState.Dispose();
        });
        
    }

    private static async Task MessageHadler(SocketMessage message)
    {
        var openAi = new OpenAIService("your_openai_key");
        // обработка сообщения 
        if (message.Content.StartsWith("gpt-image"))
        {
            await ImageGenerationHadler(openAi, message);
        }
        else if (message.Content.StartsWith("gpt"))
        {
            await TextHadler(openAi, message);
        }
    }

    private static async Task ImageGenerationHadler(OpenAIService openAi, SocketMessage message)
    {
        var messageOpenAi = message.Content.Substring(10, message.Content.Length - 10);
        var result = await openAi.ImagesEndpoint.Generations(messageOpenAi, 2);
        if (result.IsSuccess)
        {
            var answerOpenAi = result.SuccessResult.Data.ToList();

            var embedList = new List<Embed>();
            foreach (var image in answerOpenAi)
            {
                var builder = new EmbedBuilder();
                builder.WithImageUrl(image.Url);
                var embed = builder.Build();
                embedList.Add(embed);
            }

            await message.Channel.SendMessageAsync(messageReference: new MessageReference(message.Id),
                embeds: embedList.ToArray());
        }
        else
            await SendError(result.ErrorResult, message.Channel);
    }

    private static async Task TextHadler(OpenAIService openAi, SocketMessage message)
    {
        var messageOpenAi = message.Content.Substring(4, message.Content.Length - 4);
        var result =
            await openAi.ChatEndpoint.GetChatCompletions(RequestChatCompletions.SimpleMessage(messageOpenAi));

        if (result.IsSuccess)
        {
            var answerOpenAi = result.SuccessResult.GetText();
            await message.Channel.SendMessageAsync(answerOpenAi,
                messageReference: new MessageReference(message.Id));
        }
        else
            await SendError(result.ErrorResult, message.Channel);
    }

    private static async Task SendError(ErrorResult errorResult, ISocketMessageChannel socketMessageChannel)
    {
        await socketMessageChannel.SendMessageAsync(errorResult.Error.Message);
    }
}