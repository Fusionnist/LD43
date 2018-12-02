using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

using MonoGame.FZT;
using MonoGame.FZT.Assets;
using MonoGame.FZT.Data;
using MonoGame.FZT.Drawing;
using MonoGame.FZT.Input;
using MonoGame.FZT.Physics;
using MonoGame.FZT.Sound;
using MonoGame.FZT.UI;
using MonoGame.FZT.XML;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Xml.Linq;


namespace LD43
{
    enum GameState { Game, Menu }
    enum GameSubState { Game, Main, End, Pause, Tutorial }
    enum GameEnding { churchWins, villageDestroyed, revolt, cult, starvation }

    public class Game1 : Game
    {
        //Utility
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SceneCollection scenes;
        CustomEntityBuilder eBuilder;
        CursorManager cursor;

        //Objects
        InputProfile ipp;

        UISystem currentUI;
        UISystem mainUI, tutorialUI, pauseUI, endgameUI, gameUI;

        TextureDrawer cursorTex, currentBG, meterTex;
        TextureDrawer gameBG, mainMenuBG, endBG, tutorialBG, pauseBG, icons;
        TextureDrawer goodEnding, destructionEnding, revoltEnding, starvationEnding, cultEnding;

        FontDrawer fdrawer;

        MonoGame.FZT.Assets.Timer transitionTimer, gameTick;
        bool transitioning, shouldReset, showData, transitionIN; //as opposed to transition OUT
        Random rng;
        GameEnding ending;

        //Data
        Point wDims, vDims;
        GameState currentState, nextState;
        GameSubState currentSubState, nextSubState;
        bool switchedState, changeTooltipText; // set this to false if you want to have a tooltip triggered in UpdateGame
        string tooltipText;
        Vector2 tooltipPos;
        
        public Game1()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            wDims = new Point(1920 / 3 * 2, 1080 / 3 * 2);
            vDims = new Point(320, 180);

            transitionIN = true;

            //set dat up
            graphics.PreferredBackBufferWidth = wDims.X;
            graphics.PreferredBackBufferHeight = wDims.Y;
            Window.IsBorderless = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            currentState = GameState.Menu;
            currentSubState = GameSubState.Main;

            CreateInputProfile();
            CreateScenes();

            transitionTimer = new MonoGame.FZT.Assets.Timer(1f);
            gameTick = new MonoGame.FZT.Assets.Timer(12f);

            PhysicsManager.CreateWorld();
            PhysicsManager.SetUnitRatio(64);
            PhysicsManager.SetupDebugview(GraphicsDevice, Content);

            cursor = new CursorManager();

            rng = new Random();

            fdrawer = new FontDrawer();
            List<TextureDrawer> font = new List<TextureDrawer>();
            string junk = "abcdefghijklmnopqrstuvwxyz\".,;:?!'\"][-+/\\^&é0123456789";
            Texture2D tex = Content.Load<Texture2D>("Placeholder/font2");
            for (int i = 0; i < junk.Length; i++)
            {
                font.Add(new TextureDrawer(tex, new TextureFrame(new Rectangle(8 * i, 0, 8, 11), new Point(0, 0)), null, junk[i].ToString(), null, null));
            }
            fdrawer.fonts.Add(new DrawerCollection(font, "font"));
            tooltipText = "";
            changeTooltipText = true;
            tooltipPos = new Vector2(33, 0);

            currentUI = mainUI;
            currentBG = mainMenuBG;
        }
        void CreateScenes()
        {
            scenes = new SceneCollection();

            //game scene
            scenes.scenes.Add(new Scene(new RenderTarget2D(GraphicsDevice, vDims.X, vDims.Y), "game"));
            //menu scene
            scenes.scenes.Add(new Scene(new RenderTarget2D(GraphicsDevice, vDims.X, vDims.Y), "menu"));
            //overlay
            scenes.scenes.Add(new Scene(new RenderTarget2D(GraphicsDevice, vDims.X, vDims.Y), "overlay"));
            //base
            scenes.scenes.Add(new Scene(new RenderTarget2D(GraphicsDevice, vDims.X, vDims.Y), new Rectangle(0, 0, vDims.X, vDims.Y), new Rectangle(0, 0, wDims.X, wDims.Y), "base"));

        } //explicit name
        void CreateGroups()
        {

        } //entity groups
        void CreateInputProfile()
        {
            ipp = new InputProfile(new List<KeyManager>()
            {
                new KeyManager(Keys.Left, "left"),
                new KeyManager(Keys.Right, "right"),
                new KeyManager(Keys.Up, "up"),
                new KeyManager(Keys.Down, "down")
            });
        } //check whatever is plugged in to create input settings
        

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ElementCollection.ReadDocument(XDocument.Load("Content/Xml/Registry.xml"));
            SpriteSheetCollection.LoadFromElementCollection(Content);

