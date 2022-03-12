using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.IO;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using XivCommon;

namespace FFXIVGambler
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "FFXIV Gambler";

        private const string commandName = "/deck";
        private const string commandDraw = "/draw";
        private const string commandShuffle = "/shuffle";

        private PartyList partyMembers;

        private TargetManager targetManager;
        private ChatGui chat;
        private CardDeck deck;
        
        
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        private Configuration Configuration { get; init; }
        private PluginUI PluginUi { get; init; }


        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager,
            [RequiredVersion("1.0")] PartyList partyMembers,
            [RequiredVersion("1.0")] ChatGui chat,
            [RequiredVersion("1.0")] TargetManager targetManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            this.partyMembers = partyMembers;
            this.targetManager = targetManager;
            this.chat = chat;
            this.deck = new CardDeck();
            
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards.jpg");
            var cardImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration, cardImage, this.partyMembers, chat, targetManager, this.deck);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open a UI window to manage deck."
            });
            this.CommandManager.AddHandler(commandDraw, new CommandInfo(OnCommandDraw)
            {
                HelpMessage = "Draw a new card for currently targeted player."
            });
            this.CommandManager.AddHandler(commandShuffle, new CommandInfo(OnCommandShuffle)
            {
                HelpMessage = "Reset and Shuffle the deck."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.CommandManager.RemoveHandler(commandDraw);
            this.CommandManager.RemoveHandler(commandShuffle);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            this.PluginUi.Visible = true;
        }

        private void OnCommandDraw(string command, string args)
        {
            if (this.targetManager.Target != null)
            {
                GameObject target = this.targetManager.Target;
                string card = this.deck.drawCard();
                string message = "Draw Card for " + target.Name + ": " + card;
                var Common = new XivCommonBase(Hooks.Talk);
                Common.Functions.Chat.SendMessage(message);

            }
            else
            {
                this.chat.Print("No Target for card!");
                this.chat.UpdateQueue();
            }
        }

        private void OnCommandShuffle(string command, string args)
        {
            this.deck.reshuffleDeck();
            this.chat.Print("Deck Shuffled!  Fresh Deck ready!");
            this.chat.UpdateQueue();
        }

        private void DrawUI()
        {
            this.PluginUi.Draw();
        }

        private void DrawConfigUI()
        {
            this.PluginUi.SettingsVisible = true;
        }
    }
}
