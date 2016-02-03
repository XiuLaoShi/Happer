using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Happer.Http.Routing.Trie.Nodes;

namespace Happer.Http.Routing.Trie
{
    public class RouteResolverTrie
    {
        private readonly TrieNodeFactory nodeFactory;
        private readonly IDictionary<string, TrieNode> routeTries = new Dictionary<string, TrieNode>();
        private static char[] splitSeparators = new[] {'/'};

        public RouteResolverTrie(TrieNodeFactory nodeFactory)
        {
            this.nodeFactory = nodeFactory;
        }

        public void BuildTrie(RouteCache cache)
        {
            foreach (var cacheItem in cache)
            {
                var moduleKey = cacheItem.Key;
                var routeDefinitions = cacheItem.Value;

                foreach (var routeDefinition in routeDefinitions)
                {
                    var routeIndex = routeDefinition.Item1;
                    var routeDescription = routeDefinition.Item2;

                    TrieNode trieNode;
                    if (!this.routeTries.TryGetValue(routeDescription.Method, out trieNode))
                    {
                        trieNode = this.nodeFactory.GetNodeForSegment(null, null);

                        this.routeTries.Add(routeDefinition.Item2.Method, trieNode);
                    }

                    var segments = routeDefinition.Item2.Segments.ToArray();

                    trieNode.Add(segments, moduleKey, routeIndex, routeDescription);
                }
            }
        }

        public MatchResult[] GetMatches(string method, string path, Context context)
        {
            if (string.IsNullOrEmpty(path))
            {
                return MatchResult.NoMatches;
            }

            // TODO -concurrent if allowing updates?
            if (!this.routeTries.ContainsKey(method))
            {
                return MatchResult.NoMatches;
            }

            return this.routeTries[method].GetMatches(path.Split(splitSeparators, StringSplitOptions.RemoveEmptyEntries), context)
                                          .ToArray();
        }

        public IEnumerable<string> GetOptions(string path, Context context)
        {
            foreach (var method in this.routeTries.Keys)
            {
                if (this.GetMatches(method, path, context).Any())
                {
                    yield return method;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var kvp in this.routeTries)
            {
                var method = kvp.Key;
                sb.Append(
                    kvp.Value.GetRoutes().Select(s => method + " " + s)
                             .Aggregate((r1, r2) => r1 + "\n" + r2));
            }

            return sb.ToString();
        }
    }
}