using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace Outracks.Fusion.Windows
{
	class WebViewImplementation
	{
		public static void Initialize(IScheduler scheduler)
		{
			WebView.Implementation.UrlFactory += (uri, onNavigating) =>
				Control.Create(control =>
					{
						var webBrowser = new WebBrowser();
						
						webBrowser.Navigating += (sender, args) =>
						{
							var navigatingArgs = new Navigating(args.Uri);
							onNavigating(navigatingArgs);
							args.Cancel = navigatingArgs.Cancel;
						};
						

						DataBinding.ObservableFromNativeEvent<EventArgs>(webBrowser, "LoadCompleted")
							.CombineLatest(webBrowser.GetDpi(), Tuple.Create)
							.Subscribe((_, density) => webBrowser.SetZoom(density));

						scheduler.Schedule(() => webBrowser.Navigate(uri));

						control.BindNativeDefaults(webBrowser, scheduler);

						return webBrowser;
					});

			WebView.Implementation.StringFactory += (content) =>
				Control.Create(control =>
					{
						var webBrowser = new WebBrowser();
						content.Subscribe( c =>
						{
							if (!string.IsNullOrEmpty(c))
								scheduler.Schedule(() =>  webBrowser.NavigateToString(c));
						});
						control.BindNativeDefaults(webBrowser, scheduler);
						return webBrowser;
					});

		}
	}

	static class WebBrowserExtensions
	{
		public static void SetZoom(this WebBrowser webBrowser, double zoom)
		{
			if (zoom <= 1)
				return;

			var doc = webBrowser.Document as mshtml.IHTMLDocument2;
			doc.parentWindow.execScript(
				"if(document != null && document.body != null) {"
					+ "document.body.className = document.body.className + \" hdpi\";"
				+ "}");
		}
	}
	
}
