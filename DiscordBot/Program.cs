﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace _01_basic_ping_bot
{
	// This is a minimal, bare-bones example of using Discord.Net
	//
	// If writing a bot with commands, we recommend using the Discord.Net.Commands
	// framework, rather than handling commands yourself, like we do in this sample.
	//
	// You can find samples of using the command framework:
	// - Here, under the 02_commands_framework sample
	// - https://github.com/foxbot/DiscordBotBase - a bare-bones bot template
	// - https://github.com/foxbot/patek - a more feature-filled bot, utilizing more aspects of the library
	class Program
	{
		private readonly DiscordSocketClient _client;

		// Discord.Net heavily utilizes TAP for async, so we create
		// an asynchronous context from the beginning.
		static void Main(string[] args)
		{
			new Program().MainAsync().GetAwaiter().GetResult();
		}

		public Program()
		{
			// It is recommended to Dispose of a client when you are finished
			// using it, at the end of your app's lifetime.
			_client = new DiscordSocketClient();

			_client.Log += LogAsync;
			_client.Ready += ReadyAsync;
			_client.MessageReceived += MessageReceivedAsync;
			_client.ReactionAdded += _client_ReactionAdded;
			_client.ReactionRemoved += _client_ReactionRemoved;
		}

		Dictionary<string, ulong> roleMap = new Dictionary<string, ulong>()
		{
			{"🇨",703074654713151568},
			{"🇩",703073545164685393},
			{"🇲",703074554163101788},
			{"🇸",703074653874290730},
		};

		const ulong ROLE_MESSAGE_ID = 703293132468387931;
		const ulong CHANNEL_ID = 702746281981771816;
		const ulong GUILD_ID = 695318762051731559;

		public async Task MainAsync()
		{
			// Tokens should be considered secret data, and never hard-coded.
			var token = Environment.GetEnvironmentVariable("BOT_TOKEN", EnvironmentVariableTarget.Process);
			Console.WriteLine(token);
			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			// Block the program until it is closed.
			await Task.Delay(-1);
		}

		private Task LogAsync(LogMessage log)
		{
			Console.WriteLine(log.ToString());
			return Task.CompletedTask;
		}

		// The Ready event indicates that the client has opened a
		// connection and it is now safe to access the cache.
		private async Task ReadyAsync()
		{
			Console.WriteLine($"{_client.CurrentUser} is connected!");

			var msg = await _client.GetGuild(GUILD_ID).GetTextChannel(CHANNEL_ID).GetMessageAsync(ROLE_MESSAGE_ID);

			
			foreach(var kvp in roleMap)
			{
				Console.WriteLine($"Trying to add {kvp.Key}");
				var emote = new Emoji(kvp.Key);
				if (!msg.Reactions.ContainsKey(emote))
				{
					Console.WriteLine($"Adding {emote.Name}");
					await msg.AddReactionAsync(new Emoji(kvp.Key));
				}
				else
				{
					Console.WriteLine($"Message already has {emote.Name}");
				}
			}			
		}

		// This is not the recommended way to write a bot - consider
		// reading over the Commands Framework sample.
		private async Task MessageReceivedAsync(SocketMessage message)
		{
			// The bot should never respond to itself.
			if (message.Author.Id == _client.CurrentUser.Id)
				return;

			if (message.Content == "!ping")
				await message.Channel.SendMessageAsync("pong!");
		}

		private async Task UpdateRoleAsync(SocketReaction reaction, bool isRemove)
		{
			if (reaction.MessageId != ROLE_MESSAGE_ID) return;

			if (roleMap.ContainsKey(reaction.Emote.Name))
			{
				var guild = _client.GetGuild(GUILD_ID);
				var roleName = roleMap[reaction.Emote.Name];
				var role = guild.GetRole(roleName);
				var user = guild.GetUser(reaction.UserId);

				if (isRemove)
				{
					await user.RemoveRoleAsync(role);
				}
				else
				{
					await user.AddRoleAsync(role);
				}

			}
		}

		private async Task _client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			await UpdateRoleAsync(arg3, false);
		}

		private async Task _client_ReactionRemoved(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
		{
			await UpdateRoleAsync(arg3, true);
		}
	}
}