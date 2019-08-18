using Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TextProcessing.Tests
{
    public class CountVectorizerTests
    {
        [Fact]
        public void CanFit()
        {
            // arrange
            var target = new CountVectorizer();
            var trainingData = new string[]
            {
                "Some cool text",
                "Another cool text"
            };

            // act
            var res = target.Fit(trainingData);

            // assert
            Assert.Same(target, res);
            Assert.NotNull(res.Vocabulary);
        }
                
        [Fact]
        public void ThrowExceptionOnTransformIfIsNotFitted()
        {
            // arrange 
            var target = new CountVectorizer();

            // act & assert
            Assert.Throws<NotFittedException>(() => target.Transform("some test text"));
        }

        [Fact]
        public void CanFitTransformData()
        {
            // arrange
            var target = new CountVectorizer();
            var trainingData = GetTrainingData();

            var expected = new List<IDictionary<string, uint>>
            {
                new Dictionary<string, uint>
                {
                    { "somebody", 1 },
                    { "once", 1 },
                    { "told", 1 },
                    { "me", 2 },
                    { "the", 1 },
                    { "world", 1 },
                    { "is", 1 },
                    { "gonna", 1 },
                    { "roll", 1 },
                    { "i", 0 },
                    { "ain't", 0 },
                    { "sharpest", 0 },
                    { "tool", 0 },
                    { "in", 0 },
                    { "shed", 0 }
                },
                new Dictionary<string, uint>
                {
                    { "i", 1 },
                    { "ain't", 1 },
                    { "the", 2 },
                    { "sharpest", 1 },
                    { "tool", 1 },
                    { "in", 1 },
                    { "shed", 1 },
                    { "somebody", 0 },
                    { "once", 0 },
                    { "told", 0 },
                    { "me", 0 },
                    { "world", 0 },
                    { "is", 0 },
                    { "gonna", 0 },
                    { "roll", 0 }
                },
            };

            // act 
            var res = target.FitTransform(trainingData);

            // assert
            Assert.NotNull(res);
            Assert.Equal(expected, res);
        }

        [Fact]
        public void ExtractOnlyFeaturesThatAreInVocabulary()
        {
            // arrange
            var trainingData = GetTrainingData();
            var testData = new string[]
            {
                "Somebody once asked could I spare some change for gas?",
                "I need to get myself away from this place"
            };

            var expected = new List<IDictionary<string, uint>>
            {
                new Dictionary<string, uint>
                {
                    { "somebody", 1 },
                    { "once", 1 },
                    { "told", 0 },
                    { "me", 0 },
                    { "the", 0 },
                    { "world", 0 },
                    { "is", 0 },
                    { "gonna", 0 },
                    { "roll", 0 },
                    { "i", 1 },
                    { "ain't", 0 },
                    { "sharpest", 0 },
                    { "tool", 0 },
                    { "in", 0 },
                    { "shed", 0 }
                },
                new Dictionary<string, uint>
                {
                    { "i", 1 },
                    { "ain't", 0 },
                    { "the", 0 },
                    { "sharpest", 0 },
                    { "tool", 0 },
                    { "in", 0 },
                    { "shed", 0 },
                    { "somebody", 0 },
                    { "once", 0 },
                    { "told", 0 },
                    { "me", 0 },
                    { "world", 0 },
                    { "is", 0 },
                    { "gonna", 0 },
                    { "roll", 0 }
                },
            };

            var target = GetFittedVectorizer(trainingData);

            // act 
            var res = target.Transform(testData);

            // assert
            Assert.NotNull(res);
            Assert.Equal(expected, res);
        }

        private CountVectorizer GetFittedVectorizer(IEnumerable<string> trainingData) =>
            new CountVectorizer().Fit(trainingData);

        [Fact]
        public void CanLimitFeaturesCount()
        {
            // arrange
            var settings = new CountVectorizerSettings
            {
                MaxFeatures = 5
            };

            var target = new CountVectorizer(settings);
            var trainingData = GetTrainingData();

            // act
            target.Fit(trainingData);

            // assert
            Assert.True(target.Vocabulary.Count() == 5);
        }

        [Fact]
        public void SortFeaturesByCountWhenApplyingCountLimit()
        {
            // arrange
            var settings = new CountVectorizerSettings
            {
                MaxFeatures = 5
            };

            var target = new CountVectorizer(settings);
            var trainingData = GetTrainingData();

            target.Fit(trainingData);

            var vectorizerWithoutLimit = GetFittedVectorizer(trainingData);
            IEnumerable<uint> getTopCounts(IEnumerable<IDictionary<string, uint>> tokensCounts) =>
                tokensCounts.SelectMany(kv => kv.Values)
                    .OrderByDescending(v => v)
                    .Take((int)settings.MaxFeatures);

            var expectedTopCounts = getTopCounts(vectorizerWithoutLimit.Transform(trainingData));

            // act
            var result = target.Transform(trainingData);

            // assert
            var resultTopCounts = getTopCounts(result);
            Assert.Equal(expectedTopCounts, resultTopCounts);
        }

        private IEnumerable<string> GetTrainingData() =>
            new string[]
            {
                "Somebody once told me the world is gonna roll me",
                "I ain't the sharpest tool in the shed"
            };
    }
}
