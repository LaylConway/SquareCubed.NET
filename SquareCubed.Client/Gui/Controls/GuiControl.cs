﻿using System;
using System.Diagnostics;
using System.Drawing;
using SquareCubed.Common.Utils;

namespace SquareCubed.Client.Gui.Controls
{
	using ParentLink = ParentLink<GuiControl.GuiParentControl, GuiControl>;

	public abstract class GuiControl : IDisposable
	{
		private readonly ParentLink _parent;
		private bool _isFocused;

		protected GuiControl()
		{
			_parent = new ParentLink(this, p => p.Controls);
		}

		public bool IsHovered { get; private set; }
		public bool IsHeld { get; private set; }
		
		public bool IsFocused
		{
			get { return _isFocused; }
			set
			{
				if (value == _isFocused) return;

				if (Parent != null)
					Parent.FocusedChild = value ? this : null;
			}
		}

		public Point Position { get; set; }
		public abstract Size Size { get; set; }

		public Rectangle BoundingBox
		{
			get { return new Rectangle(Position, Size); }
		}

		public GuiParentControl Parent
		{
			get { return _parent.Property; }
			set { _parent.Property = value; }
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool managed)
		{
		}

		~GuiControl()
		{
			Dispose(false);
		}

		public event EventHandler Click = (s, e) => { };

		internal void HandleMouseMove(MouseMoveData data)
		{
			if (BoundingBox.Contains(data.Position))
			{
				if (!IsHovered)
					OnMouseEnter(data);
				OnMouseMove(data);
			}
			else if (BoundingBox.Contains(data.PreviousPosition))
			{
				OnMouseExit(data);
				OnMouseMove(data);
			}
		}

		internal void HandleMouseDown(MousePressData data)
		{
			if (BoundingBox.Contains(data.Position))
				OnMouseDown(data);
		}

		internal void HandleMouseUp(MousePressData data)
		{
			if (BoundingBox.Contains(data.Position))
				OnMouseUp(data);
		}

		protected virtual void OnMouseMove(MouseMoveData data)
		{
		}

		protected virtual void OnMouseEnter(MouseMoveData data)
		{
			IsHovered = true;
		}

		protected virtual void OnMouseExit(MouseMoveData data)
		{
			IsHovered = false;
		}

		protected virtual void OnMouseDown(MousePressData data)
		{
			Debug.Assert(data != null);

			IsHeld = true;
			data.EndEvent.Event += (s, e) => IsHeld = false;
		}

		protected virtual void OnMouseUp(MousePressData data)
		{
			if (IsHeld)
				OnMouseClick(data);
		}

		protected virtual void OnMouseClick(MousePressData data)
		{
			IsFocused = true;
			Click(this, EventArgs.Empty);
		}

		internal abstract void Render(float delta);

		public abstract class GuiParentControl : GuiControl
		{
			private GuiControl _focusedChild;

			protected GuiParentControl()
			{
				Controls = new ParentLink.ChildrenCollection(this, c => c._parent);
			}

			public ParentLink.ChildrenCollection Controls { get; private set; }
			public abstract Size InternalOffset { get; }
			public GuiControl FocusedChild
			{
				get { return _focusedChild; }
				set
				{
					if (value == _focusedChild) return;

					if (_focusedChild != null)
						_focusedChild._isFocused = false;
					if (value != null)
						value._isFocused = true;

					_focusedChild = value;
				}
			}

			internal override void Render(float delta)
			{
				foreach (var control in Controls)
				{
					control.Render(delta);
				}
			}

			protected override void OnMouseMove(MouseMoveData data)
			{
				var internalData = new MouseMoveData(
					new Point(
						data.Position.X - Position.X - InternalOffset.Width,
						data.Position.Y - Position.Y - InternalOffset.Height),
					new Point(
						data.PreviousPosition.X - Position.X - InternalOffset.Width,
						data.PreviousPosition.Y - Position.Y - InternalOffset.Height));

				foreach (var control in Controls)
					control.HandleMouseMove(internalData);

				base.OnMouseMove(data);
			}

			protected override void OnMouseDown(MousePressData data)
			{
				var internalData = new MousePressData(
					new Point(
						data.Position.X - Position.X - InternalOffset.Width,
						data.Position.Y - Position.Y - InternalOffset.Height),
					data.Button, data.EndEvent);

				foreach (var control in Controls)
					control.HandleMouseDown(internalData);

				base.OnMouseDown(data);
			}

			protected override void OnMouseUp(MousePressData data)
			{
				var internalData = new MousePressData(
					new Point(
						data.Position.X - Position.X - InternalOffset.Width,
						data.Position.Y - Position.Y - InternalOffset.Height),
					data.Button, data.EndEvent);

				foreach (var control in Controls)
					control.HandleMouseUp(internalData);

				base.OnMouseUp(data);
			}

			protected override void Dispose(bool managed)
			{
				if (managed)
				{
					foreach (var control in Controls)
					{
						control.Dispose();
					}
				}

				base.Dispose(managed);
			}
		}
	}
}