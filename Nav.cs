using System.Collections;

namespace CS_Lessons.Utils;

internal class MyListEnumerator : IEnumerator<int>
{
    private Node? _head;

    private Node? _currentNode;
    private int _currentValue;


    public MyListEnumerator(Node? head)
    {
        _head = head;
        _currentNode = null;
    }

    public int Current
    {
        get
        {
            if (_currentNode == null) throw new InvalidOperationException();
            return _currentNode.Value;
        }
    }

    object IEnumerator.Current
    {
        get
        {
            return Current;
        }
    }

    public bool MoveNext()
    {
        if (_currentNode == null)
        {
            _currentNode = _head;
        }

        else
        {
            _currentNode = _currentNode.Next;
        }

        return _currentNode != null;
    }

    public void Reset()
    {
        _currentNode = null;
    }

    public void Dispose()
    {

    }
}

public class MyList : IEnumerable<int>
{

    private Node? _head;
    private Node? _tail;

    public int Count { get; set; }

    public MyList(int[] values)
    {
        Node? lastNode = null;
        foreach (var value in values)
        {
            var newNode = new Node() { Value = value };
            if (lastNode != null)
            {
                lastNode.Next = newNode;
            }
            else
            {
                _head = newNode;
            }
            lastNode = newNode;
        }
        _tail = lastNode;
    }

    public override string ToString()
    {
        if (_head == null) return "[]";

        var values = new int[Count];
        var current = _head;

        var count = 0;
        while (current != null)
        {
            values[count++] = current.Value;
            current = current.Next;
        }

        return $"[{string.Join(", ", values)}]";

    }
    public IEnumerator<int> GetEnumerator()
    {
        return new MyListEnumerator(_head);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}
