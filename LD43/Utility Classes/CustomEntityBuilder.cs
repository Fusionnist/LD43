using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Data;
using MonoGame.FZT.Drawing;
using MonoGame.FZT.Physics;

namespace LD43
{
    public class CustomEntityBuilder : EntityBuilder
    {
        public CustomEntityBuilder() { }
        public override Entity CreateEntity(string type_, DrawerCollection dc_, PositionManager pos_, List<Property> props_, string name_)
        {
            //Create the default entity
            if (type_ == "entity")
            {
                return new Entity(dc_, pos_, props_, name_, type_);
            }

            return base.CreateEntity(type_, dc_, pos_, props_, name_);
        }
    }
}
