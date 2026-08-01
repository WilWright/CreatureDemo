using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using Utils;

namespace Navigation
{
    public class NavigationMap
    {
        readonly struct NavigationResult
        {
            readonly Action<NavigationPath> _onReady;
            readonly NavigationPath _path;

            public NavigationResult(Action<NavigationPath> onReady, NavigationPath path)
            {
                _onReady = onReady;
                _path    = path;
            }

            public readonly void OnReady()
            {
                _onReady?.Invoke(_path);
            }
        }

        readonly NavigationPathSearch[] _availableSearches;
        int _currentAvailableSearchIndex;

        readonly Queue<NavigationResult> _results = new();

        const int DEFAULT_AVAILABLE_SEARCH_COUNT = 3;

        public NavigationMap(NavigationGraph graph, int availableSearchCount = DEFAULT_AVAILABLE_SEARCH_COUNT)
        {
            _availableSearches = new NavigationPathSearch[availableSearchCount];
            for (int i = 0; i < availableSearchCount; i++)
            {
                _availableSearches[i] = new NavigationPathSearch(graph);
            }

            GameController.OnMainThreadUpdate.AddListener(CheckNavigationResults);
        }

        public void Unload()
        {
            foreach (var search in _availableSearches)
            {
                search.Unload();
            }
        }

        public static Coordinates3D GetNodeId(Vector3 worldPosition, Vector3 origin, float nodeSize)
        {
            var pos = (worldPosition - origin) / nodeSize;
            var c = CoordinatesUtils.FromVector3Floor(pos);
            c.y = (int)Math.Round(pos.y, MidpointRounding.AwayFromZero);
            return c;
        }

        public CancellationTokenSource RequestNavigationPath(Vector3 from, Vector3 to, Action<NavigationPath> onReady)
        {
            return GetAvailableSearch().RequestNavigationPath(from, to,
                (path) =>
                {
                    _results.Enqueue(new NavigationResult(onReady, path));
                }
            );
        }

        public void CheckNavigationResults()
        {
            while (_results.TryDequeue(out var result))
            {
                result.OnReady();
            }
        }

        NavigationPathSearch GetAvailableSearch()
        {
            if (++_currentAvailableSearchIndex >= _availableSearches.Length)
            {
                _currentAvailableSearchIndex = 0;
            }

            return _availableSearches[_currentAvailableSearchIndex];
        }
    }
}
