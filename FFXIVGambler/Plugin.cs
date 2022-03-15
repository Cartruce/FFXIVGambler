using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.IO;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Gui;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace FFXIVGambler
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "FFXIV Gambler";

        private const string commandName = "/blackjack";
        private const string commandDraw = "/draw";
        private const string commandShuffle = "/shuffle";
        private const string commandDealer = "/dealer";
        private const string commandSplit = "/split";

        private PartyList partyMembers;

        private TargetManager targetManager;
        private ChatGui chat;
        private CardDeck deck;
        private Hand dealerHand;
        private string dealerName;
        private SortedDictionary<String, Hand> playerHands;


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
            this.dealerHand = new Hand();
            if(partyMembers != null)
            this.dealerName = partyMembers[(int)partyMembers.PartyLeaderIndex].Name.TextValue;
            this.playerHands = new SortedDictionary<String, Hand>();
            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "Cards.jpg");
            var cardImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            this.PluginUi = new PluginUI(this.Configuration, cardImage, this.partyMembers, chat, targetManager, this.deck, this.playerHands, this.dealerHand, this.dealerName);

            this.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open a UI window to manage blackjack game."
            });
            this.CommandManager.AddHandler(commandDraw, new CommandInfo(OnCommandDraw)
            {
                HelpMessage = "Draw a new card for currently targeted player."
            });
            this.CommandManager.AddHandler(commandDealer, new CommandInfo(OnCommandDealer)
            {
                HelpMessage = "Draw a card for the dealer (party leader)."
            });
            this.CommandManager.AddHandler(commandShuffle, new CommandInfo(OnCommandShuffle)
            {
                HelpMessage = "Reset and Shuffle the deck."
            });
            this.CommandManager.AddHandler(commandSplit, new CommandInfo(OnCommandSplit)
            {
                HelpMessage = "split the selected players hand."
            });


            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        private void OnCommandSplit(string command, string arguments)
        {
            //check if the current target has a 'split' hand:
            if (this.targetManager.Target != null)
            {
                GameObject target = this.targetManager.Target;
                string name = "" + target.Name;
                string splitName = "split:" + name;
                if (this.playerHands.ContainsKey(splitName))
                {
                    string card = this.deck.drawCard();
                    string message = "Draw Card for " + name + ": " + card;
                    //this.chat.Print(message);
                    ImGui.SetClipboardText(message);
                    //var Common = new XivCommonBase(Hooks.Talk);
                    //Common.Functions.Chat.SendMessage(message);
                    if (!card.Contains("Empty"))
                        AddDrawToPlayer(splitName, card);
                }
                else
                {
                    //try to perform a split operation.
                    Hand currentHand;
                    Boolean hasHand = this.playerHands.TryGetValue(name, out currentHand);
                    if (hasHand)
                    {
                        Hand newHand = new Hand();
                        newHand = SplitHand(currentHand);
                        this.playerHands.Add(splitName, newHand);
                    }
                    else
                    {
                        this.chat.Print("target doesn't have a registered hand.  Deal cards to them first!");
                        this.chat.UpdateQueue();
                    }
                }
            }
            else
            {
                this.chat.Print("No Target for card!");
                ImGui.SetClipboardText("No Target for card!");
                this.chat.UpdateQueue();
            }

        }

        private Hand SplitHand(Hand currentHand)
        {
            if (currentHand.getNumCards() == 2)
            {
                string card1 = currentHand.getCards()[0];
                string card2 = currentHand.getCards()[1];
                currentHand.resetHand();
                currentHand.addCard(card1);

                Hand splithand = new Hand();
                splithand.addCard(card2);
                return splithand;
            }
            else
            {
                return new Hand();
            }
        }

        private void OnCommandDealer(string command, string args)
        {

            string card = this.deck.drawCard();
            string message = "Draw Card for " + this.dealerName + ": " + card;
            //this.chat.Print(message);
            ImGui.SetClipboardText(message);
            //var Common = new XivCommonBase(Hooks.Talk);
            //Common.Functions.Chat.SendMessage(message);
            if (!card.Contains("Empty"))
                this.dealerHand.addCard(card);
        }

        public void Dispose()
        {
            this.PluginUi.Dispose();
            this.CommandManager.RemoveHandler(commandName);
            this.CommandManager.RemoveHandler(commandDraw);
            this.CommandManager.RemoveHandler(commandShuffle);
            this.CommandManager.RemoveHandler(commandDealer);
            this.CommandManager.RemoveHandler(commandSplit);
        }

        private void OnCommand(string command, string args)
        {
            OnCommandShuffle(command, args);
            this.PluginUi.Visible = true;
        }

        private void OnCommandDraw(string command, string args)
        {
            if (args == null || args.Length == 0 || args == "1")
            {
                if (this.targetManager.Target != null)
                {
                    GameObject target = this.targetManager.Target;
                    string card = this.deck.drawCard();
                    string message = "Draw Card for " + target.Name + ": " + card;
                    //this.chat.Print(message);
                    ImGui.SetClipboardText(message);
                    //var Common = new XivCommonBase(Hooks.Talk);
                    //Common.Functions.Chat.SendMessage(message);
                    if(!card.Contains("Empty"))
                    AddDrawToPlayer(target.Name, card);
                }
                else
                {
                    this.chat.Print("No Target for card!");
                    ImGui.SetClipboardText("No Target for card!");
                    this.chat.UpdateQueue();
                }
            }
            else
            {
                //args will be number of draws to make.
                string card = "";
                string cards = "";
                int numcards = 0;
                if (args != null)
                {
                    string numCards = args;
                    numcards = int.Parse(numCards);
                }
                if (this.targetManager.Target != null)
                {
                    GameObject target = this.targetManager.Target;
                    for (int i = 0; i < numcards; i++)
                    {
                        card = this.deck.drawCard();
                        cards = cards + card + "  ";
                        if(!card.Contains("Empty"))
                        AddDrawToPlayer(target.Name, card);
                    }
                    string message = "Draw " + args + " cards for " + target.Name + ": " + cards;
                    ImGui.SetClipboardText(message);
                    //var Common = new XivCommonBase(Hooks.Talk);
                    //Common.Functions.Chat.SendMessage(message);
                }
                else
                {
                    this.chat.Print("No Target for card!");
                    ImGui.SetClipboardText("No Target for card!");
                    this.chat.UpdateQueue();
                }
            }
        }

        private void AddDrawToPlayer(SeString name, string card)
        {
            //check if player exists in playerlist:
            if(this.playerHands.ContainsKey(name.TextValue))
            {
                Hand currentHand = this.playerHands[name.TextValue];
                currentHand.addCard(card);
            }
            else
            {
                Hand newHand = new Hand(card);
                this.playerHands.Add(name.TextValue, newHand);
            }
        }

        private void OnCommandShuffle(string command, string args)
        {
            this.deck.reshuffleDeck();
            this.playerHands.Clear();
            this.dealerHand.resetHand();
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
