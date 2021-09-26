using System;
using System.Collections.Generic;

namespace MatrixScreen
{
    /// <summary>
    /// A class that has a function that takes an array of ColumnNodes (character, highlight, position) and renders them vertically in the console.
    /// Only updates the changes by keeping track of what was previously rendered as I think this would be faster than redrawing every charecter with every frame.
    /// </summary>
    internal class ColumnRenderer : IRenderable
    {
        private const string CSI = "\u001b[";
        private const string GREEN = "32m";
        private const string WHITE = "97m";
        private const string HIDECURSOR = "?25l";

        private int _columnIndex;                // Which column we are.
        private ColumnNode[] _currentColumnData;
        private ColumnNode[] _previousColumnData;
        private int _bufferSize;

        private Queue<ColumnNode> _displayQueue;       // The update method adds items to the display queue which are then rendered in the draw method.

        public ColumnRenderer(int columnIndex, int bufferSize)
        {
            _columnIndex = columnIndex;
            _bufferSize = bufferSize;
            _previousColumnData = new ColumnNode[_bufferSize];
            _displayQueue = new Queue<ColumnNode>(_bufferSize);
        }

        public void UpdateColumnData(ColumnNode[] columnData)
        {
            _currentColumnData = columnData;
        }

        public void Update(long elapsedTime)
        {
            if (_currentColumnData != null)
            {
                for (int x = 0; x < _bufferSize; x++)
                {
                    // Anything in our current buffer that's different from the previous buffer gets added to the displayQueue.
                    if (_previousColumnData[x] != _currentColumnData[x])
                    {
                        ColumnNode newNode = _currentColumnData[x];
                        newNode.position = x;
                        _displayQueue.Enqueue(newNode);
                        _previousColumnData[x] = newNode;
                    }
                }
            }
        }

        public void Draw(long elapsedTime)
        {
            ColumnNode item;
            while (_displayQueue.Count > 0)
            {
                item = _displayQueue.Dequeue();
                if (item.highLight)
                {
                    Console.Write($"{CSI}{HIDECURSOR}{CSI}{WHITE}{CSI}{item.position};{_columnIndex}H{item.character}");
                }
                else
                {
                    Console.Write($"{CSI}{HIDECURSOR}{CSI}{GREEN}{CSI}{item.position};{_columnIndex}H{item.character}");
                }
            }
        }
    }
}
