using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeapItem<T> : IComparable<T> //每个元素都可比较，实现最小堆的基本条件
{
    //存储元素在堆中的位置
    int HeapIndex { get; set; }
}

public class Heap<T> where T : IHeapItem<T> //限制T必须完成IHeapItem接口
{
    private T[] items;
    private int currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    //赋值当前元素在堆中的位置为当前元素个数，将其加入堆数组中，使用上浮操作重构最小堆，增加元素个数
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    //取到堆顶元素，减少元素个数，将堆底元素放在堆顶，更改其索引，使用下沉操作重构最小堆，返回移除的元素
    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    //更新路径函数
    public void UpdateItem(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        if (item.HeapIndex > 0 && items[parentIndex].CompareTo(item) > 0)
        {
            SortUp(item);
        }
        else
        {
            SortDown(item);
        }
    }

    //让外部可以查询堆元素个数，而不暴露私有元素
    public int Count => currentItemCount;

    //确保成功添加元素
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //下沉操作函数
    //定义左子元素索引为当前乘二加一，右子元素加二，交换索引为0
    //真循环，如果左子元素索引小于总个数（不小于则退出循环），如果右子元素小于总个数且左子元素值小于右，赋值交换索引为右
    //如果当前元素值小于交换索引值，交换元素，否则退出循环
    private void SortDown(T item)
    {
        int leftChildIndex = item.HeapIndex * 2 + 1;
        int rightChildIndex = item.HeapIndex * 2 + 2;
        int swapIndex = 0;
        while (true)
        {
            if (leftChildIndex < currentItemCount)
            {
                if ((rightChildIndex < currentItemCount) && items[leftChildIndex].CompareTo(items[rightChildIndex]) > 0)
                {
                    swapIndex = rightChildIndex;
                }
                if (item.CompareTo(items[swapIndex]) > 0)
                {
                    Swap(item, items[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    //上浮操作函数
    //定义父节点索引，真循环，定义父节点赋值索引元素，如果当前元素大于父节点，交换，否则退出循环。继续则要更新父节点索引
    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            T parentItem = items[parentIndex];
            if(item.CompareTo(items[parentIndex]) > 0)
            {
                Swap(item, items[parentIndex]);
            }
            else
            {
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    //交换函数，交换item对应索引的内容和双方的索引
    private void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int index = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = index;
    }
}
