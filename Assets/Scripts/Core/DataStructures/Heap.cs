using System;

public class Heap<T> where T : IHeapItem<T>
{
    private readonly T[] items;
    private int itemsCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = itemsCount;
        items[itemsCount] = item;
        SortUp(item);
        itemsCount++;
    }

    public T Remove()
    {
        T firstItem = items[0];
        itemsCount--;
        items[0] = items[itemsCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        if (item.HeapIndex < 0 || item.HeapIndex >= items.Length)
        {
            throw new Exception($"Illegal heap index {item.HeapIndex}.");
        }

        if (!items[item.HeapIndex].Equals(item))
        {
            throw new Exception($"Illegal heap item {item}.");
        }

        SortUp(item);
    }

    public int Count
    {
        get
        {
            return itemsCount;
        }
    }

    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    private void SortDown(T item)
    {
        int itemIndex = item.HeapIndex;
        int swapIndex = 0;

        while (true)
        {
            int childIndexLeft = itemIndex * 2 + 1;
            int childIndexRight = itemIndex * 2 + 2;

            if (childIndexLeft >= itemsCount)
            {
                break;
            }

            swapIndex = childIndexLeft;
            if (childIndexRight < itemsCount && items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
            {
                swapIndex = childIndexRight;
            }

            if (item.CompareTo(items[swapIndex]) < 0)
            {
                Swap(item, items[swapIndex]);

                itemIndex = item.HeapIndex;
            }
            else
            {
                break;
            }
        }
    }

    private void SortUp(T item)
    {
        int itemIndex = item.HeapIndex;

        while (itemIndex > 0)
        {
            int parentIndex = (itemIndex - 1) / 2;
            T parentItem = items[parentIndex];

            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
                itemIndex = item.HeapIndex;
            }
            else
            {
                break;
            }
        }
    }

    private void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        (itemB.HeapIndex, itemA.HeapIndex) = (itemA.HeapIndex, itemB.HeapIndex);
    }
}
