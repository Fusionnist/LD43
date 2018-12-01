using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
using System.Threading;using System.Collections.Generic;
using System.Xml.Linq;


namespace LD43
{
    enum GameState { Game, Menu }
    enum GameSubState { Game, Main, End, Pause, Tutorial }

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

        TextureDrawer cursorTex, currentBG;
        TextureDrawer gameBG, mainMenuBG, endBG, tutorialBG, pauseBG;

        FontDrawer fdrawer;

        //Data
        Point wDims, vDims, mousePos;
        GameState currentState, nextState;
        GameSubState currentSubState, nextSubState;
        bool switchedState, changeCursorText;
        string cursorText;
        
        public Game1()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            wDims = new Point(1920 / 3 * 2, 1080 / 3 * 2);
            vDims = new Point(320, 180);

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

            PhysicsManager.CreateWorld();
            PhysicsManager.SetUnitRatio(64);
            PhysicsManager.SetupDebugview(GraphicsDevice, Content);

            cursor = new CursorManager();

            fdrawer = new FontDrawer();
            List<TextureDrawer> font = new List<TextureDrawer>();
            string junk = "abcdefghijklmnopqrstuvwxyz.,!?'";
            Texture2D tex = Content.Load<Texture2D>("Placeholder/shittyfont");
            for (int i = 0; i < 31; i++)
            {
                font.Add(new TextureDrawer(tex, new TextureFrame(new Rectangle(6 * i, 0, 6, 6), new Point(0, 0)), null, junk[i].ToString(), null ,null));
            }
            fdrawer.fonts.Add(new DrawerCollection(font, "font"));
            cursorText = "ass";

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
        void CreateNewGame()
        {

        } //create whatever's needed to start a new game

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            ElementCollection.ReadDocument(XDocument.Load("Content/Xml/Registry.xml"));
            SpriteSheetCollection.LoadFromElementCollection(Content);

            cursorTex = SpriteSheetCollection.GetTex("static", "PlaceholderSheet", "cursor");

