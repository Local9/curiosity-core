﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace NativeUI
{
	public class UIMenuHorizontalOneLineGridPanel : UIMenuPanel
	{
		private UIResText Left;
		private UIResText Right;
		private Sprite Grid;
		private Sprite Circle;
		private UIMenuGridAudio Audio;
		private PointF SetCirclePosition;
		protected bool CircleLocked;
		protected bool Pressed;
		private readonly PointF safezoneOffset = ScreenTools.SafezoneBounds;
		public PointF CirclePosition
		{
			get
			{
				return new PointF((float)Math.Round((Circle.Position.X - (Grid.Position.X + 20) + (Circle.Size.Width / 2)) / (Grid.Size.Width - 40), 2), (float)Math.Round((Circle.Position.Y - (Grid.Position.Y + 20) + (Circle.Size.Height / 2)) / (Grid.Size.Height - 40), 2));
			}
			set
			{
				Circle.Position.X = (Grid.Position.X + 20) + ((Grid.Size.Width - 40) * (value.X >= 0f && value.X <= 1.0f ? value.X : 0.0f)) - (Circle.Size.Width / 2);
				Circle.Position.Y = (Grid.Position.Y + 20) + ((Grid.Size.Height - 40) * (value.Y >= 0f && value.Y <= 1.0f ? value.Y : 0.0f)) - (Circle.Size.Height / 2);
			}
		}

		public UIMenuHorizontalOneLineGridPanel(string LeftText, string RightText, float CirclePositionX = 0.5f)
		{
			Enabled = true;
			Background = new Sprite("commonmenu", "gradient_bgd", new Point(0, 0), new Size(431, 275));
			Grid = new Sprite("NativeUI", "horizontal_grid", new Point(0, 0), new Size(200, 200), 0f, Color.FromArgb(255, 255, 255));
			Circle = new Sprite("mpinventory", "in_world_circle", new Point(0, 0), new Size(20, 20), 0f, Color.FromArgb(255, 255, 255));
			Audio = new UIMenuGridAudio("CONTINUOUS_SLIDER", "HUD_FRONTEND_DEFAULT_SOUNDSET", 0);
			Left = new UIResText(LeftText ?? "Left", new Point(0, 0), .35f, Color.FromArgb(255, 255, 255), CitizenFX.Core.UI.Font.ChaletLondon, Alignment.Center);
			Right = new UIResText(RightText ?? "Right", new Point(0, 0), .35f, Color.FromArgb(255, 255, 255), CitizenFX.Core.UI.Font.ChaletLondon, Alignment.Center);
			SetCirclePosition = new PointF(CirclePositionX, 0.5f);
		}

		internal override void Position(float y)
		{
			float Y = y;
			float ParentOffsetX = ParentItem.Offset.X;
			int ParentOffsetWidth = ParentItem.Parent.WidthOffset;
			Background.Position = new PointF(ParentOffsetX, Y);
			Grid.Position = new PointF(ParentOffsetX + 115.5f + (ParentOffsetWidth / 2), 72.5f + y);
			Left.Position = new PointF(ParentOffsetX + 57.75f + (ParentOffsetWidth / 2), 155f + y);
			Right.Position = new PointF(ParentOffsetX + 373.25f + (ParentOffsetWidth / 2), 155f + y);
			if (!CircleLocked)
			{
				CircleLocked = true;
				CirclePosition = SetCirclePosition;
			}
		}

		private void UpdateParent(float X)
		{
			ParentItem.Parent.ListChange(ParentItem, ParentItem.Index);
			ParentItem.ListChangedTrigger(ParentItem.Index);
		}

		private async void Functions()
		{
			if (ScreenTools.IsMouseInBounds(new PointF(Grid.Position.X + 20f + safezoneOffset.X, Grid.Position.Y + 20f + safezoneOffset.Y), new SizeF(Grid.Size.Width - 40f, Grid.Size.Height - 40f)))
			{
				if (API.IsDisabledControlPressed(0, 24))
				{
					if (!Pressed)
					{
						Pressed = true;
						Audio.Id = API.GetSoundId();
						API.PlaySoundFrontend(Audio.Id, Audio.Slider, Audio.Library, true);
					}
					float mouseX = API.GetDisabledControlNormal(0, 239) * Resolution.Width;
					mouseX -= (Circle.Size.Width / 2) + safezoneOffset.X;
					Circle.Position = new PointF(mouseX > (Grid.Position.X + 10 + Grid.Size.Width - 40) ? (Grid.Position.X + 10 + Grid.Size.Width - 40) : ((mouseX < (Grid.Position.X + 20 - (Circle.Size.Width / 2))) ? (Grid.Position.X + 20 - (Circle.Size.Width / 2)) : mouseX), Circle.Position.Y);
					var resultX = (float)Math.Round((Circle.Position.X - (Grid.Position.X + 20) + (Circle.Size.Width + 20)) / (Grid.Size.Width - 40), 2) + safezoneOffset.X;
					UpdateParent(((resultX >= 0.0f && resultX <= 1.0f) ? resultX : ((resultX <= 0f) ? 0.0f : 1.0f) * 2f) - 1f);
				}
				if (API.IsDisabledControlJustReleased(0, 24))
				{
					API.StopSound(Audio.Id);
					API.ReleaseSoundId(Audio.Id);
					Pressed = false;
				}
			}
			else
			{
				API.StopSound(Audio.Id);
				API.ReleaseSoundId(Audio.Id);
				Pressed = false;
			}
		}

		internal async override Task Draw()
		{
			if (!Enabled) return;
			Background.Size = new Size(431 + ParentItem.Parent.WidthOffset, 275);
			Background.Draw();
			Grid.Draw();
			Circle.Draw();
			Left.Draw();
			Right.Draw();
			Functions();
			await Task.FromResult(0);
		}
	}

}
