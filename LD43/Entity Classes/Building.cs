using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using MonoGame.FZT;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Data;
using MonoGame.FZT.Drawing;
using MonoGame.FZT.Input;
using MonoGame.FZT.Physics;
using MonoGame.FZT.Sound;
using MonoGame.FZT.UI;
using MonoGame.FZT.XML;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LD43
{
    class Building : Entity
    {
        public bool isHovered, wasHovered, wasClicked, release;
        public string hoveredText;
        public int level;
        float cd, maxCd;

        public Building(DrawerCollection textures_, PositionManager pos_, List<Property> props_, float cd_, string name_ = null, string text_ = null) : base(textures_, pos_, props_, name_, "building")
        {
            maxCd = cd_;
            isHovered = false;
            wasClicked = false;
            hoveredText = text_;
            release = false;
            level = 1;
        }

        public override void Update(float es_)
        {
            if (wasClicked)
            {
                cd -= es_;
                if (cd <= 0)
                {
                    wasClicked = false;
                    release = true;
                }
            }
            if (!wasClicked)
            {
                if (isHovered)
                {
                    if (!wasHovered) { textures.GetTex("hovered").Reset(); }                 
                }
                else
                    currentTex = textures.GetTex("idle");
            }
            base.Update(es_);

            wasHovered = isHovered;
        }

        public virtual void Click()
        {
            if (!wasClicked)
            {
                wasClicked = true;
                cd = maxCd;
                currentTex = textures.GetTex("clicked");
            }
        }

        public override void Draw(SpriteBatch sb_, bool flipH_ = false, bool flipV_ = false, float angle_ = 0)
        {
            base.Draw(sb_, flipH_, flipV_, angle_);
            if(isHovered && wasHovered)
            textures.GetTex("hovered").Draw(sb_, posman.pos);
        }
    }
}
