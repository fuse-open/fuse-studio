using System;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using AppKit;

namespace Outracks.Fusion.OSX
{
	public class MessageBox
	{
		readonly NSAlert _alert;
		readonly MessageBoxButtons _buttons;
		readonly MessageBoxDefaultButton _defaultButton;

		MessageBox(
			string text,
			string caption,
			MessageBoxButtons buttons,
			MessageBoxType type,
			MessageBoxDefaultButton defaultButton)
		{
			_buttons = buttons;
			_defaultButton = defaultButton;

			_alert = new NSAlert ();
			AddButtons (_alert);
			_alert.AlertStyle = Convert (type);
			_alert.MessageText = caption ?? string.Empty;
			_alert.InformativeText = text ?? string.Empty;
		}

		public static DialogResult ShowMessageBox(string text,
			string caption,
			MessageBoxButtons buttons,
			MessageBoxType type,
			MessageBoxDefaultButton defaultButton)
		{
			return Fusion.Application.MainThread.InvokeAsync(() => new MessageBox (text, caption, buttons, type, defaultButton).Show()).Result;
		}

		DialogResult Show()
		{
			var ret = (int)_alert.RunModal ();
			switch (_buttons) 
			{
			case MessageBoxButtons.Ok:
				return DialogResult.Ok;
			case MessageBoxButtons.OkCancel:
				return (ret == 1000) ? DialogResult.Ok : DialogResult.Cancel;
			case MessageBoxButtons.YesNo:
				return (ret == 1000) ? DialogResult.Yes : DialogResult.No;
			case MessageBoxButtons.YesNoCancel:
				return (ret == 1000) ? DialogResult.Yes : (ret == 1001) ? DialogResult.No : DialogResult.Cancel;
			default:
				throw new NotSupportedException ();
			}
		}

		void AddButtons(NSAlert alert)
		{
			switch (_buttons) 
			{
			case MessageBoxButtons.Ok:
				alert.AddButton ("Ok");
				break;
			case MessageBoxButtons.OkCancel:
				{
					var ok = alert.AddButton ("Ok");
					var cancel = alert.AddButton ("Cancel");
					switch (_defaultButton) {
					case MessageBoxDefaultButton.Ok:
						ok.BecomeFirstResponder ();
						break;
					case MessageBoxDefaultButton.Cancel:
					case MessageBoxDefaultButton.Default:
						cancel.BecomeFirstResponder ();
						break;
					}
				}
				break;
			case MessageBoxButtons.YesNo:
				{
					var yes = alert.AddButton ("Yes");
					var no = alert.AddButton ("No");
					switch (_defaultButton) {
					case MessageBoxDefaultButton.Yes:
						yes.BecomeFirstResponder ();
						break;
					case MessageBoxDefaultButton.No:
					case MessageBoxDefaultButton.Default:
						no.BecomeFirstResponder ();
						break;
					}
				}
				break;
			case MessageBoxButtons.YesNoCancel:
				{
					var yes = alert.AddButton ("Yes");
					var no = alert.AddButton ("No");
					var cancel = alert.AddButton ("Cancel");
					switch (_defaultButton) {
					case MessageBoxDefaultButton.Yes:
						yes.BecomeFirstResponder ();
						break;
					case MessageBoxDefaultButton.No:
						no.BecomeFirstResponder ();
						break;
					case MessageBoxDefaultButton.Cancel:
					case MessageBoxDefaultButton.Default:
						cancel.BecomeFirstResponder ();
						break;
					}
				}
				break;
			default:
				throw new NotSupportedException ();
			}
		}

		static NSAlertStyle Convert(MessageBoxType type)
		{
			switch (type) 
			{
			case MessageBoxType.Error:
				return NSAlertStyle.Critical;
			case MessageBoxType.Information:
				return NSAlertStyle.Informational;
			default:
				throw new NotSupportedException ();
			}
		}
	}
}

