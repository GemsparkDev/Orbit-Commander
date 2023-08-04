using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Space_Wars.Content.Main.UI_Elements;
using System.Collections;
using System.Linq;

namespace Space_Wars.Content.Main.UI_Elements
{
    internal class TabbedWindow : Container
    {
        private List<KeyValuePair<int, IFunctional>> functionalChildren;
        private List<KeyValuePair<int, Widget>> children;
        private List<Decal> tabList;
        private int currentTab = 0;
        private int prevTab = 0;
        private int totalTabs = 0;
        public TabbedWindow(Vector2 _position, Texture2D _texture, int? tabs)
        {
            size = new Vector2(_texture.Width, _texture.Height);
            texture = _texture;
            position = _position - Size / 2;
            enabled = true;
            functionalChildren = new List<KeyValuePair<int, IFunctional>>();
            children = new List<KeyValuePair<int, Widget>>();
            tabList = new();
            totalTabs = tabs?? 0;
            Vector2 tabOffset = new Vector2(0, -Size.Y / 4 + Assets.Sprites["Tab"].Height);
            for (int i = 0; i < totalTabs; i++)
            {
                if(i == currentTab)
                {
                    tabList.Add(new Decal(tabOffset, Assets.Sprites["Selected Tab"]));
                }
                else
                {
                    tabList.Add(new Decal(tabOffset, Assets.Sprites["Tab"]));
                }
                tabOffset.X += Assets.Sprites["Tab"].Width + 4;
            }
        }
        public override void AddWidget(Widget widget, int tab = 0)
        {
            if(tab >= totalTabs)
            {
                totalTabs = tab + 1;
                RecalculateTabs();
            }
            children.Add(new KeyValuePair<int, Widget>(tab, widget));
        }
        public override void AddWidget(IFunctional widget, int tab = 0)
        {
            if (tab >= totalTabs)
            {
                totalTabs = tab + 1;
                RecalculateTabs();
            }
            functionalChildren.Add(new KeyValuePair<int, IFunctional>(tab, widget));
        }
        public override Widget GetWidget(int index)
        {
            return children[index].Value;
        }
        public override IFunctional GetFuncWidget(int index)
        {
            return functionalChildren[index].Value;
        }
        public override bool GetMouseOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X, Mouse.GetState().Y);
            if (position.X <= mousePosition.X && mousePosition.X <= position.X + Size.X && position.Y - Assets.Sprites["Tab"].Height <= mousePosition.Y && mousePosition.Y <= position.Y + Size.Y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override IFunctional GetWidgetOver()
        {
            Vector2 mousePosition = new(Mouse.GetState().X - position.X, Mouse.GetState().Y - position.Y);
            float bestDistance = float.MaxValue;
            float currentDistance;
            IFunctional bestWidget = new DummyWidget();
            for(int i = 0; i < totalTabs; i++)
            {
                if (tabList[i].offset.X <= mousePosition.X && mousePosition.X <= tabList[i].offset.X + tabList[i].Size.X && tabList[i].offset.Y <= mousePosition.Y && mousePosition.Y <= tabList[i].offset.Y + tabList[i].Size.Y)
                {
                    prevTab = currentTab;
                    currentTab = i;
                    tabList[prevTab].texture = Assets.Sprites["Tab"];
                    tabList[currentTab].texture = Assets.Sprites["Selected Tab"];
                    return new DummyWidget();
                }
            }
            var funcMatches = from val in functionalChildren where val.Key == currentTab select val.Value;
            foreach (var functionalWidget in funcMatches)
            {
                Widget widget = functionalWidget as Widget ?? new DummyWidget();
                if (widget.offset.X <= mousePosition.X && mousePosition.X <= widget.offset.X + widget.Size.X && widget.offset.Y <= mousePosition.Y && mousePosition.Y <= widget.offset.Y + widget.Size.Y)
                {
                    currentDistance = EntityManager.DistanceSqr(widget.Size/2 + widget.offset, mousePosition);
                    if (currentDistance < bestDistance)
                    {
                        bestDistance = currentDistance;
                        bestWidget = functionalWidget;
                    }
                }
            }
            return bestWidget;
        }
        private void RecalculateTabs()
        {
            Vector2 tabOffset = new Vector2(0, -Size.Y / 4 + Assets.Sprites["Tab"].Height);
            for (int i = 0; i < totalTabs; i++)
            {
                if (i >= tabList.Count())
                {
                    tabList.Add(new Decal(tabOffset, Assets.Sprites["Tab"]));
                }
                else
                {
                    tabList[i].offset = tabOffset;
                }
                tabOffset.X += Assets.Sprites["Tab"].Width + 4;
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < totalTabs; i++)
            {
                spriteBatch.Draw(tabList[i].texture, position + tabList[i].offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                tabList[i].Draw(spriteBatch, position);
            }

            if(children.Count() > 0)
            {
                var matches = from val in children where val.Key == currentTab select val.Value;
                foreach (var widget in matches)
                {
                    if (widget.texture != null)
                    {
                        spriteBatch.Draw(widget.texture, position + widget.offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                        widget.Draw(spriteBatch, position);
                    }
                    else
                    {
                        widget.Draw(spriteBatch, position);
                    }
                }
            }

            if(functionalChildren.Count() > 0)
            {
                var funcMatches = from val in functionalChildren where val.Key == currentTab select val.Value;
                foreach (var functionalWidget in funcMatches)
                {
                    Widget widget = functionalWidget as Widget;
                    if (widget != null)
                    {
                        if (widget.texture != null)
                        {
                            spriteBatch.Draw(widget.texture, position + widget.offset, null, Color.White, 0, Vector2.One / 2, Engine.UIScale, SpriteEffects.None, 0.4f);
                            widget.Draw(spriteBatch, position);
                        }
                        else
                        {
                            widget.Draw(spriteBatch, position);
                        }
                    }
                }
            }
        }
    }
}