            LoadBGs();
            CreateUI();
        }
        void CreateUI()
        {
            //NOTE: CREATE BUTTON VARIANTS
            mainUI = new UISystem(new List<Button>()
            {
                new Button("startGame", new Rectangle(100,50,32,16), 
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","start"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","start"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","start")
                ),
                new Button("tutorial", new Rectangle(100,100,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","tutorial"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","tutorial"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","tutorial")
                )
            }
            );

            endgameUI = new UISystem(new List<Button>()
            {
                new Button("mainMenu", new Rectangle(100,0,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","menu"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","menu"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","menu")
                ),
                new Button("restartGame", new Rectangle(100,32,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","restart"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","restart"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","restart")
                ),
                  new Button("exitGame", new Rectangle(100,64,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","quit"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","quit"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","quit")
                ),
            }
            );

            tutorialUI = new UISystem(new List<Button>()
            {
                new Button("startGame", new Rectangle(100,100,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","start"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","start"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","start")
                ),
                 new Button("mainMenu", new Rectangle(100,0,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","menu"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","menu"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","menu")
                )
            }
           );

            pauseUI = new UISystem(new List<Button>()
            {
                new Button("resumeGame", new Rectangle(100,0,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","resume"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","resume"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","resume")
                ),
                 new Button("endGame", new Rectangle(100,32,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","button"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","button"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","button")
                ),
                  new Button("exitGame", new Rectangle(100,64,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","giveup"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","giveup"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","giveup")
                ),
                   new Button("mainMenu", new Rectangle(100,96,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","menu"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","menu"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","menu")
                ),
                 new Button("restartGame", new Rectangle(100,128,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","restart"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","restart"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","restart")
                )
            }
           );

            gameUI = new UISystem(new List<Button>()
            {
                new Button("pauseGame", new Rectangle(100,0,32,16),
                SpriteSheetCollection.GetTex("static","PlaceholderSheet","pause"),
                SpriteSheetCollection.GetTex("pressed","PlaceholderSheet","pause"),
                SpriteSheetCollection.GetTex("hovered","PlaceholderSheet","pause")
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

        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            UpdateState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            ipp.Update(Keyboard.GetState(), GamePad.GetState(0));
            cursor.Update();
            float es = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateSwitch(es);
            ReadCommandQueue();
            base.Update(gameTime);
        }
        void ReadCommandQueue()
        {
            if (currentUI.IssuedCommand("startGame"))
            {
                ToggleState(GameState.Game);
                ToggleSubState(GameSubState.Game);

                currentUI = gameUI;
                currentBG = gameBG;
            }

            if (currentUI.IssuedCommand("endGame"))
            {
                ToggleState(GameState.Menu);
                ToggleSubState(GameSubState.End);

                currentUI = endgameUI;
                currentBG = endBG;
            }

            if (currentUI.IssuedCommand("restartGame"))
            {
                ToggleState(GameState.Game);
                ToggleSubState(GameSubState.Game);

                currentUI = gameUI;
                currentBG = gameBG;
            }

            if (currentUI.IssuedCommand("mainMenu"))
            {
                ToggleState(GameState.Menu);
                ToggleSubState(GameSubState.Main);

                currentUI = mainUI;
                currentBG = mainMenuBG;
            }

            if (currentUI.IssuedCommand("pauseGame"))
            {
                ToggleState(GameState.Game);
                ToggleSubState(GameSubState.Pause);

                currentUI = pauseUI;
                currentBG = pauseBG;
            }

            if (currentUI.IssuedCommand("tutorial"))
            {
                ToggleState(GameState.Menu);
                ToggleSubState(GameSubState.Tutorial);

                currentUI = tutorialUI;
                currentBG = tutorialBG;
            }

            if (currentUI.IssuedCommand("exitGame"))
            {
                Exit();
            }
        }
        void UpdateState()
        {
            if (switchedState)
            {
                currentState = nextState;
                currentSubState = nextSubState;
            }
        } //set the next state and substate to the current one at the start of the update
        void UpdateSwitch(float es_)
        {
            switch (currentState)
            {
                case GameState.Game:
                    switch (currentSubState)
                    {
                        case GameSubState.Game: //GAME-GAME
                            UpdateMenu(es_);
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

        } //update the movey things
        void UpdateMenu(float es_)
        {
            currentUI.UpdateWithMouse(es_, cursor, scenes.CurrentScene.ToVirtualPos(cursor.RawPos()));

            if (currentUI.HoveredButton != null)
                HandleButtonTooltips();
            else
                changeCursorText = true;
        } //update the clicky things

        void HandleButtonTooltips()
        {
            switch(currentUI.HoveredButton.Command)
            {
                default:
                    cursorText = "this is a button";
                    break;
            }
        }
        void ToggleState(GameState newState_)
        {
            nextState = newState_;
            switchedState = true;
        } //set new nextstate and data based on old and new state
        void ToggleSubState(GameSubState newSubState_)
        {
            nextSubState = newSubState_;
            switchedState = true;
        } //set new nextsubstate and data based on old and new sub state

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
                            scenes.DrawScene(spriteBatch, "menu");
                            scenes.DrawScene(spriteBatch, "game");
                            break;

                        case GameSubState.Pause:
                            scenes.DrawScene(spriteBatch, "menu");
                            scenes.DrawScene(spriteBatch, "game");
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
            spriteBatch.End();
        } //draw to game scene
        void DrawMenu() 
        {
            scenes.SelectScene("menu");
            scenes.SetupScene(spriteBatch, GraphicsDevice);

            //DRAW
            fdrawer.DrawText("font", cursorText, new Rectangle(new Point(0, 0), new Point(40, 180)), spriteBatch);
            currentUI.Draw(spriteBatch);

            spriteBatch.End();
        } //draw to menu scene
        void DrawOverlay() 
        {
            scenes.SelectScene("overlay");
            scenes.SetupScene(spriteBatch, GraphicsDevice);

            //DRAW

            spriteBatch.End();
        } //draw to overlay scene
        void DrawPhysicsDebug()
        {
            spriteBatch.Begin();

            Matrix projection = Matrix.CreateScale(1f) * Matrix.CreateOrthographicOffCenter(0, vDims.X / 64f, vDims.Y / 64f, 0, 0, 1) * Matrix.CreateTranslation(new Vector3(0, 0, 0));

            PhysicsManager.DrawDebugview(projection);

            spriteBatch.End();
        } //DEBUG
    }
}