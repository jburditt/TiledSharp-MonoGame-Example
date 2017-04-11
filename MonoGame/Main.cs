﻿using DataStore;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Manager;
using Player;
using Player.StateStack;
using System.Collections.Generic;

namespace MonoGame
{
    public class Main : Game
    {
        // TODO Add this to references: 
        // menubg.png   http://opengameart.org/content/space-background
        // cursor.png   http://opengameart.org/content/iron-plague-pointercursor
        // icons.png    https://www.scirra.com/forum/pixel-art-rpg-icons_t66386
        // dialog       http://opengameart.org/content/10-basic-message-boxes

        // MonoGame Framework
        GraphicsDeviceManager graphicsDeviceManager;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D menu;

        // MonoGame Managers
        Graphics graphics;
        DialogManager dialogManager;
        TilesetManager tilesetManager;
        SongManager songManager;
        SoundManager soundManager;
        BattleManager battleManager;
        ActorManager actorManager;
        EnemyManager enemyManager;
        IconManager iconManager;
        InputManager inputManager;

        // Game
        StateStack stateStack = new StateStack();
        SerializableDictionary<State, IState> states = new SerializableDictionary<State, IState>();

        // DataStore
        private IDataStore _dataStore;

        public Main()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);
            graphicsDeviceManager.PreferredBackBufferWidth = Screen.Width;// GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphicsDeviceManager.PreferredBackBufferHeight = Screen.Height;// GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphicsDeviceManager.IsFullScreen = true;
            graphicsDeviceManager.ApplyChanges();

            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            inputManager = new InputManager(this);
            
            this.Components.Add(inputManager);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("Menu");

            graphics = new Graphics(Content, spriteBatch, font);
            tilesetManager = new TilesetManager(Content, spriteBatch);
            dialogManager = new DialogManager(Content, spriteBatch);
            songManager = new SongManager(Content);
            soundManager = new SoundManager(Content);
            actorManager = new ActorManager(Content, spriteBatch);
            enemyManager = new EnemyManager(Content, spriteBatch);
            iconManager = new IconManager(Content, spriteBatch);
            battleManager = new BattleManager(Content, spriteBatch);

            _dataStore = new BinaryDataStore("../../../../Data/map/");

            var worldState = new WorldState(_dataStore, songManager, graphics, battleManager, actorManager, enemyManager, iconManager, inputManager, tilesetManager, dialogManager);
            states.Add(State.World, worldState);

            var menuState = new MenuState(graphics, dialogManager, inputManager, songManager);
            states.Add(State.StartMenu, menuState);

            var battleState = new BattleState(graphics, battleManager, actorManager, enemyManager, iconManager, inputManager, songManager, dialogManager, worldState);
            states.Add(State.Battle, battleState);

            stateStack.Push(worldState);
            stateStack.Push(menuState);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            Content.Unload();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Exit
            if (stateStack.Empty())
                Exit();

            // Alt-Enter
            if (inputManager.JustPressedKey((int)Keys.F5))
            {
                graphicsDeviceManager.IsFullScreen = !graphicsDeviceManager.IsFullScreen;
                graphicsDeviceManager.ApplyChanges();
            }

            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            stateStack.Update(deltaTime);

            if (inputManager.JustPressedKey((int)Keys.B))
            {
                stateStack.Push(states[State.Battle]);
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin();

            GraphicsDevice.Clear(Color.Black);

            stateStack.Draw();

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}