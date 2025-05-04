using System;
using System.Collections;
using System.Collections.Generic;

namespace GameLib.Algorythms
{
    /// <summary>
    /// A fixed-size circular buffer (ring buffer) supporting efficient FIFO operations.
    /// When full, new elements overwrite the oldest.
    /// </summary>
    public class CircularBuffer<T> : IEnumerable<T>
    {
        private T[] _buffer;
        private int _head;
        private int _tail;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class.
        /// </summary>
        /// <param name="capacity">The fixed size of the buffer. Must be positive.</param>
        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");

            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        /// <summary>
        /// Gets or sets the capacity of the buffer. Resizing may discard old elements.
        /// </summary>
        public int Capacity
        {
            get => _buffer.Length;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Capacity must be positive.");

                if (value == _buffer.Length)
                    return;

                var newBuffer = new T[value];
                int itemsToCopy = Math.Min(Count, value);

                for (int i = 0; i < itemsToCopy; i++)
                    newBuffer[i] = this[i];

                _buffer = newBuffer;
                Count = itemsToCopy;
                _head = itemsToCopy % value;
                _tail = 0;
            }
        }

        /// <summary>
        /// Gets the number of elements currently in the buffer.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the buffer is full.
        /// </summary>
        public bool IsFull => Count == Capacity;

        /// <summary>
        /// Adds an item to the buffer. Overwrites the oldest if full.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>The overwritten item, or default(T) if none was overwritten.</returns>
        public T Enqueue(T item)
        {
            T overwritten = default;

            if (IsFull)
            {
                overwritten = _buffer[_head];
                _tail = (_tail + 1) % Capacity;
            }
            else
            {
                Count++;
            }

            _buffer[_head] = item;
            _head = (_head + 1) % Capacity;
            return overwritten;
        }

        /// <summary>
        /// Removes and returns the oldest item from the buffer.
        /// </summary>
        /// <returns>The dequeued item.</returns>
        /// <exception cref="InvalidOperationException">If the buffer is empty.</exception>
        public T Dequeue()
        {
            if (Count == 0)
                throw new InvalidOperationException("Buffer is empty.");

            T item = _buffer[_tail];
            _buffer[_tail] = default;
            _tail = (_tail + 1) % Capacity;
            Count--;
            return item;
        }

        /// <summary>
        /// Tries to remove the oldest item without throwing on empty buffer.
        /// </summary>
        /// <param name="item">The dequeued item, if available.</param>
        /// <returns>True if an item was dequeued; otherwise, false.</returns>
        public bool TryDequeue(out T item)
        {
            if (Count == 0)
            {
                item = default;
                return false;
            }

            item = Dequeue();
            return true;
        }

        /// <summary>
        /// Gets or sets the item at the given logical index (0 = oldest, Count-1 = newest).
        /// </summary>
        /// <param name="index">Logical index in the buffer.</param>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return _buffer[(_tail + index) % Capacity];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                _buffer[(_tail + index) % Capacity] = value;
            }
        }

        /// <summary>
        /// Returns the newest item in the buffer.
        /// </summary>
        public T PeekHead()
        {
            if (Count == 0)
                throw new InvalidOperationException("Buffer is empty.");
            return _buffer[(_head - 1 + Capacity) % Capacity];
        }

        /// <summary>
        /// Returns the oldest item in the buffer.
        /// </summary>
        public T PeekTail()
        {
            if (Count == 0)
                throw new InvalidOperationException("Buffer is empty.");
            return _buffer[_tail];
        }

        /// <summary>
        /// Clears the buffer and resets state.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _head = 0;
            _tail = 0;
            Count = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates over the buffer in order (oldest to newest).
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
