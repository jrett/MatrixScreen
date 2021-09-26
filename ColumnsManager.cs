using System;
using System.Collections.Generic;

namespace MatrixScreen
{
    /// <summary>
    /// This class manages the instantiation and cleanup of an appropriate number of columns.
    /// </summary>
    internal class ColumnsManager : IRenderable
    {
        private const float _columnCountFactor = 0.75f;
        private LinkedList<Column> _columns;
        private int _columnCount = 0;
        private ShuffleInts _indexShuffler;

        internal ColumnsManager()
        {
            _columns = new LinkedList<Column>();
            _indexShuffler = new ShuffleInts(Console.WindowWidth);
        }

        public void Draw(long elapsedTime)
        {
            foreach (Column column in _columns)
            {
                column.Draw(elapsedTime);
            }
        }

        public void Update(long elapsedTime)
        {
            _columnCount = (int)(Console.WindowWidth * _columnCountFactor);
            if (_columnCount < 1)
            {
                _columnCount = 1;
            }

            // Update our list of available columns positions if the window size changed.
            if (_indexShuffler.SetSize != Console.WindowWidth)
            {
                _indexShuffler = new ShuffleInts(Console.WindowWidth);
            }

            // Iterate over the list manually because we can delete nodes directly.
            LinkedListNode<Column> itemNode = _columns.First;
            while (itemNode != null && itemNode.Value != null)
            {
                itemNode.Value.Update(elapsedTime);                             // Update the column
                if (itemNode.Value.IsDone)
                {
                    _columns.Remove(itemNode);                                  // Get rid of the column if it's done.
                    _indexShuffler.CheckinInteger(itemNode.Value.ColumnIndex);  // Make the column index available to the pool of candidate columns
                }
                itemNode = itemNode.Next; // Even if the node was deleted from the list, the next pointer still points to the next node in the list.
            }

            while (_columns.Count < _columnCount)
            {
                int columnIndex = _indexShuffler.CheckoutRandomInteger();
                if (columnIndex > -1)
                {
                    _columns.AddLast(new Column(columnIndex));
                }
            }
        }
    }
}
