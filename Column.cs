using System;

namespace MatrixScreen
{
    /// <summary>
    /// A column here is a single line/column in our matrix style display. It will draw a string of random characters, from top to bottom of the screen, at a random speed such that it is pleasantly observable.
    /// The top of the screen is not always the actual top of the screen, and the bottom of the screen is not always the actual bottom of the screen.
    /// Also, it's not always from top to bottom, sometimes its from bottom to top.
    /// 
    /// The charecters will be in green, except for the lead character which will sometimes be white.
    /// While the column is diplayed on screen, some random charecters inside it will continue to change, randomly.
    /// 
    /// The top and bottom areas are randomly calculated. The buffer of characters will always be equal to the number rows that exist in the display.
    /// 
    /// Once an instance of a column is complete, it is discarded by the caller.
    /// </summary>
    internal class Column : IRenderable
    {
        private int _windowHeight = System.Console.WindowHeight;
        private int _totalNodeCount;             // length of the string for this column
        private int _currentNodeCount;           // How many nodes have been drawn.
        private int _eraseThreshold;             // Erase threshold must be equal to or lower than nodeCount. When the eraseThreshold number of charecters have been drawn, the tail of the string begins to erase.
        private long _addRemoveTransitionDelay;  // How long to wait after adding chars has completed before starting to removing them.
        private long _addRemoveDelay;            // Milliseconds between each add/remove of characters.
        private long _addRemoveUpdateTime;       // When do we actually add/removoe a character
        private int _addNodeIndex;               // The current node index where the next character is added while drawing.
        private int _removeNodeIndex;            // The current node index where the next charecter is erased while erasing.
        private bool _highlightLeadNode;         // Do we want th lead charecter to be white or the same as all others?
        private bool _goingUp = false;           // We go down by default, but we can go up.
        private bool _erasing = false;           // When we reach the erase threshold, we flip this flag and erase until we're done.

        // Active nodes are random nodes which randomly change while on screen.
        private int _activeNodeCount;            // The number of nodes which remain actively changing after initially being drawn.
        private int[] _activeNodeIndexes;        // indexes to the column members which continue to be actively changing. The rest stay the charecter that it was originally assigned.
        private int[] _activeNodeDelaysMax;      // Max milliseconds between changes in the charecters in the string that do change. Actual number is a random number at runtime with this as the max.
        private long[] _activeNodeUpdateTime;

        private ColumnRenderer _renderer;        // Manages drawing the column
        private ColumnNode[] _nodeBuffer;    // The array of characters to display in our column
        private int _bufferSize;

        private int _topIndex;
        private int _bottomIndex;

        public bool IsDone { get; private set; }
        public int ColumnIndex { get; private set; }                // Column position

