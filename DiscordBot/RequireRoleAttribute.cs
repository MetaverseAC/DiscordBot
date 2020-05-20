using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    // Inherit from PreconditionAttribute
    public class RequireRoleAttribute : PreconditionAttribute
    {
        // Create a field to store the specified roleId
        private readonly ulong _roleId;

        // Create a constructor so the roleId can be specified
        public RequireRoleAttribute(ulong roleId) => _roleId = roleId;

        // Hardcode this to Metaverse Discord
        const ulong GUILD_ID = 695318762051731559;

        // Override the CheckPermissions method
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            var user = context.User as SocketGuildUser;
            
            // HARDCODE to Metaverse, probably bad
            if (user == null)
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                var guild = client.GetGuild(GUILD_ID);
                user = guild.GetUser(context.User.Id);
            }
            if (user != null)
            {
                // If this command was executed by a user with the appropriate role, return a success
                if (user.Roles.Any(r => r.Id == _roleId))
                    // Since no async work is done, the result has to be wrapped with `Task.FromResult` to avoid compiler errors
                    return Task.FromResult(PreconditionResult.FromSuccess());
                // Since it wasn't, fail
                else
                    return Task.FromResult(PreconditionResult.FromError($"You must have a role <@{_roleId}> to run this command."));
            }
            else
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
