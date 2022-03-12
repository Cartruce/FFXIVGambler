using ImGuiNET;
using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.Game.ClientState.Objects.Types;

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
        public PluginUI(Configuration configuration, ImGuiScene.TextureWrap cardImage, PartyList partyMembers, ChatGui chat, TargetManager targetManager, CardDeck deck)
        {
            this.configuration = configuration;
            this.cardImage = cardImage;
            this.partyMembers = partyMembers;
            this.deck = deck;
            this.chat = chat;
            this.chat.Enable();
            this.targetManager = targetManager;
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
            
            ImGui.SetNextWindowSize(new Vector2(200, 200), ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowSizeConstraints(new Vector2(200, 200), new Vector2(float.MaxValue, float.MaxValue));
            if (ImGui.Begin("Deck of Cards", ref this.visible, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse))
            {
                if (ImGui.Button("Draw Card"))
                {
                    DrawCard();
                }
                if(ImGui.Button("Shuffle Deck"))
                {
                    ShuffleDeck();
                }
                ImGui.Spacing();
                

                ImGui.Image(this.cardImage.ImGuiHandle, new Vector2(120,120));
            }
            ImGui.End();
        }

        public void DrawCard()
        {
            
            if (this.targetManager.Target != null)
            {
                GameObject target = this.targetManager.Target;
                string card = this.deck.drawCard();
                XivChatEntry message = new XivChatEntry();
                message.Type = XivChatType.Party;
                message.Message = "Draw Card for " + target.Name + ": " + card;
                message.SenderId = partyMembers[0].ObjectId;
                message.Name = partyMembers[0].Name;
                this.chat.PrintChat(message);

                this.chat.UpdateQueue();
            }
            else
            {
                this.chat.Print("No Target for card!");
                this.chat.UpdateQueue();
            }
        }

        public void ShuffleDeck()
        {
            this.deck.reshuffleDeck();
            this.chat.Print("Deck Shuffled!  Fresh Deck ready!");
            this.chat.UpdateQueue();
        }


    }
}