        private static readonly Random _random = new Random((int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 42));  // 42 because hitch-hikers
        internal static int GetRandomNumber(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
        private static char _getRandomChar()
        {
            return (char)GetRandomNumber(33, 127);
        }

        // Constants
        private const char  NULLCHAR                = '\0';
        private const char  SPACECHAR               = ' ';
        private const int   MINNODES                = 10;
        private const float ERASETHRESHOLDFACTOR    = 0.5f;
        private const int   MINACTIVENODES          = 1;
        private const float ACTIVENODESCOUNTFACTOR  = 0.15f;
        private const int   MINADDREMOVEDELAY       = 0;
        private const int   MAXADDREMOVEDELAY       = 150;
        private const float HIGHLIGHTLEADNODECHANCE = 0.5f;
        private const int   ACTIVENODEMAXDELAYLOW   = 500;
        private const int   ACTIVENODEMAXDELAYHIGH  = 4000;
        private const int   ACTIVENODEMINDELAY      = 10;
        private const float GOUPCHANCE              = 0.01f;
        private const float PARTIALSCREENCHANCE     = 0.75f;
        private const float PARTIALSCREENSIZE       = 0.75f;
        private const int   ADDTOERASEDELAYMAX      = 4000; // Max

        internal Column(int columnIndex)
        {
            ColumnIndex = columnIndex;

            _addRemoveDelay    = GetRandomNumber(MINADDREMOVEDELAY, MAXADDREMOVEDELAY);
            _highlightLeadNode = GetRandomNumber(0, 100) < (100 * HIGHLIGHTLEADNODECHANCE);
            _goingUp           = GetRandomNumber(0, 100) < (100 * GOUPCHANCE);

            // Screen line indexes are 1 based, not zero based like arrays.
            _topIndex = 1;
            _bottomIndex = _windowHeight + 1;
            _bufferSize = _bottomIndex + 1;

            // See about doing a partial screen hight column
            if (_windowHeight > MINNODES && GetRandomNumber(0, 100) < (100 * PARTIALSCREENCHANCE))
            {
                // We're going with a partial screen sized column

                // Get a random index for the top position
                int tmpFactoredWindowHeight = (int)(_windowHeight * PARTIALSCREENSIZE);
                _topIndex = GetRandomNumber(-tmpFactoredWindowHeight, tmpFactoredWindowHeight);
                if (_topIndex > _windowHeight - MINNODES)
                {
                    _topIndex = _windowHeight - MINNODES;
                }
                if (_topIndex < 1)
                {
                    _topIndex = 1;
                }

                // Then for the bottom position, just use a random length...
                _bottomIndex = _topIndex + GetRandomNumber(0, (int)(_windowHeight * PARTIALSCREENSIZE)) + MINNODES;
                if (_bottomIndex > _windowHeight)
                {
                    _bottomIndex = _windowHeight;
                }
            }
            _totalNodeCount = (_bottomIndex - _topIndex) +1;
            _activeNodeCount = GetRandomNumber(0, (int)(_totalNodeCount * ACTIVENODESCOUNTFACTOR)) + MINACTIVENODES;

            // We don't want the erase threshold to always be lower than the totalNodeCount so a good proportion of the time the column does not start erasing before it is done drawing.
            int tmpFactoredTotal = (int)(_totalNodeCount * ERASETHRESHOLDFACTOR);
            _eraseThreshold = _totalNodeCount - GetRandomNumber(-tmpFactoredTotal, tmpFactoredTotal);
            if (_eraseThreshold > _totalNodeCount)
            {
                _eraseThreshold = _totalNodeCount;
                _addRemoveTransitionDelay = GetRandomNumber(0, ADDTOERASEDELAYMAX);
            }

            if (_goingUp)
            {
                _addNodeIndex = _bottomIndex;
                _removeNodeIndex = _bottomIndex;
            }
            else
            {
                _addNodeIndex = _topIndex;
                _removeNodeIndex = _topIndex;
            }

            _activeNodeIndexes      = new int[_activeNodeCount];
            _activeNodeDelaysMax    = new int[_activeNodeCount];
            _activeNodeUpdateTime   = new long[_activeNodeCount];

            for (int x = 0; x < _activeNodeCount; x++)
            {
                _activeNodeIndexes[x]    = GetRandomNumber(_topIndex, _bottomIndex + 1);
                _activeNodeDelaysMax[x]  = GetRandomNumber(ACTIVENODEMAXDELAYLOW, ACTIVENODEMAXDELAYHIGH);
                _activeNodeUpdateTime[x] = GetRandomNumber(ACTIVENODEMINDELAY, _activeNodeDelaysMax[x]);
            }
            _nodeBuffer = new ColumnNode[_bufferSize];
            _renderer   = new ColumnRenderer(columnIndex, _bufferSize);
        }

        private void AddNode()
        {
            bool updated = false;
            if (_goingUp)
            {
                if (_addNodeIndex < (_bufferSize - 1))
                {
                    // Turn off the highlight on the previous one, being careful not to go beyond the array bounds
                    _nodeBuffer[_addNodeIndex + 1].highLight = false;
                    updated = true;
                }

                if (_addNodeIndex >= _topIndex)
                {
                    _nodeBuffer[_addNodeIndex] = new ColumnNode() { character = _getRandomChar(), highLight = _highlightLeadNode };
                    _currentNodeCount++;
                    _addNodeIndex--;
                    updated = true;
                }
            }
            else
            {
                // Going down
                if (_addNodeIndex > 0)
                {
                    // Turn off the highlight on the previous one, being careful not to go beyond the array bounds
                    _nodeBuffer[_addNodeIndex - 1].highLight = false;
                    updated = true;
                }

                if (_addNodeIndex <= _bottomIndex)
                {
                    _nodeBuffer[_addNodeIndex] = new ColumnNode() { character = _getRandomChar(), highLight = _highlightLeadNode };
                    _currentNodeCount++;
                    _addNodeIndex++;
                    updated = true;
                }
            }
            if (updated)
            {
                _renderer.UpdateColumnData(_nodeBuffer);
            }
        }

        private bool RemoveNode()
        {
            if (_currentNodeCount > 0)
            {
                if (_goingUp)
                {
                    // going up
                    if (_removeNodeIndex >= _topIndex)
                    {
                        _nodeBuffer[_removeNodeIndex--].character = SPACECHAR; //space character for delete?
                        _currentNodeCount--;
                    }
                }
                else
                {
                    // going down
                    if (_removeNodeIndex <= _bottomIndex)
                    {
                        _nodeBuffer[_removeNodeIndex++].character = SPACECHAR; //space character for delete?
                        _currentNodeCount--;
                    }
                }
                _renderer.UpdateColumnData(_nodeBuffer);
                return true;
            }
            return false;
        }

        public void Update(long elapsedTime)
        {
            // handle updating existing active nodes.
            for (int x = 0; x < _activeNodeCount; x++)
            {
                _activeNodeUpdateTime[x] -= elapsedTime;
                if (_activeNodeUpdateTime[x] <= 0)
                {
                    _activeNodeUpdateTime[x] = GetRandomNumber(10, _activeNodeDelaysMax[x]);
                    int i = _activeNodeIndexes[x];
                    char currentChar = _nodeBuffer[i].character;
                    if (currentChar != NULLCHAR && currentChar != SPACECHAR)
                    {
                        _nodeBuffer[i].character = _getRandomChar();
                        _renderer.UpdateColumnData(_nodeBuffer);
                    }
                }
            }


            // handle adding/removing nodes
            _addRemoveUpdateTime += elapsedTime;
            if (_addRemoveUpdateTime > _addRemoveDelay)
            {
                AddNode();
                if (!_erasing && _currentNodeCount >= _eraseThreshold)
                {
                    if (_addRemoveTransitionDelay > 0)
                    {
                        _addRemoveTransitionDelay -= _addRemoveUpdateTime;
                    }
                    else
                    {
                        _erasing = true;
                    }
                }
                if (_erasing)
                {
                    if (!RemoveNode())
                    {
                        IsDone = true;
                    }
                }
                _addRemoveUpdateTime = 0;
            }

            _renderer.Update(elapsedTime);
        }

        public void Draw(long elapsedTime)
        {
            _renderer.Draw(elapsedTime);
        }
    }
}
