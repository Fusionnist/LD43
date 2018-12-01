using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Drawing;
using MonoGame.FZT.Physics;


namespace LD43
{
    enum VillagerState { walkingLeft, walkingBack, dying, returning }
    public class Villager : Entity
    {
        VillagerState state;
        public Villager(DrawerCollection textures_, PositionManager pos_, List<Property> properties_, string name_, string type_) : base(textures_, pos_, properties_, name_, type_)
        {
            state = VillagerState.walkingLeft;
        }

        public override void Update(float elapsedTime_)
        {
            CheckDeath();
            if(state == VillagerState.dying)
            {

            }
            if(state == VillagerState.walkingLeft)
            {
                posman.mov.X = -1;
                if(posman.pos.X < 32)
                    state = VillagerState.walkingBack;
            }
            if (state == VillagerState.walkingBack)
            {
                posman.mov.X = 1;

                if(posman.pos.X > 360)
                {
                    AddResource();
                    state = VillagerState.returning;
                    Return();
                }
            }
            base.Update(elapsedTime_);
        }

        void AddResource()
        {
            GameData.food++;
        }

        void CheckDeath()
        {
            if(GameData.pitOpen && posman.pos.X > 80 && posman.pos.X < 160)
            {
                state = VillagerState.dying;
            }
        }

        void Die()
        {
            exists = false;
        }

        void Return()
        {
            GameData.citizens++;
            exists = false;
        }

        public override void Draw(SpriteBatch sb_, bool flipH_ = false, bool flipV_ = false, float angle_ = 0)
        {
            base.Draw(sb_, flipH_, flipV_, angle_);
        }
    }
}
