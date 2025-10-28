using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHeapItem<T> : IComparable<T> //ÿ��Ԫ�ض��ɱȽϣ�ʵ����С�ѵĻ�������
{
    //�洢Ԫ���ڶ��е�λ��
    int HeapIndex { get; set; }
}

public class Heap<T> where T : IHeapItem<T> //����T�������IHeapItem�ӿ�
{
    private T[] items;
    private int currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    //��ֵ��ǰԪ���ڶ��е�λ��Ϊ��ǰԪ�ظ������������������У�ʹ���ϸ������ع���С�ѣ�����Ԫ�ظ���
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    //ȡ���Ѷ�Ԫ�أ�����Ԫ�ظ��������ѵ�Ԫ�ط��ڶѶ���������������ʹ���³������ع���С�ѣ������Ƴ���Ԫ��
    public T RemoveFirst()
    {
        T firstItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return firstItem;
    }

    //����·������
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

    //���ⲿ���Բ�ѯ��Ԫ�ظ�����������¶˽��Ԫ��
    public int Count => currentItemCount;

    //ȷ���ɹ����Ԫ��
    public bool Contains(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    //�³���������
    //��������Ԫ������Ϊ��ǰ�˶���һ������Ԫ�ؼӶ�����������Ϊ0
    //��ѭ�����������Ԫ������С���ܸ�������С�����˳�ѭ�������������Ԫ��С���ܸ���������Ԫ��ֵС���ң���ֵ��������Ϊ��
    //�����ǰԪ��ֵС�ڽ�������ֵ������Ԫ�أ������˳�ѭ��
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

    //�ϸ���������
    //���常�ڵ���������ѭ�������常�ڵ㸳ֵ����Ԫ�أ������ǰԪ�ش��ڸ��ڵ㣬�����������˳�ѭ����������Ҫ���¸��ڵ�����
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

    //��������������item��Ӧ���������ݺ�˫��������
    private void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int index = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = index;
    }
}
