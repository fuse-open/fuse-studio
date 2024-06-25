namespace Outracks.Fuse.Hierarchy
{
	public class PendingDrop
	{
		readonly int _rowOffset;
		readonly DropPosition _dropPosition;
		readonly int _depth;


		public int RowOffset
		{
			get { return _rowOffset; }
		}

		public DropPosition DropPosition
		{
			get { return _dropPosition; }
		}

		public int Depth
		{
			get { return _depth; }
		}

		public PendingDrop(int rowOffset, DropPosition dropPosition, int depth)
		{
			_rowOffset = rowOffset;
			_dropPosition = dropPosition;
			_depth = depth;
		}
	}
}