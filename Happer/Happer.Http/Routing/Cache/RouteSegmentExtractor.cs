﻿using System.Collections.Generic;

namespace Happer.Http.Routing
{
    public class RouteSegmentExtractor
    {
        public IEnumerable<string> Extract(string path)
        {
            var currentSegment = string.Empty;
            var openingParenthesesCount = 0;

            for (var index = 0; index < path.Length; index++)
            {
                var token = path[index];

                if (token.Equals('('))
                {
                    openingParenthesesCount++;
                }

                if (token.Equals(')'))
                {
                    openingParenthesesCount--;
                }

                if (!token.Equals('/') || openingParenthesesCount > 0)
                {
                    currentSegment += token;
                }

                if ((token.Equals('/') || index == path.Length - 1) && currentSegment.Length > 0 && openingParenthesesCount == 0)
                {
                    yield return currentSegment;
                    currentSegment = string.Empty;
                }
            }
        }
    }
}
