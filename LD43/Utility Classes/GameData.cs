using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43
{
    public static class GameData
    {
        public static int miners, lumberjacks, priests, farmers, citizens;
        public static int ores, wood, godpower, food;
        public static int godAnger, villageHealth;

        public static void Initialize()
        {
            miners = lumberjacks = priests = farmers = citizens = 1;
            ores = wood = food = godpower = 10;
            godAnger = 0;
            villageHealth = 100;
        }
    }
}
