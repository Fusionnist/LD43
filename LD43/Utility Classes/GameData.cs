using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Drawing;
using Microsoft.Xna.Framework;

namespace LD43
{
    public static class GameData
    {
        public static int TotalCitizens { get { return availableCitizens + citizensOutside; } set { } }

        public static int priests, availableCitizens, citizensOutside;

        public static int ores, wood, food;
        public static int godAnger, villageHealth, godHunger;
        public static bool pitOpen;


        public static int madness;

        public static bool cultExists;

        public static int day = 1, daysUntilDoom;

        public static int OreGain { get { return mineLevel; } }
        public static int WoodGain { get { return forestLevel; } }
        public static int FoodGain { get { return fieldsLevel * 3; } }
        public static int MaxVillagers { get { return 5 * villageLevel; } }
        public static int Holiness { get { return churchLevel; } }
        public static int VillagerGain { get { return villageLevel; } }

        static int mineLevel, churchLevel, forestLevel, fieldsLevel, villageLevel;
        
        static int ChurchWoodCost { get { return churchLevel; } }
        static int FieldsWoodCost { get { return fieldsLevel; } }
        static int MineWoodCost { get { return mineLevel; } }
        static int VillageWoodCost { get { return villageLevel; } }
        static int ForestWoodCost { get { return forestLevel; } }

        static int ChurchOreCost { get { return churchLevel; } }
        static int FieldsOreCost { get { return fieldsLevel; } }
        static int MineOreCost { get { return mineLevel; } }
        static int VillageOreCost { get { return villageLevel; } }
        static int ForestOreCost { get { return forestLevel; } }


        public static List<LinkedVector> path = new List<LinkedVector>();
        public static LinkedVector townMiddle;

        //FUNCTIONS
        public static void Initialize()
        {
            day = 1;

            priests = 1;
            availableCitizens = 3;
            citizensOutside = 0;
            ores = wood = food = 5;

            madness = 0;

            godAnger = 0;
            villageHealth = 100;        

            villageLevel = forestLevel = mineLevel = fieldsLevel = churchLevel = 1;

            godHunger = 0;
            cultExists = false;

            CreatePath();
        }

        static void CreatePath()
        {
            //create centers
            townMiddle = new LinkedVector(new Vector2(260, 160));
            //create default path
            List<LinkedVector> path1 = new List<LinkedVector>();
            path1.Add(townMiddle);
            AddToChain(path1, new LinkedVector(new Vector2(289, 124)));
            AddToChain(path1, new LinkedVector(new Vector2(264, 105)));
            AddToChain(path1, new LinkedVector(new Vector2(285, 92)));
            AddToChain(path1, new LinkedVector(new Vector2(270, 80)));
            AddToChain(path1, new LinkedVector(new Vector2(260, 80)));
            AddToChain(path1, new LinkedVector(new Vector2(236, 59)));
            AddToChain(path1, new LinkedVector(new Vector2(259, 59)));

            List<LinkedVector> path2 = new List<LinkedVector>();
            path2.Add(townMiddle);
            AddToChain(path2, new LinkedVector(new Vector2(75, 160)));
            AddToChain(path2, new LinkedVector(new Vector2(104, 137)));
            AddToChain(path2, new LinkedVector(new Vector2(86, 120)));
            AddToChain(path2, new LinkedVector(new Vector2(38, 120)));
            AddToChain(path2, new LinkedVector(new Vector2(39, 106)));
            AddToChain(path2, new LinkedVector(new Vector2(76, 70)));
            AddToChain(path2, new LinkedVector(new Vector2(70, 64)));
        }

        public static List<Vector2> GetRandomPath()
        {
            Random r = new Random();
            int segments = r.Next(1, 10);
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
            ores += OreGain;
            wood += WoodGain;
            food += FoodGain;

            if (godHunger < 0) { godHunger = 0; }
            {
                if(godAnger < 0) { godAnger = 0; }
            }

            food -= availableCitizens;

            godAnger += godHunger;

            godHunger += availableCitizens;

            if(food < 0) { villageHealth += food; food = 0;  }
            if(godAnger > 100) { godAnger = 100; }

            availableCitizens += VillagerGain;
        }

        public static bool CanUpgrade(string name_)
        {
            if (OresPerLevel(name_) <= ores && WoodPerLevel(name_) <= wood) { return true; }
            return false;
        }

        static int OresPerLevel(string name_)
        {
            if (name_ == "forest") { return ForestOreCost; }
            if (name_ == "field") { return FieldsOreCost; }
            if (name_ == "mine") { return MineOreCost; }
            if (name_ == "city") { return VillageOreCost; }
            if (name_ == "church") { return ChurchOreCost; }

            return 0;
        }

        static int WoodPerLevel(string name_)
        {
            if (name_ == "forest") { return ForestWoodCost; }
            if (name_ == "field") { return FieldsWoodCost; }
            if (name_ == "mine") { return MineWoodCost; }
            if (name_ == "city") { return VillageWoodCost; }
            if (name_ == "church") { return ChurchWoodCost; }

            return 0;
        }

        public static string UpgradeCost(string name_)
        {
            return "upgrade cost: " + wood +"/" + WoodPerLevel(name_) + " wood and "+ ores + "/" + OresPerLevel(name_)+" ores";
        }

        public static void DestroyBuilding()
        {
            Random r = new Random();

            int i = r.Next(0, 5);

            if (i == 0) { forestLevel = 1; ParticleSystem.CreateInstance(new Vector2(15, 117), "smoke", true, 3); }
            if (i == 1) { mineLevel = 1; ParticleSystem.CreateInstance(new Vector2(50, 64), "smoke", true, 3); }
            if (i == 2) { villageLevel = 1; ParticleSystem.CreateInstance(new Vector2(260, 150), "smoke", true, 3); }
            if (i == 3) { churchLevel = 1; ParticleSystem.CreateInstance(new Vector2(240, 50), "smoke", true, 3); }
            if (i == 4) { fieldsLevel = 1; ParticleSystem.CreateInstance(new Vector2(80, 150), "smoke", true, 3); }
        }

        public static void Upgrade(string name_)
        {
            if (name_ == "forest") { ores -= ForestOreCost; wood -= ForestWoodCost; forestLevel++; }
            if (name_ == "field") { ores -= FieldsOreCost; wood -= FieldsWoodCost; fieldsLevel++; }
            if (name_ == "mine") { ores -= MineOreCost; wood -= MineWoodCost; mineLevel++; }
            if (name_ == "city") { ores -= VillageOreCost; wood -= VillageWoodCost; villageLevel++; }
            if (name_ == "church") { ores -= ChurchOreCost; wood -= ChurchWoodCost; churchLevel++; }
        }
    }
}