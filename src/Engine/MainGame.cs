﻿using KeyboardCommander.Engine.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KeyboardCommander.Engine
{
    public class MainGame : Game
    {
        private BaseGameState _currentGameState;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private RenderTarget2D _renderTarget;
        private Rectangle _renderScaleRectangle;

        private int _DesignedResolutionWidth;
        private int _DesignedResolutionHeight;
        private float _DesignedResolutionAspectRatio;

        private BaseGameState _FirstGameState;

        public MainGame(int width, int height, BaseGameState firstGameState)
        {
            Content.RootDirectory = "Content";
            _graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = width,
                PreferredBackBufferHeight = height,
                IsFullScreen = false,
            };

            IsMouseVisible = true;
            _FirstGameState = firstGameState;

            _DesignedResolutionWidth = width;
            _DesignedResolutionHeight = height;
            _DesignedResolutionAspectRatio = width / (float)height;

            //IsFixedTimeStep = false;
            //TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / 50);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = _DesignedResolutionWidth;
            _graphics.PreferredBackBufferHeight = _DesignedResolutionHeight;
            _graphics.IsFullScreen = false;

            //_graphics.SynchronizeWithVerticalRetrace = true;

            _graphics.ApplyChanges();

            _renderTarget = new RenderTarget2D(_graphics.GraphicsDevice, _DesignedResolutionWidth,
                _DesignedResolutionHeight, false, SurfaceFormat.Color, DepthFormat.None, 0,
                RenderTargetUsage.DiscardContents);

            _renderScaleRectangle = GetScaleRectangle();

            base.Initialize();
        }

        /// <summary>
        /// Uses the current window size compared to the design resolution
        /// </summary>
        /// <returns>Scaled Rectangle</returns>
        private Rectangle GetScaleRectangle()
        {
            var variance = 0.5;
            var actualAspectRatio = Window.ClientBounds.Width / (float)Window.ClientBounds.Height;

            Rectangle scaleRectangle;

            if (actualAspectRatio <= _DesignedResolutionAspectRatio)
            {
                var presentHeight = (int)(Window.ClientBounds.Width / _DesignedResolutionAspectRatio + variance);
                var barHeight = (Window.ClientBounds.Height - presentHeight) / 2;

                scaleRectangle = new Rectangle(0, barHeight, Window.ClientBounds.Width, presentHeight);
            }
            else
            {
                var presentWidth = (int)(Window.ClientBounds.Height * _DesignedResolutionAspectRatio + variance);
                var barWidth = (Window.ClientBounds.Width - presentWidth) / 2;

                scaleRectangle = new Rectangle(barWidth, 0, presentWidth, Window.ClientBounds.Height);
            }

            return scaleRectangle;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            SwitchGameState(_FirstGameState);
        }

        private void CurrentGameState_OnStateSwitched(object sender, BaseGameState gameState)
        {
            SwitchGameState(gameState);
        }

        private void SwitchGameState(BaseGameState gameState)
        {
            if (_currentGameState != null)
            {
                _currentGameState.OnStateSwitched -= CurrentGameState_OnStateSwitched;
                _currentGameState.OnEventNotification -= _currentGameState_OnEventNotification;
                _currentGameState.UnloadContent();
            }

            _currentGameState = gameState;

            _currentGameState.Initialize(Content, _graphics.GraphicsDevice.Viewport.Width, _graphics.GraphicsDevice.Viewport.Height);

            _currentGameState.LoadContent();

            _currentGameState.OnStateSwitched += CurrentGameState_OnStateSwitched;
            _currentGameState.OnEventNotification += _currentGameState_OnEventNotification;
        }

        private void _currentGameState_OnEventNotification(object sender, BaseGameStateEvent e)
        {
            switch (e)
            {
                case BaseGameStateEvent.GameQuit _:
                    Exit();
                    break;
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            _currentGameState?.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            _currentGameState.HandleInput(gameTime);
            _currentGameState.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //_graphics.ApplyChanges();

            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _currentGameState.Render(_spriteBatch);

            _spriteBatch.End();

            // Now render the scaled content
            _graphics.GraphicsDevice.SetRenderTarget(null);

            _graphics.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);

            _spriteBatch.Draw(_renderTarget, _renderScaleRectangle, Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
