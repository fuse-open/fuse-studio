using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Fuse.Installer.Gui
{
	public class DesignTimeResourceDictionary : ResourceDictionary
	{
		private readonly ObservableCollection<ResourceDictionary> _noopMergedDictionaries = new NoopObservableCollection<ResourceDictionary>();

		private class NoopObservableCollection<T> : ObservableCollection<T>
		{
			protected override void InsertItem(int index, T item)
			{
				// Only insert items while in Design Mode (VS is hosting the visualization)
				if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
				{
					base.InsertItem(index, item);
				}
			}
		}

		public DesignTimeResourceDictionary()
		{
			var fieldInfo = typeof(ResourceDictionary).GetField("_mergedDictionaries", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fieldInfo != null)
			{
				fieldInfo.SetValue(this, _noopMergedDictionaries);
			}
		}
	}
}