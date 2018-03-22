
using System;
using System.Collections;
using System.Collections.Generic;

namespace Poster
{
    public class CycleList<TItem> : IList<TItem>
    {
        private readonly List<TItem> _buffer;
        private int _currentIndex;
        private int _capacity;
        private int _itemsCount;



        public CycleList(int capacity)
        {
            _capacity = capacity;
            _currentIndex = 0;
            _buffer = new List<TItem>(capacity);
            _itemsCount = 0;

            for (int i = 0; i < capacity; i++)
                _buffer.Add(default(TItem));
        }



        public TItem this[int index]
        {
            get { return _buffer[index]; }
            set { _buffer[index] = value; }
        }



        public void Add(TItem item)
        {
            _buffer[_currentIndex] = item;
            _currentIndex++;

            if (_currentIndex >= _capacity)
                _currentIndex = 0;
        }


        public void Clear()
        {
            _currentIndex = 0;
            _itemsCount = 0;
            for (int i = 0; i < _capacity; i++)
                _buffer[i] = default(TItem);
        }

        public bool Contains(TItem item)
        {
            return _buffer.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            _buffer.CopyTo(array, arrayIndex);
        }

        public bool Remove(TItem item)
        {
            try
            {
                _buffer[_buffer.IndexOf(item)] = default(TItem);
                _itemsCount = Math.Max(_itemsCount - 1, 0);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public int Count
        {
            get { return _itemsCount; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(TItem item)
        {
            return _buffer.IndexOf(item);
        }

        public void Insert(int index, TItem item)
        {
            _buffer[index] = item;
        }

        public void RemoveAt(int index)
        {
            _buffer[index] = default(TItem);
        }





        public IEnumerator<TItem> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }
    }
}