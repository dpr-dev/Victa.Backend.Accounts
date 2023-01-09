namespace System.Collection.Generic;

public static class LinkedListExtensions
{
    public static LinkedListNode<T>? FirstOrDefaultMatching<T>(this LinkedList<T> list, Func<T, bool> matcher)
    {
        LinkedListNode<T>? node = list.First;
        while (node != null)
        {
            if (matcher(node.Value))
            {
                return node;
            }

            node = node.Next;
        }

        return null;
    }
}
