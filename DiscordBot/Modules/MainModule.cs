using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

		public static class ShellHelper
		{
			public static string Bash(string cmd)
			{
				var escapedArgs = cmd.Replace("\"", "\\\"");

				var process = new Process()
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = "/bin/bash",
						Arguments = $"-c \"{escapedArgs}\"",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true,
					}
				};
				process.Start();
				string result = process.StandardOutput.ReadToEnd();
				process.WaitForExit();
				return result;
			}
		}

		[Command("uptime")]
		public async Task UptimeAsync()
		{
			var processStart = Process.GetCurrentProcess().StartTime;
			var procUptime = DateTime.Now.Subtract(processStart);

			var hostUptime = ShellHelper.Bash("cat /proc/uptime");
			var hostTimeStr = hostUptime.Substring(0, hostUptime.IndexOf(" "));
			var hostTimeSeconds = double.Parse(hostTimeStr);
			var hostTime = TimeSpan.FromSeconds(hostTimeSeconds);

			var embed = new EmbedBuilder();
			embed.WithTitle("Uptime");
			embed.AddField("BOT", procUptime.Humanize(2), true);
			embed.AddField("HOST", hostTime.Humanize(2), true);
			await ReplyAsync(embed: embed.Build());
		}

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