            cursorTex = SpriteSheetCollection.GetTex("static", "PlaceholderSheet", "cursor");
            icons = SpriteSheetCollection.GetTex("idle", "buttons", "question");
            meterTex = new TextureDrawer(Content.Load<Texture2D>("Placeholder/meter"));

            LoadBGs();
            CreateUI();

            SoundManager.AddSong(Content.Load<Song>("Audio/PrototypeMusic3"), "game");
            //SoundManager.AddSong(Content.Load<Song>("Audio/tinymusic"), "game");
            SoundManager.AddSong(Content.Load<Song>("Audio/MenuTrack"), "main");
            SoundManager.AddEffect(Content.Load<SoundEffect>("Placeholder/hurty"), "temp1");
            SoundManager.AddEffect(Content.Load<SoundEffect>("Placeholder/jump"), "temp2");

            SoundManager.PlaySong("main");

            ParticleSystem.AcquireTxture(SpriteSheetCollection.GetTex("smoke", "smoke", "smoke"));

            CreateNewGame();
        }
        void CreateNewGame()
        {
            ParticleSystem.Flush();

            EntityCollection.Flush();

            GameData.Initialize();

            CreateGod();

            Building church = new Building(
                new DrawerCollection(
                    new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "church"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "church"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "church") },
                    "church"),
                    new PositionManager(new Vector2(229, 9)),
                    new List<Property>(),
                    .1f,
                    "church",
                    "this is a building"
                );
            Building field = new Building(
               new DrawerCollection(
                   new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "buildingStuff", "farm"),
                        SpriteSheetCollection.GetTex("hovered", "moreBuildings", "farm"),
                        SpriteSheetCollection.GetTex("clicked", "moreBuildings", "farm") },
                   "field"),
                   new PositionManager(new Vector2(43, 132)),
                   new List<Property>(),
                   .1f,
                   "field",
                   "this is a building"
               );
            Building mine = new Building(
               new DrawerCollection(
                   new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "mine"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "mine"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "mine") },
                   "mine"),
                   new PositionManager(new Vector2(31, 55)),
                   new List<Property>(),
                   .1f,
                   "mine",
                   "this is a building"
               );
            Building forest = new Building(
               new DrawerCollection(
                   new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "forest"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "forest"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "forest") },
                   "forest"),
                   new PositionManager(new Vector2(0, 123)),
                   new List<Property>(),
                   .1f,
                   "forest",
                   "this is a building"
               );
            Building city = new Building(
               new DrawerCollection(
                   new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "buildingStuff", "city"),
                        SpriteSheetCollection.GetTex("hovered", "moreBuildings", "city"),
                        SpriteSheetCollection.GetTex("clicked", "moreBuildings", "city") },
                   "city"),
                   new PositionManager(new Vector2(217, 124)),
                   new List<Property>(),
                   .1f,
                   "city",
                   "this is a building"
               );
            Building bridge = new Building(
               new DrawerCollection(
                   new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "buildingStuff", "bridge"),
                        SpriteSheetCollection.GetTex("hovered", "moreBuildings", "bridge"),
                        SpriteSheetCollection.GetTex("clicked", "moreBuildings", "bridge") },
                   "bridge"),
                   new PositionManager(new Vector2(117, 158)),
                   new List<Property>(),
                   .1f,
                   "bridge",
                   "this is a building"
               );

            EntityCollection.CreateGroup("building", "buildings");
            EntityCollection.CreateGroup("cunt", "villagers");
            EntityCollection.CreateGroup("god", "god");
            EntityCollection.AddEntities(new List<Entity>() { church, field, mine, forest, city, bridge });

           
        } //create whatever's needed to start a new game
        void CreateUI()
        {
            //NOTE: CREATE BUTTON VARIANTS
            mainUI = new UISystem(new List<Button>()
            {
                new Button("startGame", new Rectangle(220,42,32,16), 
                SpriteSheetCollection.GetTex("idle","buttons","play"),
                SpriteSheetCollection.GetTex("pressed","buttons","play"),
                SpriteSheetCollection.GetTex("hovered","buttons","play")
                ),
                new Button("tutorial", new Rectangle(220,82,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","square"),
                SpriteSheetCollection.GetTex("pressed","buttons","square"),
                SpriteSheetCollection.GetTex("hovered","buttons","square")
                ),
                new Button("exitGame", new Rectangle(220,122,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","quit"),
                SpriteSheetCollection.GetTex("pressed","buttons","quit"),
                SpriteSheetCollection.GetTex("hovered","buttons","quit")
                )
            }
            );

            endgameUI = new UISystem(new List<Button>()
            {
                new Button("mainMenu", new Rectangle(100,0,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","menu"),
                SpriteSheetCollection.GetTex("pressed","buttons","menu"),
                SpriteSheetCollection.GetTex("hovered","buttons","menu")
                ),
                new Button("restartGame", new Rectangle(100,32,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","restart"),
                SpriteSheetCollection.GetTex("pressed","buttons","restart"),
                SpriteSheetCollection.GetTex("hovered","buttons","restart")
                ),
                  new Button("exitGame", new Rectangle(100,64,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","quit"),
                SpriteSheetCollection.GetTex("pressed","buttons","quit"),
                SpriteSheetCollection.GetTex("hovered","buttons","quit")
                ),
            }
            );

            tutorialUI = new UISystem(new List<Button>()
            {
                new Button("startGame", new Rectangle(64,160,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","play"),
                SpriteSheetCollection.GetTex("pressed","buttons","play"),
                SpriteSheetCollection.GetTex("hovered","buttons","play")
                ),
                 new Button("mainMenu", new Rectangle(224,160,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","menu"),
                SpriteSheetCollection.GetTex("pressed","buttons","menu"),
                SpriteSheetCollection.GetTex("hovered","buttons","menu")
                )
            }
           );

            pauseUI = new UISystem(new List<Button>()
            {
                new Button("resumeGame", new Rectangle(60,120,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","play"),
                SpriteSheetCollection.GetTex("pressed","buttons","play"),
                SpriteSheetCollection.GetTex("hovered","buttons","play")
                ),
                   new Button("mainMenu", new Rectangle(144,120,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","menu"),
                SpriteSheetCollection.GetTex("pressed","buttons","menu"),
                SpriteSheetCollection.GetTex("hovered","buttons","menu")
                ),
                 new Button("restartGame", new Rectangle(228,120,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","restart"),
                SpriteSheetCollection.GetTex("pressed","buttons","restart"),
                SpriteSheetCollection.GetTex("hovered","buttons","restart")
                )
            }
           );

            gameUI = new UISystem(new List<Button>()
            {
                new Button("pauseGame", new Rectangle(288,0,32,16),
                SpriteSheetCollection.GetTex("idle","buttons","pause"),
                SpriteSheetCollection.GetTex("pressed","buttons","pause"),
                SpriteSheetCollection.GetTex("hovered","buttons","pause")
                )
            }
           );
        }
        void LoadBGs()
        {
            mainMenuBG = SpriteSheetCollection.GetTex("bg", "PlaceholderBGs", "menu");
            gameBG = SpriteSheetCollection.GetTex("bg", "PlaceholderBGs", "game");
            pauseBG = SpriteSheetCollection.GetTex("bg", "PlaceholderBGs", "pause");
            endBG = SpriteSheetCollection.GetTex("bg", "PlaceholderBGs", "end");
            tutorialBG = SpriteSheetCollection.GetTex("bg", "PlaceholderBGs", "tutorial");
            goodEnding = SpriteSheetCollection.GetTex("ending", "endings", "good");
            destructionEnding = SpriteSheetCollection.GetTex("ending", "endings", "destroyed");
            revoltEnding = SpriteSheetCollection.GetTex("ending", "endings", "revolted");
            starvationEnding = SpriteSheetCollection.GetTex("ending", "endings", "starved");
            cultEnding = SpriteSheetCollection.GetTex("ending", "endings", "awakened");
        }
        void CreateGod()
        {
            EntityCollection.AddEntity(
                new God(
                    new DrawerCollection(new List<TextureDrawer>()
                    {
                        SpriteSheetCollection.GetTex("idle", "god", "god"),
                        SpriteSheetCollection.GetTex("attack", "god", "god")
                    }, "tex"),
                    new PositionManager(new Vector2(40, 20)),
                    new List<Property>(), "god", "god"));
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            float es = (float)gameTime.ElapsedGameTime.TotalSeconds;

            SoundManager.Update(es);
            UpdateState(es);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            ipp.Update(Keyboard.GetState(), GamePad.GetState(0));
            cursor.Update();
            UpdateSwitch(es);
            ReadCommandQueue();
            if(currentState == GameState.Game && nextSubState != GameSubState.End)
                HandleEnding();
            EntityCollection.RecycleAll();

            base.Update(gameTime);
        }
        void ReadCommandQueue()
        {
            if (currentUI.IssuedCommand("startGame"))
            {
                shouldReset = true;
                ToggleState(GameState.Game, GameSubState.Game);
               
            }

            if (currentUI.IssuedCommand("endGame"))
            {
                ToggleState(GameState.Menu, GameSubState.End);
                //ending = GameEnding.villageDestroyed;
            }

            if (currentUI.IssuedCommand("endGameChurch"))
            {
                ToggleState(GameState.Menu, GameSubState.End);
                //ending = GameEnding.churchWins;
            }

            if (currentUI.IssuedCommand("endGameRevolt"))
            {
                ToggleState(GameState.Menu, GameSubState.End);
                //ending = GameEnding.revolt;
            }

            if (currentUI.IssuedCommand("restartGame"))
            {
                shouldReset = true;
                ToggleState(GameState.Game, GameSubState.Game);
               
            }

            if (currentUI.IssuedCommand("resumeGame"))
            {
                ToggleState(GameState.Game, GameSubState.Game);
            }

            if (currentUI.IssuedCommand("mainMenu"))
            {
                ToggleState(GameState.Menu, GameSubState.Main);
            }

            if (currentUI.IssuedCommand("pauseGame"))
            {
                ToggleState(GameState.Game, GameSubState.Pause);
            }

            if (currentUI.IssuedCommand("tutorial"))
            {
                ToggleState(GameState.Menu, GameSubState.Tutorial);
            }

            if (currentUI.IssuedCommand("exitGame"))
            {
                Exit();
            }
        }
        void UpdateState(float es_)
        {
            if (transitioning)
            {
                transitionTimer.Update(es_);
                if (transitionTimer.Complete())
                {
                    if (transitionIN)
                    {
                        transitionIN = false;
                        transitionTimer.Reset();
                        switchedState = true;
                    }
                    else
                    {
                        transitioning = false;
                        transitionIN = true;
                    }
                }
            }
            if (switchedState)
            {
                currentState = nextState;
                currentSubState = nextSubState;
                switchedState = false;
                if (shouldReset)
                {
                    shouldReset = false;
                    CreateNewGame();
                }
                StateChangeSwitch();
            }
        } //set the next state and substate to the current one at the start of the update
        void StateChangeSwitch()
        {
            switch (currentState)
            {
                case GameState.Game:
                    SoundManager.QueueSong("game", true);
                    switch (currentSubState)
                    {
                        case GameSubState.Game: //GAME-GAME
                            currentUI = gameUI;
                            currentBG = gameBG;
                            break;

                        case GameSubState.Pause: //GAME-MAIN
                            currentUI = pauseUI;
                            
                            break;
                    }
                    break;
                case GameState.Menu:
                    SoundManager.QueueSong("main", true);
                    switch (currentSubState)
                    {
                        case GameSubState.Main: //MENU-MAIN
                            currentUI = mainUI;
                            currentBG = mainMenuBG;
                            break;

                        case GameSubState.Tutorial: //MENU-TUT
                            currentUI = tutorialUI;
                            currentBG = tutorialBG;
                            break;

                        case GameSubState.End: //MENU-MAIN
                            currentUI = endgameUI;
                            switch (ending)
                            {
                                case GameEnding.churchWins:
                                    currentBG = goodEnding;
                                    break;
                                case GameEnding.revolt:
                                    currentBG = revoltEnding;
                                    break;
                                case GameEnding.villageDestroyed:
                                    currentBG = destructionEnding;
                                    break;
                                case GameEnding.starvation:
                                    currentBG = starvationEnding;
                                    break;
                                case GameEnding.cult:
                                    currentBG = cultEnding;
                                    break;
                                default:
                                    currentBG = endBG;
                                    break;
                            }
                            break;
                    }
                    break;
            }
        }
        void UpdateSwitch(float es_)
        {
            switch (currentState)
            {
                case GameState.Game:
                    switch (currentSubState)
                    {
                        case GameSubState.Game: //GAME-GAME
                            UpdateMenu(es_);
                            UpdateGame(es_);
                            break;

                        case GameSubState.Pause: //GAME-MAIN
                            UpdateMenu(es_);
                            break;
                    }
                    break;
                case GameState.Menu:
                    switch (currentSubState)
                    {
                        case GameSubState.Main: //MENU-MAIN
                            UpdateMenu(es_);
                            break;

                        case GameSubState.Tutorial: //MENU-TUT
                            UpdateMenu(es_);
                            break;

                        case GameSubState.End: //MENU-MAIN
                            UpdateMenu(es_);
                            break;
                    }
                    break;
            }
        } //update things based on game state
        void UpdateGame(float es_)
        {
            ParticleSystem.UpdateAll(es_);

            EntityCollection.UpdateAll(es_);
            gameTick.Update(es_);
            if (gameTick.Complete())
            {
                GameData.Tick();
                gameTick.Reset();

                if (GameData.citizensOutside < GameData.MaxVillagers)
                {
                    CreateVillager("cunt");
                }

            }
            
            UpdateHoveredLocation();

            if (GameData.madness >= 90 && !GameData.cultExists)
            {
                GameData.cultExists = true;
                Building cult = new Building(
                new DrawerCollection(
                    new List<TextureDrawer>() {
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "cult"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "cult"),
                        SpriteSheetCollection.GetTex("idle", "defBuildings", "cult") },
                    "cult"),
                    new PositionManager(new Vector2(46, 24)),
                    new List<Property>(),
                    5,
                    "cult",
                    ""
                );
                EntityCollection.AddEntity(cult);
            }

            changeTooltipText = true;
            bool x = false;
            foreach (Building building in EntityCollection.GetGroup("buildings"))
            {
                if (building.Name == "cult")
                    building.hoveredText = "";
                else if (building.Name == "bridge")
                    building.hoveredText = "the god's anger is at " + GameData.godAnger.ToString() + " and his hunger is at " + GameData.godHunger.ToString() + ". you currently have " + GameData.TotalCitizens.ToString() + " villagers. Appease the god?";
                else
                    building.hoveredText = GameData.UpgradeCost(building.Name) + ". building level:" + building.level.ToString();
                if (!x && new Rectangle(building.posman.pos.ToPoint(), building.GetBounds().Size).Contains(scenes.GetScene("base").ToVirtualPos(cursor.RawPos())))
                {
                    if (cursor.GetClicked() && GameData.CanUpgrade(building.Name))
                    {
                       if (!building.wasClicked)
                       {
                           switch (building.Name)
                                {
                                    case "field":
                                        //CreateVillager("farmer");
                                        if (GameData.CanUpgrade("field"))
                                            GameData.Upgrade("field");
                                        SoundManager.PlayEffect("temp1");
                                        break;
                                    case "mine":
                                        //CreateVillager("miner");
                                        if (GameData.CanUpgrade("mine"))
                                            GameData.Upgrade("mine");
                                        SoundManager.PlayEffect("temp1");
                                        break;
                                    case "forest":
                                        //CreateVillager("lumberjack");
                                        if (GameData.CanUpgrade("forest"))
                                            GameData.Upgrade("forest");
                                        SoundManager.PlayEffect("temp1");
                                        break;
                                    case "city":
                                        if (GameData.CanUpgrade("city"))
                                            GameData.Upgrade("city");
                                        SoundManager.PlayEffect("temp1");
                                        break;
                                    case "church":
                                        if (GameData.CanUpgrade("church"))
                                            GameData.Upgrade("church");
                                        SoundManager.PlayEffect("temp1");
                                        break;
                                    case "bridge":
                                        foreach (God god in EntityCollection.GetGroup("god"))
                                        {
                                            god.Attack();
                                        }   
                                        break;
                                }
                       }
                       building.Click();
                    }
                    else
                    {
                        if (building.hoveredText != null)
                        {
                            tooltipText = building.hoveredText; changeTooltipText = false;
                        }
                        if (GameData.CanUpgrade(building.Name))
                            building.isHovered = true;
                    }
                    x = true;
                }
                else
                    building.isHovered = false;
            }

            if (new Rectangle(10, 165, GameData.madness / 2, 10).Contains(scenes.GetScene("base").ToVirtualPos(cursor.RawPos())))
            {
                tooltipText = "the village's madness is at " + GameData.madness.ToString() + "/100";
                changeTooltipText = false;
            }
            else if (new Rectangle(135, 165, GameData.daysUntilDoom * 5, 10).Contains(scenes.GetScene("base").ToVirtualPos(cursor.RawPos())) && GameData.cultExists)
            {
                tooltipText = "the village is mad and has created a cult! you have " + GameData.daysUntilDoom.ToString() + " days until doom.";
                changeTooltipText = false;
            }
            else if (new Rectangle(260, 165, (int)((gameTick.timer / gameTick.time) * 50), 10).Contains(scenes.GetScene("base").ToVirtualPos(cursor.RawPos())))
            {
                tooltipText = "the next day starts in " + Math.Ceiling(gameTick.timer).ToString() + " seconds.";
                changeTooltipText = false;
            }

        } //update the movey things
        void UpdateHoveredLocation()
        {
            showData = false;
            Rectangle showDataBounds = new Rectangle(0, 0, 32, 16);
            if (showDataBounds.Contains(scenes.CurrentScene.ToVirtualPos(cursor.RawPos()))){
                showData = true;
            }
        }
        void UpdateMenu(float es_)
        {
            currentUI.UpdateWithMouse(es_, cursor, scenes.CurrentScene.ToVirtualPos(cursor.RawPos()));

            if (currentUI.HoveredButton != null)
                HandleButtonTooltips();
            else
            {
                if (changeTooltipText)
                { tooltipText = ""; }
            }
        } //update the clicky things        
        void CreateVillager(string type_)
        {
            if(GameData.availableCitizens > 0)
            {
                EntityCollection.AddEntity(new Villager(
               new DrawerCollection(new List<TextureDrawer>()
               {
                   SpriteSheetCollection.GetTex("normal", "villager", "villager")
               }, "texes"),
               new PositionManager(new Vector2(360, 100)),
               new List<Property>(),
               "cunt",
               type_
               ));
            }
        }   
        void HandleEnding()
        {
            if(GameData.attacks == 10)
            {
                ToggleState(GameState.Menu, GameSubState.End);
                ending = GameEnding.villageDestroyed;

                if(GameData.mineLevel == 10 && GameData.villageLevel == 10 && GameData.churchLevel == 10 && GameData.fieldsLevel == 10 && GameData.forestLevel == 10)
                {
                    ending = GameEnding.churchWins;
                }
            }
            if (GameData.daysUntilDoom == 0 && GameData.cultExists)
            {
                ToggleState(GameState.Menu, GameSubState.End);
                ending = GameEnding.cult;
            }
            if (GameData.villageHealth == 0)
            {
                ToggleState(GameState.Menu, GameSubState.End);
                ending = GameEnding.starvation;
            }
            if (GameData.revolts == 10)
            {
                ToggleState(GameState.Menu, GameSubState.End);
                ending = GameEnding.revolt;
            }
        }

        void HandleButtonTooltips()
        {
            switch (currentUI.HoveredButton.Command)
            {
                case "mainMenu":
                    tooltipText = "return to main menu?";
                    break;
                case "restartGame":
                    tooltipText = "restart from nothing?";
                    break;
                case "exitGame":
                    tooltipText = "exit the game?";
                    break;
                case "startGame":    
                    tooltipText = "begin the game?";
                    break;
                case "resumeGame":
                    tooltipText = "resume?";
                    break;
                case "pauseGame":
                    tooltipText = "";
                    break;
                case "tutorial":
                    tooltipText = "instructions for the game";
                    break;
                default:
                    tooltipText = "this is a button";
                    break;
            }
        }
        void ToggleState(GameState newState_, GameSubState newSubState_)
        {
            nextState = newState_;
            nextSubState = newSubState_;

            if (nextSubState != GameSubState.Pause && !(currentSubState == GameSubState.Pause && nextState == GameState.Game && !shouldReset))
            {
                SetTransition();
            }
            else
            {
                switchedState = true;
            }
        } //set new nextstate and data based on old and new state           
        void SetTransition()
        {
            transitioning = true;
            transitionTimer.Reset();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue); 
            DrawScenes();
            DrawSwitch();
            //draw to buffer
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            GraphicsDevice.SetRenderTarget(null);

            scenes.DrawScene(spriteBatch, "base");

            spriteBatch.End();
            base.Draw(gameTime);
        } //draw main scene to null target
        void DrawSwitch()
        {
            //draw BG
            scenes.SetupScene(spriteBatch, GraphicsDevice, "base");
            if(currentBG != null)
            {
                currentBG.Draw(spriteBatch, Vector2.Zero);
            }

            switch (currentState)
            {
                case GameState.Game:
                    switch (currentSubState)
                    {
                        case GameSubState.Game:
                            scenes.DrawScene(spriteBatch, "game");
                            scenes.DrawScene(spriteBatch, "menu");
                            break;

                        case GameSubState.Pause:
                            scenes.DrawScene(spriteBatch, "game");
                            scenes.DrawScene(spriteBatch, "menu");
                            break;
                    }
                    break;
                case GameState.Menu:
                    switch (currentSubState)
                    {
                        case GameSubState.Tutorial:
                            scenes.DrawScene(spriteBatch, "menu");
                            break;

                        case GameSubState.End:
                            scenes.DrawScene(spriteBatch, "menu");
                            break;

                        case GameSubState.Main:
                            scenes.DrawScene(spriteBatch, "menu");
                            break;
                    }
                    break;
            }
            scenes.DrawScene(spriteBatch, "overlay");
            cursorTex.Draw(spriteBatch, scenes.CurrentScene.ToVirtualPos(cursor.RawPos()));
            spriteBatch.End();

        } //draw sub scenes to main scene based on states
        void DrawScenes() 
        {
            DrawGame();
            DrawMenu();
            DrawOverlay();
        } //draw what should be on each sub scene
        void DrawGame() 
        {
            scenes.SelectScene("game");
            scenes.SetupScene(spriteBatch, GraphicsDevice);

            //DRAW
            foreach (var ent in EntityCollection.GetGroup("god"))
                ent.Draw(spriteBatch);
            foreach (var ent in EntityCollection.GetGroup("villagers"))
                ent.Draw(spriteBatch);
            foreach (var ent in EntityCollection.GetGroup("buildings"))
                ent.Draw(spriteBatch);

            ParticleSystem.DrawAll(spriteBatch);

            spriteBatch.End();
        } //draw to game scene
        void DrawMenu() 
        {
            scenes.SelectScene("menu");
            scenes.SetupScene(spriteBatch, GraphicsDevice);

            if (currentState == GameState.Game)
            {
                DrawMeter(GameData.madness / 2, new Vector2(10, 165));
                if (GameData.cultExists)
                    DrawMeter(GameData.daysUntilDoom * 5, new Vector2(135, 165));
                DrawMeter((int)((gameTick.timer / gameTick.time) * 50), new Vector2(260, 165));
            }
                
            if (currentSubState == GameSubState.Pause)
            {
                pauseBG.Draw(spriteBatch, Vector2.Zero);
                fdrawer.DrawText("font", "pause", new Rectangle(140, 10, 40, 11), spriteBatch);
            }
            //DRAW
            currentUI.Draw(spriteBatch);
            fdrawer.DrawText("font", tooltipText, new Rectangle((int)tooltipPos.X, (int)tooltipPos.Y, 320 - (int)tooltipPos.X, 180), spriteBatch);

            spriteBatch.End();
        } //draw to menu scene
        void DrawOverlay() 
        {
            scenes.SelectScene("overlay");
            scenes.SetupScene(spriteBatch, GraphicsDevice);

            if (currentState == GameState.Game)
            {
                if (showData && currentSubState == GameSubState.Game)
                {
                    DrawData();
                }
                else
                {
                    icons.Draw(spriteBatch, new Vector2(0, 0));
                }
            }
            else
            {
                if (currentSubState == GameSubState.Tutorial)
                {
                    DrawTutorialText();
                }
            }

            //DRAW
            if(transitioning)
            if (transitionIN)
            {
                GraphicsDevice.Clear(new Color(0, 0, 0, (1 - transitionTimer.timer)));
            }
            else
            {
                GraphicsDevice.Clear(new Color(0, 0, 0, transitionTimer.timer));
            }
            spriteBatch.End();
        } //draw to overlay scene
        void DrawData()
        {
            fdrawer.DrawText("font", "village health: " + GameData.villageHealth,new Rectangle(0,0, 200, 100),spriteBatch);
            fdrawer.DrawText("font", "food: " + GameData.food + "+" + GameData.FoodGain, new Rectangle(0, 11, 150, 100), spriteBatch);
            fdrawer.DrawText("font", "wood: " + GameData.wood + "+" + GameData.WoodGain, new Rectangle(0, 22, 150, 100), spriteBatch);
            fdrawer.DrawText("font", "ores: " + GameData.ores + "+" + GameData.OreGain, new Rectangle(0, 33, 150, 100), spriteBatch);
            fdrawer.DrawText("font", "villagers: " + GameData.TotalCitizens + "+" + GameData.VillagerGain, new Rectangle(0, 44, 150, 100), spriteBatch);
            fdrawer.DrawText("font", "god anger: " + GameData.godAnger + "+" + GameData.godHunger, new Rectangle(0, 55, 150, 100), spriteBatch);
            fdrawer.DrawText("font", "god hunger: " + GameData.godHunger, new Rectangle(0, 66, 150, 100), spriteBatch);
        }
        void DrawPhysicsDebug()
        {
            spriteBatch.Begin();

            Matrix projection = Matrix.CreateScale(1f) * Matrix.CreateOrthographicOffCenter(0, vDims.X / 64f, vDims.Y / 64f, 0, 0, 1) * Matrix.CreateTranslation(new Vector3(0, 0, 0));

            PhysicsManager.DrawDebugview(projection);

            spriteBatch.End();
        } //DEBUG

        void DrawTutorialText()
        {
            fdrawer.DrawText("font", "tutorial", new Rectangle(120, 0, 120, 20), spriteBatch);
            fdrawer.DrawText("font", "you control a village living under the shadow of a bloodthirsty god. your aim is to lead them to a happy end.", new Rectangle(5, 15, 315, 150), spriteBatch);
            fdrawer.DrawText("font", "to do so, you will have to balance all the resources at your disposal, mostly produced by your various buildings which can be upgraded.", new Rectangle(5, 55, 315, 180), spriteBatch);
            fdrawer.DrawText("font", "be wary though, if you don't sometimes sacrifice villagers to the god, he will get angry. you wouldn't want him to get angry, would you?", new Rectangle(5, 110, 315, 20), spriteBatch);
        }

        void DrawMeter(int length_, Vector2 pos_)
        {
            for (int i = 0; i < length_; i++)
            {
                meterTex.Draw(spriteBatch, new Vector2(pos_.X + i, pos_.Y));
            }
        }
    }
}