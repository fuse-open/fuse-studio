using System;
using System.Reactive.Concurrency;
using System.Web.Util;
using Foundation;
using ObjCRuntime;

namespace Outracks.Fusion.Mac
{
	class WebViewImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			// Need to initialize HttpEncoder.Current because it doesn't default to HttpEncoder.Default.
			// See #3385 for more details.
			HttpEncoder.Current = HttpEncoder.Default;

			WebView.Implementation.UrlFactory = (uri, onNavigating) =>
				Control.Create(self =>
				{
					var webView = new WebKit.WebView();

					webView.DecidePolicyForNavigation += (sender, eventArgs) =>
					{
						var listener = new WebPolicyListener(eventArgs.DecisionToken.Handle);
						listener.Use();

						var navigatingArgs = new Navigating(new Uri(eventArgs.Request.Url.AbsoluteString));
						onNavigating(navigatingArgs);

						if (navigatingArgs.Cancel)
							listener.Ignore();
					};

					webView.MainFrame.LoadRequest(new NSUrlRequest(new NSUrl(uri.AbsoluteUri)));

					self.BindNativeDefaults(webView, dispatcher);

					return webView;
				});

			WebView.Implementation.StringFactory = (content) =>
				Control.Create (self =>
					{
						var webView = new WebKit.WebView();

						self.BindNativeProperty(dispatcher, "content", content, c => webView.MainFrame.LoadHtmlString(c, new NSUrl("")));
						self.BindNativeDefaults(webView, dispatcher);

						return webView;
					});
		}
	}

	class WebPolicyListener : NSObject
	{
		static readonly IntPtr SelUseHandle = Selector.GetHandle("use");
		static readonly IntPtr SelIgnoreHandle = Selector.GetHandle("ignore");

		public WebPolicyListener(IntPtr handle)
			: base(handle)
		{ }

		public void Use()
		{
			Messaging.IntPtr_objc_msgSend(Handle, SelUseHandle);
		}

		public void Ignore()
		{
			Messaging.IntPtr_objc_msgSend(Handle, SelIgnoreHandle);
		}
	}
}
