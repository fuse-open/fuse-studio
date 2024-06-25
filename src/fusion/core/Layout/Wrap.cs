using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl WrapInReadingOrder(this IObservable<IEnumerable<IControl>> controlsObservable, Func<IObservable<IEnumerable<IControl>>, IControl> rowFactory = null)
		{
			return Control.BindAvailableSize(size =>
			{
				var rowsObservable = controlsObservable.Switch(ctrls =>
				{
					var controls = ctrls.ToArray();

					var widthsObservable = controls
						.Select(c => c.DesiredSize.Width)
						.ToObservableEnumerable()
						.Select(e => e.ToArray());

					return Observable.CombineLatest(
						widthsObservable,
						size.Width,
						(widths, availableWidth) =>
						{
							var rows = new List<List<IControl>>();

							rows.Add(new List<IControl>());
							var width = new Points(0);

							for (int i = 0; i < controls.Length; i++)
							{
								var row = rows[rows.Count - 1];

								width += widths[i];

								var wrap = (row.Count > 0) && (width > availableWidth);
								if (wrap)
								{
									rows.Add(new List<IControl>());
									width = new Points(0);
									i--;
									continue;
								}

								row.Add(controls[i]);
							}

							return rows;
						});
				});

				return StackFromTop(rowsObservable.PoolPerElement(rowFactory ?? (a => StackFromLeft(a))));
			});
		}

		/// <summary>
		/// A two-dimensional grid layout whose number of columns is determined by the maximum width of the input controls.
		/// </summary>
		public static IControl GridWrap(this IObservable<IEnumerable<IControl>> controlsObservable)
		{
			return Control.BindAvailableSize(size =>
			{
				var columnsObservable = controlsObservable.Select(ctrls =>
				{
					var controls = ctrls as IControl[] ?? ctrls.ToArray();
					var maxWidth = controls
						.Select(c => c.DesiredSize.Width)
						.ToObservableEnumerable()
						.Select(l => Math.Max(1, l.IsEmpty() ? 1 : l.Max()))
						.DistinctUntilChanged();

					var controlsPerRow = Observable.CombineLatest(
						maxWidth,
						size.Width,
						(maxWidths, availableWidth) =>
						{
							var divisor = Math.Max(1, (int) (availableWidth / maxWidths));
							return (controls.Length + (divisor-1)) / divisor;
						})
						.DistinctUntilChanged();

					return controlsPerRow.Select(ctrlsPerRow => controls.Batch(ctrlsPerRow));
				})
				.Switch();

				return SubdivideHorizontally(
					columnsObservable.SelectPerElement(StackFromTop));
			});
		}

		public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
		{
			using (var enumerator = source.GetEnumerator())
				while (enumerator.MoveNext())
					yield return YieldBatchElements(enumerator, batchSize);
		}

		private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
		{
			yield return source.Current;
			for (int i = 1; i < batchSize && source.MoveNext(); ++i)
				yield return source.Current;
		}
	}

}