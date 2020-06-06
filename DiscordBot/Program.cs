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
				client.Disconnected += Client_Disconnected;
				services.GetRequiredService<CommandService>().Log += LogAsync;

				// Tokens should be considered secret data and never hard-coded.
				// We can read from the environment variable to avoid hardcoding.
				await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("BOT_TOKEN"));
				await client.StartAsync();

				// Initialize Services
				await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
				services.GetRequiredService<ReactionRoleService>();
				services.GetRequiredService<JenkinsService>().Initiaize(Environment.GetEnvironmentVariable("JENKINS_TOKEN"));
				await Task.Delay(Timeout.Infinite);
			}
		}

		private Task Client_Disconnected(Exception arg)
		{
			Console.WriteLine($"Recieved Disconnect request event. Exiting process to handle restart gracefully.");
			// https://github.com/discord-net/Discord.Net/issues/960
			Environment.Exit(-1);
			// make compiler happy
			return Task.CompletedTask; 
		}

		private Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		private ServiceProvider ConfigureServices()
		{
			// Ignore certificate errors
			var handler = new HttpClientHandler()
			{
				ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
			};

			return new ServiceCollection()
				.AddSingleton<DiscordSocketClient>()
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandlingService>()
				.AddSingleton<HttpClient>(_ => new HttpClient(handler))
				.AddSingleton<ReactionRoleService>()
				.AddSingleton<JenkinsService>()
				.BuildServiceProvider();
		}
	}
}