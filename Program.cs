//Program.cs
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FirstBot;

class Program
{
#pragma warning disable CS8601 // Possible null reference assignment.
    string Token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
    DiscordSocketClient? Client;
    CommandService Commands = new CommandService();
    ServiceProvider? Services;
    
    private Database db = new Database();
    
    static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();
    public async Task MainAsync()
    {

        var config = new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Info,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent//これがないと会話を取得できない!!!!
        };
        //今回はログ出す。
        Client = new DiscordSocketClient(config);
        Client.Log += Log;
        Client.MessageReceived += MessageReceived;

        //Microsoft.Extensions.DependencyInjectionを調べてみよう
        Commands = new CommandService();
        Services = new ServiceCollection().BuildServiceProvider();
        await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

        await Client.LoginAsync(TokenType.Bot, Token);
        await Client.StartAsync();
        await Task.Delay(-1);
    }

    //ログ吐き出し！
    private Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

    private async Task MessageReceived(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;

        if (message == null)
        {
            return;
        }
        Console.WriteLine(message.Content);
        if (message.Author.IsBot)
        {
            return;
        }

        int argPos = 0;

        //コマンドかどうか判定
        /*if (!(message.HasCharPrefix('!', ref argPos)))
        {
            return;
        }*/

        var context = new CommandContext(Client, message);
        //コマンド実行
        var result = await Commands.ExecuteAsync(context, argPos, Services);

        if (!result.IsSuccess)
        {
            await NotCommandMessage(messageParam);
        }
    }


    /*private async Task NotCommandMessage(SocketMessage messageParam)
    {
        string text = messageParam.Content;
        if (text.Contains("おはよう")) await messageParam.Channel.SendMessageAsync("おはようです！");
    }*/


    private async Task NotCommandMessage(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null || message.Author.IsBot) return;

        var content = message.Content.Trim();

        if (content.StartsWith("!addnote "))
        {
            // !addnote タイトル:本文:タグ1,タグ2
            var parts = content.Substring(9).Split(':');
            if (parts.Length < 3) return;

            string title = parts[0];
            string body = parts[1];
            string[] tags = parts[2].Split(',');

            db.AddNote(title, body, tags);
            await message.Channel.SendMessageAsync("ノートを追加しました！");
        }

        if (content.StartsWith("!search "))
        {
            string keyword = content.Substring(8);
            var results = db.SearchNotes(keyword);
            foreach (var note in results)
                await message.Channel.SendMessageAsync($"[{note.Title}] {note.Content}");
        }
    }

}

