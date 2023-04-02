using Discord;
using Discord.WebSocket;
using GPTSharp;
using GPTSharp.Models.Chat.Requests;

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

        // обработка сообщения
        if (message.Content.StartsWith("gpt"))
        {
            var openAi = new OpenAIService("your_openai_key");

            var messageOpenAi = message.Content.Substring(4, message.Content.Length - 4);
            var result =
                await openAi.ChatEndpoint.GetChatCompletions(RequestChatCompletions.SimpleMessage(messageOpenAi));

            if (result.IsSuccess)
            {
                var answerOpenAi = result.SuccessResult.GetText();
                await message.Channel.SendMessageAsync(answerOpenAi);
            }
        }
    }
}