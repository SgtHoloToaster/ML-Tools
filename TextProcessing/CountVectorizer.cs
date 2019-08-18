using Common.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TextProcessing
{
    public class CountVectorizer
    {
        readonly (byte, byte) _ngramRange;
        readonly IEnumerable<string> _stopWords;
        readonly int? _maxFeatures;
        readonly Regex _wordPattern;
        IDictionary<string, uint> _defaultTokensFrequency;
        HashSet<string> _vocabulary;

        private IDictionary<string, uint> GetDefaultTokensCount()
        {
            if (_defaultTokensFrequency == null)
                _defaultTokensFrequency = _vocabulary.ToDictionary<string, string, uint>(v => v, v => 0);

            return new Dictionary<string, uint>(_defaultTokensFrequency);
        }

        public HashSet<string> Vocabulary {
            get => _vocabulary;
            private set
            {
                _defaultTokensFrequency = null;
                _vocabulary = value;
            }
        }

        public CountVectorizer() : this(new CountVectorizerSettings()) { }

        public CountVectorizer(CountVectorizerSettings settings)
        {
            _ngramRange = settings.NgramRange;
            _stopWords = settings.StopWords;
            _maxFeatures = settings.MaxFeatures;
            _wordPattern = settings.WordPattern;
            _vocabulary = settings.Vocabulary;
        }

        public CountVectorizer Fit(IEnumerable<string> data)
        {
            var tokens = new List<string>();
            foreach(var text in data)
            {
                tokens.AddRange(Tokenize(text));
            }

            var tokensCounts = tokens.GroupBy(t => t).ToDictionary(g => g.Key, g => (uint)g.Count());
            if (_maxFeatures > 0)
                tokensCounts = LimitFeatures(tokensCounts, _maxFeatures.Value);

            _vocabulary = new HashSet<string>(tokensCounts.Keys);
            return this;
        }

        private Dictionary<string, uint> LimitFeatures(IDictionary<string, uint> tokensCounts, int maxFeatures) =>
            tokensCounts.OrderByDescending(kv => kv.Value)
                .Take(maxFeatures)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

        private IEnumerable<string> Tokenize(string text) =>
            _wordPattern.Matches(text.ToLower())
                    .Cast<Match>()
                    .Select(m => m.Value);

        private IDictionary<string, uint> CountTokens(IEnumerable<string> tokens)
        {
            var tokensCount = GetDefaultTokensCount();
            foreach (var token in tokens)
            {
                if (tokensCount.ContainsKey(token))
                    tokensCount[token]++;
            }

            return tokensCount;
        }

        public IEnumerable<IDictionary<string, uint>> FitTransform(IEnumerable<string> data) =>
            Fit(data).Transform(data);

        public IEnumerable<IDictionary<string, uint>> Transform(IEnumerable<string> data)
        {
            CheckVocabulary();
            var result = new List<IDictionary<string, uint>>();
            foreach (var text in data)
            {
                result.Add(TransformInternal(text));
            }

            return result;
        }

        public IDictionary<string, uint> Transform(string text)
        {
            CheckVocabulary();
            return TransformInternal(text);
        }

        private IDictionary<string, uint> TransformInternal(string text) =>
            CountTokens(Tokenize(text));

        private void CheckVocabulary()
        {
            if (_vocabulary == null)
                throw new NotFittedException();
        }
    }
}
