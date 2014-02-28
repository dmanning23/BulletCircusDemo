using BasicPrimitiveBuddy;
using BulletFlockBuddy;
using BulletMLLib;
using FlockBuddy;
using FontBuddyLib;
using FrameRateCounter;
using GameTimer;
using HadoukInput;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RandomExtensions;
using ResolutionBuddy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vector2Extensions;

namespace BulletFlockDemo
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	class Game1 : Microsoft.Xna.Framework.Game
	{
		#region Members

		static public GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D texture;

		List<Mover> playerShip;

		BulletBoidManager _boidManager;
		SimpleBulletManager _simpleManager;

		GameClock _clock;

		InputState _inputState;
		InputWrapper _inputWrapper;

		float _Rank = 0.0f;

		private FontBuddy _text = new FontBuddy();

		/// <summary>
		/// A list of all the bulletml samples we have loaded
		/// </summary>
		private List<BulletPattern> _myPatterns = new List<BulletPattern>();

		/// <summary>
		/// The names of all the bulletml patterns that are loaded, stored so we can display what is being fired
		/// </summary>
		private List<string> _patternNames = new List<string>();

		/// <summary>
		/// The current Bullet ML pattern to use to shoot bullets
		/// </summary>
		private int _CurrentPattern = 0;

		XNABasicPrimitive prim;

		List<BaseEntity> Obstacles { get; set; }

		Random g_Random = new Random();

		#endregion //Members

		#region Methods

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
			Resolution.Init(ref graphics);
			Content.RootDirectory = "Content";
			Resolution.SetDesiredResolution(1280, 720);
			Resolution.SetScreenResolution(1280, 720, false);

			Myship dude = new Myship();
			playerShip = new List<Mover>();
			playerShip.Add(dude);

			_clock = new GameClock();
			_inputState = new InputState();
			_inputWrapper = new InputWrapper(new ControllerWrapper(PlayerIndex.One, true), _clock.GetCurrentTime);
			_inputWrapper.Controller.UseKeyboard = true;

			_boidManager = new BulletBoidManager(dude.MyPos);
			_boidManager.StartPosition = new Vector2(Resolution.ScreenArea.Width / 2, Resolution.ScreenArea.Height / 2);
			_boidManager.SetWorldSize(new Vector2(Resolution.ScreenArea.Width, Resolution.ScreenArea.Height), true, false, 5, 4);
			_boidManager.Targets = playerShip;

			_simpleManager = new SimpleBulletManager(dude.MyPos);
			_simpleManager.StartPosition = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);

			Obstacles = new List<BaseEntity>();
			_boidManager.Obstacles = Obstacles;

			//add an fps counter
			FPSCounter fps = new FPSCounter(this);
			this.Components.Add(fps);
		}

		protected override void Initialize()
		{
			_clock.Start();

			base.Initialize();
		}

		public float GetRank() { return _Rank; }

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GraphicsDevice);
			prim = new XNABasicPrimitive(GraphicsDevice, spriteBatch);

			_text.LoadContent(Content, "ArialBlack14");

			texture = Content.Load<Texture2D>("Sprites\\bullet");

			//Get all the xml files in the Content\\Samples directory
			foreach (var source in Directory.GetFiles("Content\\Samples", "*.xml"))
			{
				//store the name
				_patternNames.Add(source);

				//load the pattern
				BulletPattern pattern = new BulletPattern();
				pattern.ParseXML(source);
				_myPatterns.Add(pattern);
			}

			GameManager.GameDifficulty = this.GetRank;

			AddBullet();
		}

		protected override void UnloadContent()
		{
		}

		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				this.Exit();
			}

			//update the timer
			_clock.Update(gameTime);

			//update the input
			_inputState.Update();
			_inputWrapper.Update(_inputState, false);

			//check input to increment/decrement the current bullet pattern
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.LShoulder))
			{
				//decrement the pattern
				if (0 >= _CurrentPattern)
				{
					//if it is at the beginning, move to the end
					_CurrentPattern = _myPatterns.Count - 1;
				}
				else
				{
					_CurrentPattern--;
				}

				AddBullet();
			}
			else if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.RShoulder))
			{
				//increment the pattern
				if ((_myPatterns.Count - 1) <= _CurrentPattern)
				{
					//if it is at the beginning, move to the end
					_CurrentPattern = 0;
				}
				else
				{
					_CurrentPattern++;
				}

				AddBullet();
			}

			//reset the bullet pattern
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.A))
			{
				AddBullet();
			}

			//increase/decrease the rank
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.LShoulder))
			{
				if (_Rank > 0.0f)
				{
					_Rank -= 0.1f;
				}

				if (_Rank < 0.0f)
				{
					_Rank = 0.0f;
				}
			}
			else if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.RShoulder))
			{
				if (_Rank < 1.0f)
				{
					_Rank += 0.1f;
				}

				if (_Rank > 1.0f)
				{
					_Rank = 1.0f;
				}
			}

			//if y is held down, do some slowdown
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.Y))
			{
				_boidManager.TimeSpeed = 0.5f;
				_simpleManager.TimeSpeed = 0.5f;
			}
			else
			{
				_boidManager.TimeSpeed = 1.0f;
				_simpleManager.TimeSpeed = 1.0f;
			}

			//if b is held down, make it bigger
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.LTrigger))
			{
				_boidManager.Scale -= 0.1f;
				_simpleManager.Scale -= 0.1f;
			}
			else if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.RTrigger))
			{
				_boidManager.Scale += 0.1f;
				_simpleManager.Scale += 0.1f;
			}

			//add an obstacle
			if (_inputWrapper.Controller.CheckKeystroke(EKeystroke.B))
			{
				Vector2 pos = g_Random.NextVector2(100.0f, 900.0f, 100.0f, 600.0f);
				float radius = g_Random.NextFloat(150.0f, 250.0f);
				AddObstacle(pos, radius);
			}

			_boidManager.Update(gameTime);
			_simpleManager.Update();

			foreach (var dude in playerShip)
			{
				dude.Update(_clock);
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

#if WINDOWS
			// Calculate Proper Viewport according to Aspect Ratio
			Resolution.ResetViewport();
#endif

			spriteBatch.Begin(SpriteSortMode.Deferred,
			BlendState.AlphaBlend,
			null, null, null, null,
			Resolution.TransformationMatrix());

			if (_boidManager.UseCellSpace)
			{
				_boidManager.DrawCells(prim);
			}

			Vector2 position = Vector2.Zero;

			//say what pattern we are shooting
			_text.Write(_patternNames[_CurrentPattern], position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.MeasureString("test").Y;

			//how many bullets on the screen
			_text.Write(_boidManager.Bullets.Count.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.MeasureString("test").Y;

			//the current rank
			StringBuilder rankText = new StringBuilder();
			rankText.Append("Rank: ");
			rankText.Append(((int)(_Rank * 10)).ToString());
			_text.Write(rankText.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.MeasureString("test").Y;

			//the current time speed
			rankText = new StringBuilder();
			rankText.Append("Time Speed: ");
			rankText.Append(_boidManager.TimeSpeed.ToString());
			_text.Write(rankText.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.MeasureString("test").Y;

			//the current scale
			rankText = new StringBuilder();
			rankText.Append("Scale: ");
			rankText.Append(_boidManager.Scale.ToString());
			_text.Write(rankText.ToString(), position, Justify.Left, 1.0f, Color.White, spriteBatch);
			position.Y += _text.Font.MeasureString("test").Y;

			foreach (var boid in _boidManager.Bullets)
			{
				boid.Render(prim, Color.Green);
			}

			foreach (var boid in _simpleManager.Bullets)
			{
				boid.Render(prim, Color.Red);
			}

			foreach (var dude in Obstacles)
			{
				dude.DrawPhysics(prim, Color.White);
			}

			foreach (var dude in playerShip)
			{
				dude.Render(prim, Color.Black);
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}

		private void AddBullet()
		{
			//clear out all the bulelts
			_boidManager.Clear();
			_simpleManager.Clear();

			//add a new bullet in the center of the screen
			var boid = _boidManager.CreateBullet();
			boid.InitTopNode(_myPatterns[_CurrentPattern].RootNode);

			//TODO: comment this back in if you want to see the default BulletMLLib behavior
			//var simple = _simpleManager.CreateBullet();
			//simple.InitTopNode(_myPatterns[_CurrentPattern].RootNode);
		}

		public void AddObstacle(Vector2 pos, float radius)
		{
			var obs = new BaseEntity(pos, radius);
			Obstacles.Add(obs);
		}

		#endregion //Methods
	}
}
