using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Gui;
using System.Collections.Generic;

namespace FFXIVGambler
{
    // It is good to have this be disposable in general, in case you ever need it
    // to do any cleanup
    class PluginUI : IDisposable
    {
        private Configuration configuration;
        private ImGuiScene.TextureWrap cardImage;
        private CardDeck deck;
        private ChatGui chat;
        private PartyList partyMembers;
        private TargetManager targetManager;
        private SortedDictionary<String, Hand> playerHands;
        private String dealerName;
        private Hand dealerHand;

        // this extra bool exists for ImGui, since you can't ref a property
        private bool visible = false;
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }

        private bool settingsVisible = false;
        public bool SettingsVisible
        {
            get { return this.settingsVisible; }
            set { this.settingsVisible = value; }
        }

        // passing in the image here just for simplicity
        public PluginUI(Configuration configuration, ImGuiScene.TextureWrap cardImage, PartyList partyMembers, ChatGui chat, TargetManager targetManager, CardDeck deck, SortedDictionary<String,Hand> playerHands, Hand dealerHand, string dealerName)
        {
            this.cardImage = cardImage;
            this.partyMembers = partyMembers;
            this.deck = deck;
            this.chat = chat;
            this.chat.Enable();
            this.targetManager = targetManager;
            this.playerHands = playerHands;
            this.dealerHand = dealerHand;
            this.dealerName = dealerName;
        }

        public void Dispose()
        {
            this.cardImage.Dispose();
        }

        public void Draw()
        {
            // This is our only draw handler attached to UIBuilder, so it needs to be
            // able to draw any windows we might have open.
            // Each method checks its own visibility/state to ensure it only draws when
            // it actually makes sense.
            // There are other ways to do this, but it is generally best to keep the number of
            // draw delegates as low as possible.

            DrawMainWindow();
        }

        public void DrawMainWindow()
        {
            if (!Visible)
            {
                return;
            }
            
            ImGui.SetNextWindowSize(new Vector2(400, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(400, 200), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Blackjack", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {

                ImGui.SetWindowFontScale((float)1.3);
                ImGui.Spacing();
                string dealerHand = "Dealer: " + this.dealerName + " : " + this.dealerHand.getTotal() + " : " + this.dealerHand.showHand();
                if (this.dealerHand.checkBlackjack())
                {
                    dealerHand = dealerHand + " BLACKJACK!";
                }
                else if (this.dealerHand.getTotal() > 21)
                {
                    dealerHand = dealerHand + " BUST";
                }
                ImGui.Text(dealerHand);
                if (this.dealerHand.getTotal() > 16)
                {
                    //finish the hand!
                    if (this.playerHands != null)
                    {
                        List<string> playerList = new List<String>(this.playerHands.Keys);
                        foreach (string player in playerList)
                        {
                            if (player != null)
                            {
                                Hand playerHand = new Hand();
                                _ = this.playerHands.TryGetValue(player, out playerHand);
                                string messageHand = player + " : " + playerHand.getTotal() + " : " + playerHand.showHand();
                                messageHand = AddResult(playerHand, messageHand);
                                ImGui.Text(messageHand);
                            }
                        }
                    }
                }
                else
                {
                    if (this.playerHands != null)
                    {
                        List<string> playerList = new List<String>(this.playerHands.Keys);
                        foreach (string player in playerList)
                        {
                            if (player != null)
                            {
                                Hand playerHand = new Hand();
                                _ = this.playerHands.TryGetValue(player, out playerHand);
                                string messageHand = player + " : " + playerHand.getTotal() + " : " + playerHand.showHand();
                                if (playerHand.checkBlackjack())
                                {
                                    messageHand = messageHand + " BLACKJACK!";
                                }
                                else if(playerHand.getTotal() > 21)
                                {
                                    messageHand = messageHand + " BUST";
                                }
                                ImGui.Text(messageHand);
                            }
                        }
                    }
                }

            }
            ImGui.End();
        }

        private string AddResult(Hand playerHand, string messageHand)
        {
            if (playerHand.checkBlackjack())
            {
                messageHand = messageHand + " BLACKJACK!";
            }
            else if (playerHand.getTotal() > 21)
            {
                messageHand = messageHand + " BUST";
            }
            else
            {
                if (this.dealerHand.checkBlackjack())
                {
                    messageHand = messageHand + " LOSS";
                }
                else if(this.dealerHand.getTotal() > 21)
                {
                    messageHand = messageHand + " WIN";
                }
                else if (this.dealerHand.getTotal() == playerHand.getTotal())
                {
                    messageHand = messageHand + " PUSH";
                }
                else if (this.dealerHand.getTotal() > playerHand.getTotal())
                {
                    messageHand = messageHand + " LOSS";
                }
                else
                {
                    messageHand = messageHand + " WIN";
                }
            }
            return messageHand;
        }
    }
}
