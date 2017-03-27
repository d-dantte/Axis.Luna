using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Axis.Luna.Test
{
    public class BalanceResult { public bool IsBalanced; public int Height; }
    public class TreeNode { public TreeNode Left; public TreeNode Right; public string Name; }
    public class MarkedNode {
        public TreeNode Node; public MarkedNode Parent; public Func<int> LHeight; public Func<int> RHeight;  public int Height => Math.Max(LHeight?.Invoke() ?? 0, RHeight?.Invoke() ?? 0);
        //public static IEnumerable<MarkedNode> Enumerate(TreeNode node, TreeNode parent, int[] hm)
        //{
        //    int[] lhm = new int[1], rhm = new int[1];
        //    if (node.Left != null) foreach (var dpt in Enumerate(node.Left, node, lhm)) yield return dpt;
        //    if (node.Right != null) foreach (var dpt in Enumerate(node.Right, node, rhm)) yield return dpt;
        //    yield return new MarkedNode { Node = node, Parent = parent, Height = hm[0] = Math.Max(lhm[0], rhm[0]) + 1 };
        //}
        public static IEnumerable<MarkedNode> Flatten(TreeNode node, MarkedNode parent)
        {
            var mn = new MarkedNode { Node = node, Parent = parent };
            if (parent!=null && parent.LHeight == null) parent.LHeight = () => mn.Height + 1;
            else if (parent!= null && parent.RHeight == null) parent.RHeight = () => mn.Height + 1;
            yield return mn;
            if (node.Left != null) foreach (var dpt in Flatten(node.Left, mn)) yield return dpt;
            if (node.Right != null) foreach (var dpt in Flatten(node.Right, mn)) yield return dpt;
        }
        public static bool IsBalanced2(TreeNode node) {
            var g = Flatten(node, null).GroupBy(_x => _x.Parent).Where(_x => _x.Key != null).ToArray();
            return g.All(_x => Math.Abs((_x.FirstOrDefault()?.Height ?? 0) - (_x.Skip(1).FirstOrDefault()?.Height ?? 0)) <= 1);
        }

        public static BalanceResult IsBalanced(TreeNode node)
        {
            var leftr = node.Left != null ? IsBalanced(node.Left) : new BalanceResult { IsBalanced = true, Height = 0 };
            var rightr = node.Right != null ? IsBalanced(node.Right) : new BalanceResult { IsBalanced = true, Height = 0 };
            return new BalanceResult
            {
                IsBalanced = leftr.IsBalanced && rightr.IsBalanced && ((leftr.Height - rightr.Height) <= 1),
                Height = Math.Max(leftr.Height, rightr.Height) + 1
            };
        }
    }

    [TestClass]
    public class Tester
    {
        public static TreeNode balanced = new TreeNode
        {
            Name = "A",

            Left = new TreeNode
            {
                Name = "B",
                Left = new TreeNode
                {
                    Name = "D"
                }
            },

            Right = new TreeNode
            {
                Name = "C",
                Left = new TreeNode
                {
                    Name = "E",
                    Left = new TreeNode
                    {
                        Name = "G"
                    }
                },
                Right = new TreeNode
                {
                    Name = "F"
                }
            }
        };

        public static TreeNode unBalanced = new TreeNode
        {
            Name = "A",

            Left = new TreeNode
            {
                Name = "B",
                Left = new TreeNode
                {
                    Name = "D"
                }
            },

            Right = new TreeNode
            {
                Name = "C",
                Left = new TreeNode
                {
                    Name = "E",
                    Left = new TreeNode
                    {
                        Name = "G"
                    }
                }
            }
        };


        [TestMethod]
        public void BalancedNodeTest()
        {
            var balancedR = MarkedNode.IsBalanced(balanced);
            Assert.IsTrue(balancedR.IsBalanced);
            
            var unbalancedR = MarkedNode.IsBalanced(unBalanced);
            Assert.IsFalse(unbalancedR.IsBalanced);
        }

        [TestMethod]
        public void BalancedEnumTest()
        {
            Assert.IsTrue(MarkedNode.IsBalanced2(balanced));

            Assert.IsFalse(MarkedNode.IsBalanced2(unBalanced));
        }
    }

}
