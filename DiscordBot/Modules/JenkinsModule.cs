using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
	public class JenkinsModule : ModuleBase<SocketCommandContext>
	{
		private DiscordSocketClient _client;
		private JenkinsService _jenkins;
		const ulong OPERATOR_ROLE_ID = 702744926466736179;

		public JenkinsModule(DiscordSocketClient client, JenkinsService jenkins)
		{
			_client = client;
			_jenkins = jenkins;
		}

		[Command("redeploy", RunMode = RunMode.Async)]
		[Alias("update")]
		[RequireRole(OPERATOR_ROLE_ID)]
		public async Task RedeployAsync()
		{
			await _jenkins.RunJobAsync("Deploy_ACE_PTR");
			await ReplyAsync("Redeploying PTR with latest version.");
		}


		private readonly string[] PR_WHITELIST =
		{
			"https://github.com/ACEmulator/ACE",
			"https://github.com/MetaverseAC/ACE"
		};

		[Command("deploy", RunMode = RunMode.Async)]
		[Alias("pr")]
		[RequireRole(OPERATOR_ROLE_ID)]
		public async Task DeployPrAsync(string uri)
		{
			uri = uri.TrimStart('<').TrimEnd('>'); // allow suppressing embeds

			var regex = new Regex(@"\.com/(.*)/pull/(\d+)");
			var match = regex.Match(uri);

			if (!match.Success || !PR_WHITELIST.Any(prefix => uri.StartsWith(prefix)))
			{
				await ReplyAsync("Not a valid PR URL, contact @deca.");
				return;
			}

			var args = new Dictionary<string, string>()
			{
				{ "GH_PR_URL",uri }
			};

			var repo = match.Groups[1];
			var pr = match.Groups[2];

			await _jenkins.RunJobAsync("ACE_PR", args);
			
			var embed = new EmbedBuilder()
			{
				Title = $"Deploying {repo} PR-{pr}",
				Description = "Use this info to connect:",
				Url = uri,
			};
			
			embed.WithFooter("Should be available in a few minutes.");
			embed.AddField("name", "Metaverse PR Test", true);
			embed.AddField("host", "play.metaverse.ac", true);
			embed.AddField("port", "9322", true);
			
			await ReplyAsync(embed: embed.Build());
		}
	}
}
