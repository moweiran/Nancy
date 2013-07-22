﻿namespace Nancy.Routing.Trie.Nodes
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Linq;

    /// <summary>
    /// A node multiple standard captures combined with a literal e.g. {id}.png.{thing}.{otherthing}
    /// </summary>
    public class CaptureNodeWithMultipleParameters : TrieNode
    {
        private readonly List<string> parameterNames = new List<string>();
        private string builtRegex = "";

        private static readonly Regex MatchRegex = new Regex(@"({?[^{}]*}?)", RegexOptions.Compiled);

        public static bool IsMatch(string segment)
        {
            return MatchRegex.Matches(segment).Cast<Group>().Count(g => g.Value != "") > 1;
        }

        /// <summary>
        /// Score for this node
        /// </summary>
        public override int Score
        {
            get { return 10; }
        }

        public CaptureNodeWithMultipleParameters(TrieNode parent, string segment, ITrieNodeFactory nodeFactory)
            : base(parent, segment, nodeFactory)
        {
            this.ExtractParameterNames();
        }

        /// <summary>
        /// Matches the segment for a requested route
        /// </summary>
        /// <param name="segment">Segment string</param>
        /// <returns>A <see cref="SegmentMatch"/> instance representing the result of the match</returns>
        public override SegmentMatch Match(string segment)
        {
            var match = SegmentMatch.NoMatch;
            var regex = new Regex(builtRegex);

            if (regex.IsMatch(segment))
            {
                match = new SegmentMatch(true);
                var regexMatch = regex.Match(segment);
                for (int i = 1; i < regexMatch.Groups.Count; i++)
                {
                    match.CapturedParameters.Add(parameterNames[i - 1], regexMatch.Groups[i].Value);
                }
            }
            return match;
        }

        /// <summary>
        /// Extracts the parameter name and the literals for the segment
        /// </summary>
        private void ExtractParameterNames()
        {
            var matches = MatchRegex.Matches(this.RouteDefinitionSegment);
            builtRegex += "^";
            foreach (Match match in matches)
            {
                if (match.Value.StartsWith("{") && match.Value.EndsWith("}"))
                {
                    parameterNames.Add(match.Value.Trim('{', '}'));
                    builtRegex += "(.+)";
                }
                else
                {
                    builtRegex += Regex.Escape(match.Value);
                } 
            }
            builtRegex += "$";
        }
    }
}