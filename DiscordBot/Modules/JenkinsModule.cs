using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
	}
}
