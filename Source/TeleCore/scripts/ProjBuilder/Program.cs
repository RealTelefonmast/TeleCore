// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
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
        TestEnum testEnum = TestEnum.A | TestEnum.B;
        TestEnum2 testEnum2 = TestEnum2.A | TestEnum2.B;
        TestEnum3 testEnum3 = TestEnum3.A | TestEnum3.B;

        var test1 = EnumUtils.HasFlag(TestEnum.A | TestEnum.B, TestEnum.B);
        var test11 = EnumUtils.HasFlag(TestEnum.A | TestEnum.B, TestEnum.C);
        var test2 = EnumUtils.HasFlag(TestEnum2.A | TestEnum2.E, TestEnum2.E);
        var test22 = EnumUtils.HasFlag(TestEnum2.A | TestEnum2.E, TestEnum2.F);
        var test3 = EnumUtils.HasFlag(TestEnum3.C | TestEnum3.K, TestEnum3.K);
        var test33 = EnumUtils.HasFlag(TestEnum3.C | TestEnum3.K, TestEnum3.A);
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