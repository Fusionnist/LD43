using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.FZT.Assets;
using Microsoft.Xna.Framework;

namespace LD43
{
    public static class GameData
    {
        public static int  priests, availableCitizens;
        public static int ores, wood, food, holiness;
        public static int godAnger, villageHealth, godHunger;
        public static bool pitOpen;
        public static int oreGain, woodGain, foodGain, maxVillagers, maxPriests;

        public static List<LinkedVector> path = new List<LinkedVector>();
        public static LinkedVector townMiddle;

        public static void Initialize()
        {
            priests = 1;
            availableCitizens = 5;
            ores = wood = food;
            holiness = 0;
            godAnger = 0;
            villageHealth = 100;
            oreGain = woodGain = foodGain = 1;
            maxVillagers = 10;
            maxPriests = 1;
            godHunger = 0;

            CreatePath();
        }

        static void CreatePath()
        {
            //create centers
            townMiddle = new LinkedVector(new Vector2(30, 30));
            //create default path
            List<LinkedVector> path1 = new List<LinkedVector>();
            path1.Add(townMiddle);
            AddToChain(path1, new LinkedVector(new Vector2(60, 60)));
            AddToChain(path1, new LinkedVector(new Vector2(100, 60)));

            List<LinkedVector> path2 = new List<LinkedVector>();
            path2.Add(townMiddle);
            AddToChain(path2, new LinkedVector(new Vector2(60, 30)));
            AddToChain(path2, new LinkedVector(new Vector2(100, 30)));
        }

        public static List<Vector2> GetRandomPath()
        {
            Random r = new Random();
            int segments = r.Next(5, 10);
            List<Vector2> answer = new List<Vector2>(){townMiddle.vec};
            LinkedVector current = townMiddle;
            for(int x = 0; x < segments; x++)
            {
                current = current.links[r.Next(0, current.links.Count)];
                answer.Add(current.vec);
            }
            return answer;
        }

        static void AddToChain(List<LinkedVector> vs_, LinkedVector v_)
        {
            vs_[vs_.Count - 1].LinkTo(v_);
            vs_.Add(v_);
            path.Add(v_);
        }

        public static void Tick()
        {
            if(godHunger < 0) { godHunger = 0; }
            {
                if(godAnger < 0) { godAnger = 0; }
            }

            food -= availableCitizens + priests;

            godAnger += godHunger;

            godHunger += availableCitizens;

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