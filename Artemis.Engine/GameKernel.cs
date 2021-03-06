﻿#region Using Statements

using Artemis.Engine.Assets;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

#endregion

namespace Artemis.Engine
{
    /// <summary>
    /// A subclass of Monogame's Game object from which the Updating and Rendering
    /// actually stems. We need to explicitly create an instance of this in ArtemisEngine.
    /// </summary>
    internal class GameKernel : Game
    {

        /// <summary>
        /// A reference to the main engine instance. We need this so
        /// that we can initialize it's RenderPipeline from Initialize.
        /// </summary>
        ArtemisEngine engine;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private int _frameRate;
        public int FrameRate
        {
            get
            {
                return _frameRate;
            }
            internal set
            {
                _frameRate = value;
                TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)value);
            }
        }

        public GameKernel(ArtemisEngine engine, string contentFolder, bool fixedTimeStep, int frameRate)
            : base()
        {
            this.engine = engine;

            IsFixedTimeStep = fixedTimeStep;
            FrameRate = frameRate;

            graphics = new GraphicsDeviceManager(this);

            Content.RootDirectory = contentFolder;
            AssetLoader.Initialize(Content);
        }

        sealed protected override void Initialize()
        {
            base.Initialize();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Due to Monogame's stupidity, we have to create the spriteBatch here, and not in
            // the most logical location (GameKernel's constructor OR even better, WE shouldn't
            // have to create it at all), we have to initialize the render pipeline from here.
            
            engine.InitializeRenderPipeline(spriteBatch, GraphicsDevice, graphics);
        }

        sealed protected override void LoadContent() { base.LoadContent(); }
        sealed protected override void UnloadContent() { base.UnloadContent(); }

        sealed protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            #region Angry Rant About Monogame

            // 4/23/2016: This is all just for fun. In reality I love you guys at Monogame, and appreciate
            // your hard, though sometimes questionable, work!

            // WRITTEN: 11/15/2015
            //
            // Alright, story time. Monogame's Game class is structured so that Initialize
            // has to get called before LoadContent, and LoadContent has to be called before Update.
            // Initialize must be called before the GraphicsDevice or the SpriteBatch can be used,
            // and LoadContent must be called before ContentManager can be used.
            //
            // But here's the deal. You can't just call Initialize and LoadContent, oh no. Monogame's
            // brilliant structure makes it so that calling either Initialize or LoadContent on your
            // own throws an arbitrary NullReferenceException or ArgumentNullException (when trying
            // to use the ContentManager), for completely irrelevant things. If you want Initialize
            // and LoadContent to be called, you have to call Game.Run().
            //
            // That's right. Game.Run() calls Initialize and LoadContent, before of course doing the stupid
            // internal things necessary to keep those pesky NullReferenceExceptions or ArgumentNullExceptions
            // from happening. Sure, the code that prevents those exceptions could've just been placed in
            // Game's constructor, but who cares right? It's not like it's a big deal (it's totally a big deal).
            //
            // So if you want to initialize your game, you have to run your game. It ALMOST sounds like it
            // makes sense, but really, you should be initializing everything before your game gets run at
            // all. Due to Monogame's just outstanding structure, we have to initialize the game in the
            // first frame of the game (i.e. the first time we call Update).
            //
            // To give an analogy, this is like if you had to start your car by standing outside your car,
            // turning it on, and then getting in and buckling up as it's moving. Sounds stupid and unnecessary
            // and completely illogical and completely avoidable? It is, and so is this, but Monogame decided
            // to take the moronic root.
            //
            // Honestly, the code looks like it was made by Larry the intern in version v0.0.1 alpha,
            // which he was then subsequently fired for writing but no one had the motivation to go
            // in and actually edit the code to make it reasonable. They just left it like that, presumably
            // to make time for their Content Pipeline (good use of your time guys).
            //
            // If you're wondering what could make the system better, well for starters why not take all
            // the initialization code that happens in Run and put it in the game's constructor? Wow! Doesn't
            // that make a lot of sense? I wonder how I came up with that! Oh I know! I'm not an idiot.
            //
            // Even if you DIDN'T want to put all that code in the constructor, you could at least make
            // each method more complete so that they don't throw NullReferenceExceptions when not called
            // at the right time in the delicately balanced system that is the Game class. Like, seriously.
            // It's not that hard to do, I could literally just copy and paste the code from Run into
            // Initialize and it would work fine. I mean, that's like the FIRST principle you learn in
            // OOP, it's the first letter in SOLID. "Single Responsibility", isn't it Initialize's
            // responsibility to initialize the game, and it shouldn't be the responsibility of Run to
            // prevent Initialize from throwing NullReferenceExceptions? That's like the first thing
            // they teach you, how could you forget that?
            //
            // So that's the reason why we have to initialize the game in the first frame of Update.
            // I can think of about 30,000 words to describe how stupid Monogame is for being this way,
            // but I won't list them because I have better things to do with my time, like for example,
            // oh I don't know, making game architectures that work and make sense.
            //
            // Fuck you monogame.
            //
            // Fuck you.

            #endregion

            if (!engine.Initialized)
            {
                engine.Initialize();
            }

            engine.Update(gameTime);
        }

        sealed protected override void Draw(GameTime gameTime)
        {
            // 11/15/2015: At least everything's okay here.

            base.Draw(gameTime);

            engine.Render();
        }

    }
}
