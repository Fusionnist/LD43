using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LD43
{
    public static class GameData
    {
        public static int  priests, citizens;
        public static int ores, wood, food, holiness;
        public static int godAnger, villageHealth, godHunger;
        public static bool pitOpen;
        public static int oreGain, woodGain, foodGain, maxVillagers, maxPriests;

        public static void Initialize()
        {
            priests = 1;
            citizens = 5;
            ores = wood = food;
            holiness = 0;
            godAnger = 0;
            villageHealth = 100;
            oreGain = woodGain = foodGain = 1;
            maxVillagers = 10;
            maxPriests = 1;
            godHunger = 0;
        }

        public static void Tick()
        {
            if(godHunger < 0) { godHunger = 0; }
            {
                if(godAnger < 0) { godAnger = 0; }
            }

            food -= citizens + priests;

            godAnger += godHunger;

            godHunger += citizens;

            if(food < 0) { villageHealth += food; food = 0;  }
            if(godAnger > 100) { villageHealth -= (godAnger - 100); godAnger = 100; }

            
        }

        public static bool CanUpgrade(string name_)
        {
            return false;
        }

        public static string UpgradeCost(string name_)
        {
            return "cost: 1 pp and 34 bob";
        }

        public static void Upgrade(string name_)
        {

        }
    }
}