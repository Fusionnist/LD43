﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Drawing;
using MonoGame.FZT.Physics;
using Microsoft.Xna.Framework;
using MonoGame.FZT.Sound;

namespace LD43
{
    enum GodState { Idle, Attacking }
    enum GodMood { Ok, Ungry, Pissed, VladimirPutin }
    public class God : Entity
    {
        GodState state;
        GodMood mood;

        Villager target;

        public God(DrawerCollection textures_, PositionManager pos_, List<Property> properties_, string name_, string type_) : base(textures_, pos_, properties_, name_, type_)
        {
            state = GodState.Idle;
            mood = GodMood.Ok;
            currentTex = textures.GetTex("idle");
        }

        public void Attack()
        {
            if(state != GodState.Attacking)
                if (FindVillager())
                {
                    
                    state = GodState.Attacking;
                    textures.GetTex("swipe").Reset();
                    GameData.madness += GameData.MadnessGain;
                    if(GameData.madness > 100) { GameData.madness = 100; }
                    SoundManager.PlayEffect("sacrifice");
                }
        }

        bool FindVillager()
        {
            foreach(Villager v in EntityCollection.GetGroup("villagers"))
            {
                if(v.posman.pos.X > 125 && v.posman.pos.X < 210)
                {
                    target = v;
                    return true;
                }
            }
            return false;
        }

        public override void Update(float elapsedTime_)
        {
            textures.GetTex("swipe").Update(elapsedTime_);
            if (state == GodState.Attacking)
                if (textures.GetTex("swipe").Ended())
                {
                    GameData.godHunger -= GameData.GodFeed;
                    if(GameData.godHunger < 0) { GameData.godHunger = 0; }
                    target.exists = false;
                    GameData.citizensOutside--;
                    state = GodState.Idle;
                }
            if(GameData.godAnger == 100)
            {
                GameData.attacks++;
                GameData.DestroyBuilding();
                GameData.godAnger = 50;
            }
            base.Update(elapsedTime_);
        }

        public override void Draw(SpriteBatch sb_, bool flipH_ = false, bool flipV_ = false, float angle_ = 0)
        {
            if(state == GodState.Attacking)
            {
                textures.GetTex("swipe").Draw(sb_, target.posman.pos + new Vector2(-24,-11));
            }
            //base.Draw(sb_, flipH_, flipV_, angle_);
        }
    }
}
