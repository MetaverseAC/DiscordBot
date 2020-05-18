using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
	public class ReactionRoleService
	{
		private DiscordSocketClient _client;
		private IServiceProvider _services;

		Dictionary<string, ulong> roleMap = new Dictionary<string, ulong>()
		{
			{"🇨",703074654713151568},
			{"🇩",703073545164685393},
			{"🇲",703074554163101788},
			{"🇸",703074653874290730},
		};

		const ulong ROLE_MESSAGE_ID = 711752387995500606;
		const ulong CHANNEL_ID = 707632533734686762;
		const ulong GUILD_ID = 695318762051731559;
		const ulong OPERATOR_ROLE_ID = 702744926466736179;

		public ReactionRoleService(IServiceProvider services)
		{
			_client = services.GetRequiredService<DiscordSocketClient>();
			_services = services;

			_client.Ready += ReadyAsync;
			_client.ReactionAdded += ReactionAddedAsync;
			_client.ReactionRemoved += ReactionRemovedAsync;
		}

		private async Task ReactionRemovedAsync(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction reaction)
		{
			await UpdateRoleAsync(reaction, isRemove: true);
		}

		private async Task ReactionAddedAsync(Discord.Cacheable<Discord.IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction reaction)
		{
			await UpdateRoleAsync(reaction, isRemove: false);
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

				var removeString = isRemove ? "Removing" : "Adding";
				Console.WriteLine($"{removeString} role {reaction.Emote.Name} for {user.Username}");
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

		// The Ready event indicates that the client has opened a
		// connection and it is now safe to access the cache.
		private async Task ReadyAsync()
		{
			Console.WriteLine($"{_client.CurrentUser} is connected!");

			var msg = await _client.GetGuild(GUILD_ID).GetTextChannel(CHANNEL_ID).GetMessageAsync(ROLE_MESSAGE_ID);

			foreach (var kvp in roleMap)
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
	}
}
