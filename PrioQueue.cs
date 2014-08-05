using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Buerkert.Diagrams.Service
{
    public sealed class PrioQueue<TValue, TPriority> : ICollection<TValue>, IEnumerable<TValue>, IEnumerable
    {
        /// <summary>
        /// The current priority type for the queue.
        /// </summary>
        private readonly OrderType queueType;
        /// <summary>
        /// The RedBlack tree list this queue is based on.
        /// </summary>
        private readonly RedBlackTreeList<TPriority, TValue> tree;
        /// <summary>
        /// Gets the number of elements still in the queue.
        /// </summary>
        public int Count
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets or sets the default priority when an item is added.
        /// </summary>
        /// <remarks>The default value is zero if not set.</remarks>
        /// <value>The default priority.</value>
        public TPriority DefaultPriority
        {
            get;
            set;
        }
        /// <summary>
        /// Gets <c>false</c> since this queue is never read-only.
        /// </summary>
        /// <value>
        /// 	<c>false</c>.
        /// </value>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// Initializes a new instance of the PrioQueue class.
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        public PrioQueue(OrderType queueType = OrderType.Descending)
            : this(queueType, Comparer<TPriority>.Default)
        {
        }
        /// <summary>
        /// Initializes a new instance of the PrioQueue class.
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <param name="comparer">The comparer.</param>
        public PrioQueue(OrderType queueType, IComparer<TPriority> comparer)
        {
            if (queueType != OrderType.Ascending && queueType != OrderType.Descending)
            {
                throw new ArgumentOutOfRangeException("queueType");
            }
            this.queueType = queueType;
            this.tree = new RedBlackTreeList<TPriority, TValue>(comparer);
        }
        /// <summary>
        /// Initializes a new instance of the PrioQueue class.
        /// </summary>
        /// <param name="queueType">Type of the queue.</param>
        /// <param name="comparison">The comparison.</param>
        /// <inheritdoc />
        public PrioQueue(OrderType queueType, Comparison<TPriority> comparison)
            : this(queueType, new ComparisonComparer<TPriority>(comparison))
        {
        }
        /// <summary>
        /// Adds the specified item to the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(TValue item)
        {
            this.Push(item, this.DefaultPriority);
        }
        /// <summary>
        /// Adds the specified items to the priority queue with the specified priority.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="items" /> is a null reference (<c>Nothing</c> in Visual Basic).</exception>
        public void AddPriorityGroup(IEnumerable<TValue> items, TPriority priority)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            this.AddPriorityGroupItem(items, priority);
        }
        /// <summary>
        /// Clears this queue of all its items..
        /// </summary>
        public void Clear()
        {
            this.ClearItems();
        }
        /// <summary>
        /// Returns whether the given item is present in the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///   <c>true</c> if the queue contains the given item; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TValue item)
        {
            return this.tree.ContainsValue(item);
        }
        /// <summary>
        /// Copies the content of the queue to the given array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(TValue[] array, int arrayIndex)
        {
            if (array.Length - arrayIndex < this.Count)
            {
                throw new ArgumentException("ArrayIndex should not exceed array.Length", "array");
            }
            foreach (TValue item in (
                from association in this.tree
                select association.Value).SelectMany((LinkedList<TValue> items) => items))
            {
                array.SetValue(item, arrayIndex++);
            }
        }
        /// <summary>
        /// Dequeues the item at the front of the queue.
        /// </summary>
        /// <returns>The item at the front of the queue.</returns>
        public TValue Pop()
        {
            TPriority priority;
            return this.Dequeue(out priority);
        }
        /// <summary>
        /// Dequeues the item from the head of the queue.
        /// </summary>
        /// <param name="priority">The priority of the item to dequeue.</param>
        /// <returns>The item at the head of the queue.</returns>
        public TValue Dequeue(out TPriority priority)
        {
            return this.DequeueItem(out priority);
        }
        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Enqueue(TValue item)
        {
            this.Push(item);
        }
        /// <summary>
        /// Enqueues the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="priority">The priority.</param>
        public void Enqueue(TValue item, TPriority priority)
        {
            this.Push(item, priority);
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return this.tree.GetValueEnumerator();
        }
        /// <summary>
        /// Returns an enumerator that iterates through the keys in the collection.
        /// </summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the keys in the collection.</returns>
        public IEnumerator<KeyValuePair<TPriority, TValue>> GetKeyEnumerator()
        {
            return this.tree.GetKeyEnumerator();
        }
        /// <summary>
        /// Removes the items with the specified priority.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <returns>The items with the specified priority.</returns>
        public IList<TValue> GetPriorityGroup(TPriority priority)
        {
            LinkedList<TValue> items;
            if (!this.tree.TryGetValue(priority, out items))
            {
                return new List<TValue>();
            }
            return new List<TValue>(items);
        }
        /// <summary>
        /// Peeks at the item in the front of the queue, without removing it.
        /// </summary>
        /// <returns>The item at the front of the queue.</returns>
        public TValue Peek()
        {
            return this.GetNextItem().Value.First.Value;
        }
        /// <summary>
        /// Peeks at the item in the front of the queue, without removing it.
        /// </summary>
        /// <param name="priority">The priority of the item.</param>
        /// <returns>The item at the front of the queue.</returns>
        public TValue Peek(out TPriority priority)
        {
            KeyValuePair<TPriority, LinkedList<TValue>> association = this.GetNextItem();
            TValue item = association.Value.First.Value;
            priority = association.Key;
            return item;
        }
        /// <summary>
        /// Pushes the specified item in the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Push(TValue item)
        {
            this.Push(item, this.DefaultPriority);
        }
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <param name="priority">The priority of the item.</param>
        public void Push(TValue item, TPriority priority)
        {
            this.AddItem(item, priority);
        }
        /// <summary>
        /// Removes the specified item from this queue.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public bool Remove(TValue item)
        {
            TPriority priority;
            return this.Remove(item, out priority);
        }
        /// <summary>
        /// Removes the first occurrence of the specified item from the property queue.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="priority">The priority associated with the item.</param>
        public bool Remove(TValue item, out TPriority priority)
        {
            return this.RemoveItem(item, out priority);
        }
        /// <summary>
        /// Removes the items with the specified priority.
        /// </summary>
        /// <param name="priority">The priority.</param>
        public bool RemovePriorityGroup(TPriority priority)
        {
            return this.RemoveItems(priority);
        }
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        /// <summary>
        /// Adds the item to the queue.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="priority">The priority of the item.</param>
        /// <remarks>
        /// 	<b>Notes to Inheritors: </b>
        /// </remarks>
        private void AddItem(TValue item, TPriority priority)
        {
            LinkedList<TValue> list;
            if (this.tree.TryGetValue(priority, out list))
            {
                list.AddLast(item);
            }
            else
            {
                list = new LinkedList<TValue>();
                list.AddLast(item);
                this.tree.Add(priority, list);
            }
            this.Count++;
        }
        /// <summary>
        /// Adds the specified items to the priority queue with the specified priority.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="priority">The priority.</param>
        private void AddPriorityGroupItem(IEnumerable<TValue> items, TPriority priority)
        {
            LinkedList<TValue> currentValues;
            if (this.tree.TryGetValue(priority, out currentValues))
            {
                using (IEnumerator<TValue> enumerator = items.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        TValue t = enumerator.Current;
                        currentValues.AddLast(t);
                    }
                    return;
                }
            }
            currentValues = new LinkedList<TValue>(items);
            this.tree.Add(priority, currentValues);
        }
        /// <summary>
        /// Checks if the list is not empty, and if it is, throw an exception.
        /// </summary>
        private void CheckTreeNotEmpty()
        {
            if (this.tree.Count == 0)
            {
                throw new InvalidOperationException("The Priority Queue is empty.");
            }
        }

        /// <summary>
        /// Clears all the objects in this instance.
        /// </summary>
        /// <remarks>
        /// <b>Notes to Inheritors: </b>
        /// </remarks>
        private void ClearItems()
        {
            this.tree.Clear();
            this.Count = 0;
        }

        /// <summary>
        /// Dequeues the item at the front of the queue.
        /// </summary>
        /// <returns>The item at the front of the queue.</returns>
        private TValue DequeueItem(out TPriority priority)
        {
            KeyValuePair<TPriority, LinkedList<TValue>> association = this.GetNextItem();
            TValue item = association.Value.First.Value;
            association.Value.RemoveFirst();
            TPriority key = association.Key;
            if (association.Value.Count == 0)
            {
                this.tree.Remove(association.Key);
            }
            this.Count--;
            priority = key;
            return item;
        }

        /// <summary>
        /// Gets the next item.
        /// </summary>
        private KeyValuePair<TPriority, LinkedList<TValue>> GetNextItem()
        {
            this.CheckTreeNotEmpty();
            if (this.queueType != OrderType.Descending)
            {
                return this.tree.Minimum;
            }
            return this.tree.Maximum;
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <param name="priority">The priority of the item that was removed.</param>
        /// <returns>An indication of whether the item was found, and removed.</returns>
        private bool RemoveItem(TValue item, out TPriority priority)
        {
            bool removed = this.tree.Remove(item, out priority);
            if (removed)
            {
                this.Count--;
            }
            return removed;
        }

        /// <summary>
        /// Removes the items from the collection with the specified priority.
        /// </summary>
        /// <param name="priority">The priority to search for.</param>
        /// <returns>An indication of whether items were found having the specified priority.</returns>
        private bool RemoveItems(TPriority priority)
        {
            LinkedList<TValue> items;
            if (this.tree.TryGetValue(priority, out items))
            {
                this.tree.Remove(priority);
                this.Count -= items.Count;
                return true;
            }
            return false;
        }
    }
}
