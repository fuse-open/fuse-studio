using Uno;
using Uno.IO;
using Uno.UX;
using Uno.Collections;
using Fuse;
using Uno.Diagnostics;
using Fuse.Controls;
using Fuse.Elements;

namespace Outracks.Simulator.Client
{
	using Bytecode;
	using Dialogs;
	using Protocol;
	using Runtime;

	class LoadingScreen
	{
		public static void Show(FakeApp app, string header, string message)
		{
			UserAppState.Default.ApplyTo(app);
			app.Children.Add(new Outracks.Simulator.Client.Dialogs.LoadingScreen()
			{
				Header = header,
				Message = message,
			});
		}

	}

	class ModalDialog
	{
		public static void ShowPrompt(FakeApp app, string header, string body, Control inputControl, IEnumerable<Control> buttons)
		{
			var stackPanel = new StackPanel()
			{
				Alignment = Alignment.Center,
				Padding = float4(40),
				Children = 
				{
					new Text()
					{
						Margin = float4(0, 0, 0, 20),
						Value = header,
						FontSize = 40,
						TextAlignment = TextAlignment.Center,
						TextWrapping = TextWrapping.Wrap,
					},
					new Text()
					{
						Margin = float4(0, 0, 0, 20),
						Value = body,
						TextWrapping = TextWrapping.Wrap,
					},
					inputControl,
				}
			};

			var buttonPanel = new StackPanel()
			{
				Margin = float4(0, 0, 0, 20),
				Orientation = Fuse.Layouts.Orientation.Horizontal,
				Alignment = Alignment.Center,
			};

			foreach (var btn in buttons)
				buttonPanel.Children.Add(btn);

			stackPanel.Children.Add(buttonPanel);
			var root = new ClientPanel();
			root.Children.Add(stackPanel);

			UserAppState.Default.ApplyTo(Context.App);
			app.Children.Add(root);
		}

		public static void Show(FakeApp app, string header, string body, string details, IEnumerable<Control> buttons)
		{
			var dialog = new Outracks.Simulator.Client.Dialogs.ModalDialog
			{
				Header = header,
				Body = body,
				Details = details,
			};

			var buttonArray = buttons.ToArray();
			dialog.ButtonGrid.ColumnCount = buttonArray.Length;
			foreach (var btn in buttonArray)
			{
				dialog.ButtonGrid.Children.Add(btn);
			}

			UserAppState.Default.ApplyTo(Context.App);
			app.Children.Clear();
			app.Children.Add(dialog);
		}
	}

	abstract class ShowingPrompt : State
	{
		readonly string _header;
		readonly string _body;

		TextInput _input;
		bool _okClicked;
		bool _cancelClicked;

		protected ShowingPrompt(
			string header,
			string body)
		{
			_header = header;
			_body = body;
		}

		public override State OnEnterState()
		{
			var ok = new Button()
			{
				Margin = float4(20),
				Text = "Ok",
			};
			Fuse.Gestures.Clicked.AddHandler(ok, OnOkClicked);

			var cancel = new Button()
			{
				Margin = float4(20),
				Text = "Cancel",
			};
			Fuse.Gestures.Clicked.AddHandler(cancel, OnCancelClicked);

			_input = new TextBox()
			{
				
			};

			debug_log "# " + _header;
			debug_log _body;

			ModalDialog.ShowPrompt(Context.App, _header, _body, _input, new Control[] { cancel, ok});
			return this;
		}

		void OnOkClicked(object s, Fuse.Gestures.ClickedArgs args)
		{
			_okClicked = true;
		}

		void OnCancelClicked(object s, Fuse.Gestures.ClickedArgs args)
		{
			_cancelClicked = true;
		}

		protected abstract State OnOk(string text);

		protected abstract State OnCancel();

		public override State OnUpdate()
		{
			if (_cancelClicked)
				return OnCancel();

			if (_okClicked)
				return OnOk(_input.Value);

			return this;
		}
	}

	class ShowingModalDialog : State
	{
		readonly string _header;
		readonly string _body;
		readonly string _details;
		readonly DialogButton[] _buttons;

		Optional<DialogButton> _clickedButton;

		public ShowingModalDialog(
			string header,
			string body,
			string details,
			params DialogButton[] buttons)
		{
			_header = header;
			_body = body;
			_details = details;
			_buttons = buttons;
		}

		public override State OnEnterState()
		{
			var buttons = new List<Control>();
			foreach (var button in _buttons)
			{
				var node = new ModalButton()
				{
					Text = button.Text,
				};

				var action = Closure.Apply(OnButtonClicked, button);
				var handler = (Fuse.Gestures.ClickedHandler) new ForgetAction<object, Fuse.Gestures.ClickedArgs>(action).Execute;
				Fuse.Gestures.Clicked.AddHandler(node, handler);
				buttons.Add(node);
			}

			ModalDialog.Show(Context.App, _header, _body, _details, buttons);

			return this;
		}

		void OnButtonClicked(DialogButton button)
		{
			_clickedButton = Optional.Some(button);
		}

		public override State OnUpdate()
		{
			if (_clickedButton.HasValue)
				return _clickedButton.Value.Destination;

			return this;
		}

		public override State OnException(Exception e)
		{
			debug_log(e.Message);
			return this;
		}
	}

	sealed class DialogButton
	{
		public readonly string Text;
		public readonly State Destination;

		public DialogButton(string text, State destination)
		{
			Text = text;
			Destination = destination;
		}
	}
}
