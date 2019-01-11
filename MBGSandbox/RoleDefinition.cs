using Discord;

namespace MBGSandbox
{
    public class RoleDefinition
    {
        /// <summary>
        ///     <para>
        ///         The name of the role as it appears on the server
        ///     </para>
        /// </summary>
        public string RoleName { get; private set; }

        /// <summary>
        ///     <para>
        ///         The unicode codepoint of the emoji
        ///     </para>
        /// </summary>
        public string EmojiUnicode { get; private set; }

        /// <summary>
        ///     <para>
        ///         The emoji
        ///     </para>
        /// </summary>
        public Emoji Emoji { get; private set; }

        /// <summary>
        ///     <para>
        ///         Creates a new instance
        ///     </para>
        /// </summary>
        /// <param name="roleName">
        ///     <para>
        ///         The name of the role as it appears on the server
        ///     </para>
        /// </param>
        /// <param name="unicode">
        ///     <para>
        ///         The unicode code point of the emoji
        ///     </para>
        /// </param>
        public RoleDefinition(string roleName, string unicode)
        {
            RoleName = roleName;
            EmojiUnicode = unicode;
            Emoji = new Emoji(unicode);
        }
    }
}
