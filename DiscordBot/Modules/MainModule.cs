using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	public class PublicModule : ModuleBase<SocketCommandContext>
	{
		private DiscordSocketClient _client;

		public PublicModule(DiscordSocketClient client)
		{
			_client = client;
		}
		const ulong OPERATOR_ROLE_ID = 702744926466736179;
		[Command("ping")]
		public Task PingAsync() => ReplyAsync("pong!");

		[Command("version")]
		public async Task VersionAsync()
		{
			var hash = Environment.GetEnvironmentVariable("GIT_HASH");
			var date = Environment.GetEnvironmentVariable("GIT_DATE");
			if (string.IsNullOrEmpty(hash))
				await ReplyAsync("No version info. Maybe this is a dev build?");
			else
			{
				await ReplyAsync(
$@"Metaverse DiscordBot built from: 
> <https://github.com/MetaverseAC/DiscordBot/commit/{hash}>
> SHA: `{hash}`
> Date: {date}"
				);
			}
		}

		[Command("echo")]
		[RequireRole(OPERATOR_ROLE_ID)]
		public async Task EchoAsync(ulong channelId, [Remainder] string text)
		{
			var channel = _client.GetChannel(channelId) as ISocketMessageChannel;
			if (channel != null)
			{
				var echo = await channel.SendMessageAsync(text);
				await ReplyAsync("Done:" + echo.GetJumpUrl());
			}
			else
			{
				await ReplyAsync("Could not access channel.");
			}
		}
	}
}
