using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace _01_basic_ping_bot
{
	class Program
	{
		// Discord.Net heavily utilizes TAP for async, so we create
		// an asynchronous context from the beginning.
		static void Main(string[] args)
		{
			new Program().MainAsync().GetAwaiter().GetResult();
		}
		
		public async Task MainAsync()
		{

			Console.WriteLine($"Built from: {Environment.GetEnvironmentVariable("GIT_HASH") ?? "not set"}");
			Console.WriteLine($"Built on: {Environment.GetEnvironmentVariable("GIT_DATE") ?? "not set"}");

			using (var services = ConfigureServices())
			{
				var client = services.GetRequiredService<DiscordSocketClient>();

				client.Log += LogAsync;
				services.GetRequiredService<CommandService>().Log += LogAsync;

				// Tokens should be considered secret data and never hard-coded.
				// We can read from the environment variable to avoid hardcoding.
				await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BOT_TOKEN"));
				await client.StartAsync();
				
				// Initialize Services
				await services.GetRequiredService<CommandHandlingService>().InitializeAsync();				
				services.GetRequiredService<ReactionRoleService>();								
				await Task.Delay(Timeout.Infinite);
			}
		}

		private Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices()
		{
			return new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlingService>()
				.AddSingleton<HttpClient>()
				.AddSingleton<ReactionRoleService>()
				.BuildServiceProvider();
		}
	}
}