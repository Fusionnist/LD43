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


namespace LD43
{
    enum VillagerState { walkingLeft, walkingBack, dying, returning }
    public class Villager : Entity
    {
        VillagerState state;
        Timer walkOutTimer;
        List<Vector2> path;
        int objectivevec;
        float walkSpeed = 0.1f;
        public Villager(DrawerCollection textures_, PositionManager pos_, List<Property> properties_, string name_, string type_) : base(textures_, pos_, properties_, name_, type_)
        {
            state = VillagerState.walkingLeft;
            pos_.pos = GameData.townMiddle.vec;
            path = GameData.GetRandomPath();
            GameData.citizensOutside--;
        }

        public override void Update(float elapsedTime_)
        {
            CheckDeath();
            if(state == VillagerState.dying)
            {

            }
            Vector2 mov = (posman.pos - path[objectivevec]);
            if(mov != Vector2.Zero)
                mov.Normalize();
            posman.mov += mov * walkSpeed * -1;
            if (state == VillagerState.walkingLeft)
            {
               if((posman.pos-path[objectivevec]).Length() < 1)
                {
                    objectivevec++;
                    if (objectivevec >= path.Count)
                    {
                        objectivevec--;
                        state = VillagerState.walkingBack;
                    }
                }
            }
            if (state == VillagerState.walkingBack)
            {
                if ((posman.pos - path[objectivevec]).Length() < 1)
                {
                    objectivevec--;
                    if (objectivevec <= 0)
                    {
                        state = VillagerState.returning;
                        Return();
                    }
                }
            }
            base.Update(elapsedTime_);
        }

        void AddResource()
        {
            if(type == "farmer")
                GameData.food++;
            if (type == "miner")
                GameData.ores++;
            if (type == "lumberjack")
                GameData.wood++;
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
            GameData.citizensOutside--;
        }

        void Return()
        {
            GameData.availableCitizens++;
            GameData.citizensOutside--;
            exists = false;
        }

        public override void Draw(SpriteBatch sb_, bool flipH_ = false, bool flipV_ = false, float angle_ = 0)
        {
            flipH_ = state == VillagerState.walkingBack;
            base.Draw(sb_, flipH_, flipV_, angle_);
        }
    }
}
