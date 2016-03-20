﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Veldrid.Graphics
{
    public class RenderQueue : IEnumerable<RenderItem>
    {
        private const int DefaultCapacity = 250;

        private readonly List<RenderItemIndex> _indices = new List<RenderItemIndex>(DefaultCapacity);
        private readonly List<RenderItem> _renderItems = new List<RenderItem>(DefaultCapacity);

        public void Clear()
        {
            _indices.Clear();
            _renderItems.Clear();
        }

        public void AddRange(IEnumerable<RenderItem> renderItems)
        {
            foreach (RenderItem item in renderItems)
            {
                Add(item);
            }
        }

        public void Add(RenderItem item)
        {
            int index = _renderItems.Count;
            _indices.Add(new RenderItemIndex(item.GetRenderOrderKey(), index));
            _renderItems.Add(item);
            Debug.Assert(_renderItems.IndexOf(item) == index);
        }

        public void Sort()
        {
            _indices.Sort();
        }

        public void Sort(Comparer<RenderOrderKey> keyComparer)
        {
            _indices.Sort(
                (RenderItemIndex first, RenderItemIndex second)
                    => keyComparer.Compare(first.Key, second.Key));
        }

        public IEnumerator<RenderItem> GetEnumerator()
        {
            return new Enumerator(_indices, _renderItems);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(_indices, _renderItems);
        }

        public struct Enumerator : IEnumerator<RenderItem>
        {
            private readonly List<RenderItemIndex> _indices;
            private readonly List<RenderItem> _renderItems;
            private int _nextItemIndex;
            private RenderItem _currentItem;

            public Enumerator(List<RenderItemIndex> indices, List<RenderItem> renderItems)
            {
                _indices = indices;
                _renderItems = renderItems;
                _nextItemIndex = 0;
                _currentItem = null;
            }

            public RenderItem Current => _currentItem;
            object IEnumerator.Current => _currentItem;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_nextItemIndex >= _indices.Count)
                {
                    _currentItem = null;
                    return false;
                }
                else
                {
                    var currentIndex = _indices[_nextItemIndex];
                    _currentItem = _renderItems[currentIndex.ItemIndex];
                    _nextItemIndex += 1;
                    return true;
                }
            }

            public void Reset()
            {
                _nextItemIndex = 0;
                _currentItem = null;
            }
        }
    }
}