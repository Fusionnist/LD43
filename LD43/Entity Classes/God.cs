using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Drawing;
using MonoGame.FZT.Physics;
using Microsoft.Xna.Framework;

namespace LD43.Entity_Classes
{
    enum GodState { Idle, Attacking }
    enum GodMood { Ok, Ungry, Pissed, VladimirPutin }
    public class God : Entity
    {
        GodState state;
        GodMood mod;

        public God(DrawerCollection textures_, PositionManager pos_, List<Property> properties_, string name_, string type_) : base(textures_, pos_, properties_, name_, type_)
        {

        }

        public void Attack()
        {
            textures.GetTex("attack").Reset();
        }

        public override void Update(float elapsedTime_)
        {
            base.Update(elapsedTime_);
        }

        public override void Draw(SpriteBatch sb_, bool flipH_ = false, bool flipV_ = false, float angle_ = 0)
        {
            if(state == GodState.Attacking)
            {
                textures.GetTex("attack").Draw(sb_, Vector2.Zero);
            }
            base.Draw(sb_, flipH_, flipV_, angle_);
        }
    }
}
