using FlockBuddy;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ResolutionBuddy;

namespace BulletCircusDemo
{
	class Myship : Mover
	{
		const float speed = 3;

		public Myship()
			: base(Resolution.TitleSafeArea.Center.ToVector2(), 10.0f, Vector2.UnitY, 0f)
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
