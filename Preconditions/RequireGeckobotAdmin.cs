using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace GeckoBot.Preconditions
{
    // Inherit from PreconditionAttribute
    public class RequireGeckobotAdmin : PreconditionAttribute
    {
        // Override the CheckPermissions method
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // If this command was executed by an admin, return success
            if (Globals.GeckobotAdmins.Contains(context.User.Id))
                // Since no async work is done, the result has to be wrapped with `Task.FromResult` to avoid compiler errors
                return Task.FromResult(PreconditionResult.FromSuccess());
            
            // If it wasn't, fail
            return Task.FromResult(PreconditionResult.FromError("You must be a geckobot admin to run this command."));
        }
    }
}