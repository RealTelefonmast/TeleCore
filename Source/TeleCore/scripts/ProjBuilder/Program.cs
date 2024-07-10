// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using TeleCore.Lib;
using TeleCore.Lib.Utils;

[Flags]
public enum TestEnum : byte
{
    A = 1,
    B = 2,
    C = 4,
    D = 8,
}

[Flags]
public enum TestEnum2 : int
{
    A = 1,
    B = 2,
    C = 4,
    D = 8,
    E = 16,
    F = 32,
}

[Flags]
public enum TestEnum3 : long
{
    A = 1,
    B = 2,
    C = 4,
    D = 8,
    E = 16,
    F = 32,
    G = 64,
    K = 1024,
}

internal class Program
{
    public static void Main(string[] args)
    {
        // DHBGraph graph = new DHBGraph();
        // graph.AddVertex(1);
        // graph.AddEdge(2,1);
        // graph.AddEdge(5,1);
        // graph.AddEdge(6,1);
        // graph.AddEdge(8,2);
        var num0 = MathT.NextPowerOfTwo(5);
        var num1 = MathT.NextPowerOfTwo(12);
        var num2 = MathT.NextPowerOfTwo(20);
        var num3 = MathT.NextPowerOfTwo(28);
        var num4 = MathT.NextPowerOfTwo(48);
        var num5 = MathT.NextPowerOfTwo(100);
        var q = 1 + 0.04;
        var sum = 120000;
        var n = 6;
        Console.WriteLine("" + sum * ((Math.Pow(q, n)*(q-1))/(Math.Pow(q,n)-1)));
        Console.ReadKey();
    }
}

public class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; }
    public bool IsEndOfWord { get; set; }

    public TrieNode()
    {
        Children = new Dictionary<char, TrieNode>();
        IsEndOfWord = false;
    }
}

public class Trie
{
    private readonly TrieNode _root;

    public TrieNode Root => _root;
    
    public Trie()
    {
        _root = new TrieNode();
    }

    public void Insert(string word)
    {
        TrieNode node = _root;

        foreach (char c in word)
        {
            if (!node.Children.ContainsKey(c))
            {
                node.Children[c] = new TrieNode();
            }
            node = node.Children[c];
        }

        node.IsEndOfWord = true;
    }

    public bool Search(string word)
    {
        TrieNode node = _root;

        foreach (char c in word)
        {
            if (!node.Children.ContainsKey(c))
            {
                return false;
            }
            node = node.Children[c];
        }

        return node.IsEndOfWord;
    }

    public bool StartsWith(string prefix)
    {
        TrieNode node = _root;

        foreach (char c in prefix)
        {
            if (!node.Children.ContainsKey(c))
            {
                return false;
            }
            node = node.Children[c];
        }

        return true;
    }
}