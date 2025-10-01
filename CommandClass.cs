//CommandClass.cs
using Discord.Commands;

namespace SecondBot;

public class CommandClass : ModuleBase
{
    [Command("hi")]
    public async Task Reply()
    {
        await ReplyAsync("hello");
    }

    [Command("返して")]
    public async Task Echo(string str)
    {
        await ReplyAsync(str);
    }
}
