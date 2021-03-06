﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Drawing;
using Microsoft.Xna.Framework;
using MonoGame.FZT.Sound;

namespace LD43
{
    public static class GameData
    {
        public static float GameTick = 15f;

        public static int TotalCitizens { get { return availableCitizens + citizensOutside; } set { } }

        public static int priests, availableCitizens, citizensOutside;

        public static int ores, wood, food;
        public static int godAnger, villageHealth, godHunger;
        public static bool pitOpen;

        public static int revolts, attacks;

        public static bool GodAppeared;

        public static int madness;

        public static bool cultExists;

        public static int day = 1, daysUntilDoom;

        public static int OreGain { get { return mineLevel * villageLevel; } }
        public static int WoodGain { get { return forestLevel * villageLevel * 2; } }
        public static int FoodGain { get { return fieldsLevel * 2 * villageLevel; } }
        public static int MaxVillagers { get { return 5 * villageLevel; } }
        public static int Holiness { get { return churchLevel; } }
        public static int VillagerGain { get { return villageLevel; } }

        public static int GodFeed { get { return day * 3; } }
        public static int HungerIncrease { get { return day * villageLevel; } }
        public static int MadnessGain { get { return TotalCitizens; } }
        public static int MadnessLoss { get { return TotalCitizens; } }

        public static int mineLevel, churchLevel, forestLevel, fieldsLevel, villageLevel;
        
        static int ChurchWoodCost { get { return churchLevel * 15; } }
        static int FieldsWoodCost { get { return fieldsLevel * 2; } }
        static int MineWoodCost { get { return mineLevel * 3; } }
        static int VillageWoodCost { get { return villageLevel * 4; } }
        static int ForestWoodCost { get { return forestLevel; } }

        static int ChurchOreCost { get { return churchLevel * 7; } }
        static int FieldsOreCost { get { return fieldsLevel; } }
        static int MineOreCost { get { return mineLevel; } }
        static int VillageOreCost { get { return villageLevel; } }
        static int ForestOreCost { get { return forestLevel; } }

        public static int maxLevel = 5;


        public static List<LinkedVector> path = new List<LinkedVector>();
        public static LinkedVector townMiddle;

        //FUNCTIONS
        public static int LevelOfBuilding(string name_)
        {
            if (name_ == "field" ) { return fieldsLevel; }
            if (name_ == "city" ) { return villageLevel; }
            if (name_ == "forest" ) { return forestLevel; }
            if (name_ == "church" ) { return churchLevel; }
            if (name_ == "mine") { return mineLevel; }
            return 0;
        }

        public static void Initialize()
        {
            GodAppeared = false;

            day = 1;

            priests = 1;
            availableCitizens = 3;
            citizensOutside = 0;
            ores = wood = food = 5;

            madness = 0;

            attacks = revolts = 0;

            godAnger = 0;
            villageHealth = 100;        

            villageLevel = forestLevel = mineLevel = fieldsLevel = churchLevel = 1;

            godHunger = 0;
            cultExists = false;

            daysUntilDoom = 10;

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
            day++;

            ores += OreGain;
            wood += WoodGain;
            food += FoodGain;

            madness -= MadnessLoss;
            if(madness < 0) { madness = 0; }

            if(godAnger < 0) { godAnger = 0; }
           
            food -= TotalCitizens;

            godAnger += godHunger;

            godHunger += HungerIncrease;

            if(food < 0) { villageHealth += food; food = 0;  if (villageHealth < 0) { villageHealth = 0; } }
            if(godAnger > 100) { godAnger = 100; }

            availableCitizens += VillagerGain;

            if (cultExists) { daysUntilDoom--; }
            if(daysUntilDoom < 0) { daysUntilDoom=0;}
        }

        public static bool CanUpgrade(string name_)
        {
            if (name_ == "field" && fieldsLevel == maxLevel) { return false; }
            if (name_ == "city" && villageLevel == maxLevel) { return false; }
            if (name_ == "forest" && forestLevel == maxLevel) { return false; }
            if (name_ == "church" && churchLevel == maxLevel) { return false; }
            if (name_ == "mine" && mineLevel == maxLevel) { return false; }
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
            if (name_ == "field" && fieldsLevel == maxLevel) { return "Field is at max level"; }
            if (name_ == "city" && villageLevel == maxLevel) { return "City is at max level"; }
            if (name_ == "forest" && forestLevel == maxLevel) { return "Forest is at max level"; }
            if (name_ == "church" && churchLevel == maxLevel) { return "Church is at max level"; }
            if (name_ == "mine" && mineLevel == maxLevel) { return "Mine is at max level"; }
            return "Upgrade cost: " + wood +"/" + WoodPerLevel(name_) + " wood and "+ ores + "/" + OresPerLevel(name_)+" ores";
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

            SoundManager.PlayEffect("destroy");
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