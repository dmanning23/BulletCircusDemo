using Microsoft.Xna.Framework;
using GameTimer;
using Microsoft.Xna.Framework.Input;
using FlockBuddy;

namespace BulletFlockDemo
{
	class Myship : Mover
	{
		const float speed = 3;

		public Myship()
			: base(new Vector2(Game1.graphics.PreferredBackBufferWidth / 2, Game1.graphics.PreferredBackBufferHeight / 2),
				10.0f, Vector2.UnitY, 0.0f, 1.0f, 100.0f, 2.0f, 100.0f)
		{
		}

		public Vector2 MyPos()
		{
			return Position;
		}

		public override void Update(GameClock time)
		{
			base.Update(time);

			Vector2 pos = Position;

			if (Keyboard.GetState().IsKeyDown(Keys.Left))
			{
				pos.X -= speed;
				Speed = speed * 60.0f;
				Heading = new Vector2(-1.0f, 0.0f);
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Right))
			{
				pos.X += speed;
				Speed = speed * 60.0f;
				Heading = new Vector2(1.0f, 0.0f);
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Up))
			{
				pos.Y -= speed;
				Speed = speed * 60.0f;
				Heading = new Vector2(0.0f, -1.0f);
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Down))
			{
				pos.Y += speed;
				Speed = speed * 60.0f;
				Heading = new Vector2(0.0f, 1.0f);
			}
			else
			{
				Speed = 0.0f;
			}

			Position = pos;
		}
	}
}
