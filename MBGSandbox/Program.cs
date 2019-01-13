using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MBGSandbox
{
    class Program
    {
        /// <summary>
        ///     <para>
        ///         Reference list of role definitons
        ///     </para>
        /// </summary>
        private List<RoleDefinition> _roleEmoji;

        /// <summary>
        ///     <para>
        ///         The discord client used to connect
        ///     </para>
        /// </summary>
        private DiscordSocketClient _client;


        /// <summary>
        ///     <para>
        ///         Discord.NET uses Task-based Asyncyhronous Patter (TAP). 
        ///         So we need our Main entry method to be async.  This is 
        ///         accomplished below by having Main create a new Program object
        ///         that calls the MainAsync Method and awaiting 
        ///     </para>
        /// </summary>
        /// <param name="args">
        ///     <para>
        ///         Arguments that are passed to the application from the
        ///         command line when launched.
        ///     </para>
        /// </param>
        /// <remarks>
        ///     Exceptions that are thrown within the aync context that is not handled
        ///     will be thrown all the way back up to here, our first non-async method.
        /// </remarks>
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        /// <summary>
        ///     <para>
        ///         This is our main entry point that is asynchronous
        ///     </para>
        /// </summary>
        /// <returns></returns>
        public async Task MainAsync()
        {
            //  Initilize the role list
            _roleEmoji = new List<RoleDefinition>();
            _roleEmoji.Add(new RoleDefinition("Unity", "\uD83D\uDE00"));
            _roleEmoji.Add(new RoleDefinition("MonoGame", "\uD83C\uDF6A"));
            _roleEmoji.Add(new RoleDefinition("GameMaker", "\uD83C\uDF73"));
            _roleEmoji.Add(new RoleDefinition("Unreal", "\uD83D\uDC09"));
            _roleEmoji.Add(new RoleDefinition("C#", "\uD83D\uDC4C"));
            _roleEmoji.Add(new RoleDefinition("JavaScript", "\uD83D\uDC4E"));

            //  Since this is a bot, we will be using a DiscordSocketClient
            _client = new DiscordSocketClient();

            //  Before connecting the client, we need to subscribe to the log event 
            _client.Log += Log;

            //  This event is raised when a message is received in chat. This
            //  is any message in chat that the bot can see the channel, not 
            //  a private message specifically
            _client.MessageReceived += MessageReceived;

            //  This event is raised when a reaction is added to a message in chat
            _client.ReactionAdded += ReactionAdded;

            _client.ReactionRemoved += ReactionRemoved;

            //  This is the private token used for the bot.  This is a 
            //  PRIVATE token, shoudl not be shared publically. 
            string token = "";

            //  Perform the login
            await _client.LoginAsync(TokenType.Bot, token);

            //  Start the bot
            await _client.StartAsync();

            //  Block this task until the program is closed
            await Task.Delay(-1);
        }


        /// <summary>
        ///     <para>
        ///         Called by the _client when the ReactionRemoved event is raised
        ///     </para>
        ///     <para>
        ///         Parses the reaction removed and uses it to remove the assignment of a role
        ///         for the user ont he server
        ///     </para>
        /// </summary>
        /// <param name="messageReactedTo">
        ///     <para>
        ///         This is the message that was reacted to
        ///     </para>
        /// </param>
        /// <param name="messageChannel">
        ///     <para>
        ///         This is the channel that the message is in
        ///     </para>
        /// </param>
        /// <param name="reaction">
        ///     <para>
        ///         This is the reaction that was removed
        ///     </para>
        /// </param>
        /// <returns></returns>
        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> messageReactedTo, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            //  We only do this for one specific message
            //  (to get the id, click the menu for the specific message like you would to go
            //  and delete it, then click Copy ID)
            if (messageReactedTo.Id != 533410190641463307) { return; }

            //  Ensure we have a user specified.  If not just return and dont' continue
            if (!reaction.User.IsSpecified)
            {
                await Log(new LogMessage(LogSeverity.Critical, "ReactionRemoved", "User is not specified in reaction"));
                return;
            }

            //  Get the user
            var user = reaction.User.Value;

            //  Check to see if the emoji used in the reaction is one of the ones in our
            //  role emoji list
            var roleEmoji = _roleEmoji.FirstOrDefault(r => r.Emoji.Name == reaction.Emote.Name);

            //  Ensure that we matched and emoji.  If not, then the user removed one that's not
            //  valid, so log the message and return out.
            if (roleEmoji == null)
            {
                await Log(new LogMessage(LogSeverity.Warning, "ReactionRemoved", "Role name could not be identified by the reaction given."));
                return;
            }

            //  Get the guild
            var guild = (messageChannel as IGuildChannel)?.Guild;

            //  Ensure the guild isn't null, if it is, just return since we need it
            if (guild == null)
            {
                await Log(new LogMessage(LogSeverity.Critical, "ReactionRemoved", "Unable to get Guild"));
                return;
            }

            //  Get the role from the guilds roles
            var role = guild.Roles.FirstOrDefault(r => r.Name == roleEmoji.RoleName);

            //  Ensure we retrieved it
            if (role != null)
            {
                await (user as IGuildUser).RemoveRoleAsync(role, null);
                await Log(new LogMessage(LogSeverity.Debug, "ReactionRemoved", $"Removed role {roleEmoji.RoleName} from {user.Id}"));
            }
            else
            {
                await Log(new LogMessage(LogSeverity.Debug, "ReactionRemoved", $"Unable to find a role named {roleEmoji.RoleName} on the server"));
            }
        }


        /// <summary>
        ///     <para>
        ///         Called by the _client when the ReactionAdded event is raised.
        ///     </para>
        ///     <para>
        ///         Parses the reaction given and uses it to auto assign a role to the
        ///         user on the server
        ///     </para>
        /// </summary>
        /// <param name="messageReactedTo">
        ///     <para>
        ///         This is the message that was reacted to
        ///     </para>
        /// </param>
        /// <param name="messageChannel">
        ///     <para>
        ///         This is the channel that the message is in
        ///     </para>
        /// </param>
        /// <param name="reaction">
        ///     <para>
        ///         This is the reaction given
        ///     </para>
        /// </param>
        /// <returns></returns>
        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> messageReactedTo, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {

            //  We only do this for one specific message
            //  (to get the id, click the menu for the specific message like you would to go
            //  and delete it, then click Copy ID)
            if (messageReactedTo.Id != 533410190641463307) { return; }

            //  Ensure we have a user specified. If not just return and don't continue
            if (!reaction.User.IsSpecified)
            {
                await Log(new LogMessage(LogSeverity.Critical, "ReactionAdded", "User is not specified in reaction"));
                return;
            }

            //  Get the user
            var user = reaction.User.Value;

            //  Check to see if the emoji used in the reaction is one of the ones in our
            //  role emoji list
            var roleEmoji = _roleEmoji.FirstOrDefault(r => r.Emoji.Name == reaction.Emote.Name);

            //  Ensure that we matched an emoji. If not, then the user used an invalid one,
            //  so log the message and return out.
            if (roleEmoji == null)
            {
                await Log(new LogMessage(LogSeverity.Warning, "ReactionAdded", "Role name could not be identified by the reaction given."));
                return;
            }

            //  Get the guild
            var guild = (messageChannel as IGuildChannel)?.Guild;

            //  Ensure the guild isn't null, if it is, jsut return since we need it
            if (guild == null)
            {
                await Log(new LogMessage(LogSeverity.Critical, "ReactionAdded", "Unable to get Guild"));
                return;
            }

            //  Get the role from the guilds roles
            var role = guild.Roles.FirstOrDefault(r => r.Name == roleEmoji.RoleName);

            //  Ensure we retrived it
            if (role != null)
            {
                await (user as IGuildUser).AddRoleAsync(role, null);
                await Log(new LogMessage(LogSeverity.Debug, "ReactionAdded", $"Assigned role {roleEmoji.RoleName} to {user.Id}"));
            }
            else
            {
                await Log(new LogMessage(LogSeverity.Debug, "ReactionRemoved", $"Unable to find a role named {roleEmoji.RoleName} on the server"));
            }
        }

        /// <summary>
        ///     <para>
        ///         Called by the discord client when the MessageReceived Event is raised
        ///     </para>
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task MessageReceived(SocketMessage message)
        {
            //  These commands can only be done by me.  Set your id here for you
            if (message.Author.Id != 57912695378149376) { return; }


            //  We can issue the !ping command to ensure the bot has connected and 
            //  is reading messages.
            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }

            //  Using the !reaction-message, this will have the bot post the template message in the channel
            //  that users should react to.
            if (message.Content == "!reaction-message")
            {
                string reactionMessage = $"React to this message to auto-assign a role to yourself {Environment.NewLine}";
                reactionMessage += $"See the list below for which reactions assign which roles. {Environment.NewLine}";
                reactionMessage += $"{_roleEmoji.FirstOrDefault(r => r.RoleName == "Unity")?.Emoji.Name} => Unity {Environment.NewLine}{Environment.NewLine}";
                reactionMessage += $"{_roleEmoji.FirstOrDefault(r => r.RoleName == "MonoGame")?.Emoji.Name} => MonoGame {Environment.NewLine}{Environment.NewLine}";
                reactionMessage += $"{_roleEmoji.FirstOrDefault(r => r.RoleName == "GameMaker")?.Emoji.Name}: => GameMaker {Environment.NewLine}{Environment.NewLine}";
                reactionMessage += $"{_roleEmoji.FirstOrDefault(r => r.RoleName == "Unreal")?.Emoji.Name} => Unreal {Environment.NewLine}{Environment.NewLine}";
                reactionMessage += $"{_roleEmoji.FirstOrDefault(r => r.RoleName == "C#")?.Emoji.Name} => C# {Environment.NewLine}{Environment.NewLine}";
                reactionMessage += $"{_roleEmoji.FirstOrDefault(r => r.RoleName == "JavaScript")?.Emoji.Name} => JavaScript";
                var sentMessage = await message.Channel.SendMessageAsync(reactionMessage);

            }
        }

        /// <summary>
        ///     <para>
        ///         Logs message to console
        ///     </para>
        /// </summary>
        /// <param name="message">
        ///     <para>
        ///         The message to log
        ///     </para>
        /// </param>
        /// <returns></returns>
        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }


    }
}
