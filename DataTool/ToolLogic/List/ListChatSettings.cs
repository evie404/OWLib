using System;
using System.Linq;
using DataTool.Flag;
using TankLib.STU.Types;
using static DataTool.Program;
using static DataTool.Helper.STUHelper;
using static DataTool.Helper.IO;

namespace DataTool.ToolLogic.List {
    [Tool("list-chat-settings", Description = "List chat settings", CustomFlags = typeof(ListFlags))]
    public class ListChatSettings : ITool {
        public void Parse(ICLIFlags toolFlags) {
            foreach (var key in TrackedFiles[0x54]) {
                var chat = GetInstance<STUGenericSettings_Chat>(key);
                if (chat == null) continue;

                Console.Out.WriteLine("Chat Channels:");
                foreach (var chatChannel in chat.m_chatChannels) {
                    Console.Out.WriteLine($"    {GetString(chatChannel.m_chatChannelName)}:");
                    Console.Out.WriteLine($"        Type: {chatChannel.m_chatChannelType}");

                    //Error = 1,
                    //System = 2,
                    //Whisper = 3,
                    //Group = 4,
                    //Team = 5,
                    //Match = 6,
                    //General = 7
                }

                Console.Out.WriteLine("\r\nChat Commands:");
                foreach (var chatCommand in chat.m_chatCommands) {
                    Console.Out.WriteLine($"    {GetString(chatCommand.m_4CED72F5)}:");
                    Console.Out.WriteLine($"        Description: {GetString(chatCommand.m_commandDescription)}");
                    Console.Out.WriteLine($"        Aliases: {string.Join(", ", chatCommand.m_chatCommandAliases.Select(x => GetString(x)))}");
                }
            }
        }
    }
}
