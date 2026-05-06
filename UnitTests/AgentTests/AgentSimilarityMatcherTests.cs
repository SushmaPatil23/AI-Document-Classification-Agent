using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Agent;
using Agent.Similarity;
using Classifier.Embeddings;

namespace MyDocClassifierAgentUnitTests
{
    /// <summary>
    /// Tests for AgentSimilarityMatcher to verify class ranking behavior.
    /// </summary>
    [TestClass]
    public class AgentSimilarityMatcherTests
    {
        /// <summary>
        /// Creates a matcher using cosine similarity.
        /// </summary>
        private AgentSimilarityMatcher CreateMatcher()
        {
            return new AgentSimilarityMatcher(new CosineSimilarityMetric());
        }

        /// <summary>
        /// Verifies that classes are ranked by highest similarity.
        /// </summary>
        [TestMethod]
        public void RankClasses_ShouldOrderByHighestSimilarity()
        {
            var matcher = CreateMatcher();

            var intent = new DocumentEmbedding
            {
                FileName = "intent",
                Embedding = new float[] { 1, 0 }
            };

            var centroids = new List<ClassCentroid>
            {
                new ClassCentroid
                {
                    ClassName = "A",
                    Embedding = new float[] { 1, 0 }
                },
                new ClassCentroid
                {
                    ClassName = "B",
                    Embedding = new float[] { 0, 1 }
                }
            };

            var result = matcher.RankClasses(intent, centroids);

            Assert.AreEqual("A", result[0].ClassName);
            Assert.AreEqual("B", result[1].ClassName);
        }

        /// <summary>
        /// Verifies that an exception is thrown when intent is null.
        /// </summary>
        [TestMethod]
        public void RankClasses_ShouldThrow_WhenIntentIsNull()
        {
            var matcher = CreateMatcher();
            var centroids = new List<ClassCentroid>();

            Assert.ThrowsExactly<ArgumentNullException>(() =>
            matcher.RankClasses(null!, centroids));
        }

        /// <summary>
        /// Verifies that only the top N classes are returned.
        /// </summary>
        [TestMethod]
        public void RankTopClasses_ShouldReturnOnlyRequestedNumberOfClasses_WhenTopNIsSpecified()
        {
            var matcher = CreateMatcher();

            var intent = new DocumentEmbedding
            {
                FileName = "intent",
                Embedding = new float[] { 1, 0 }
            };

            var centroids = new List<ClassCentroid>
            {
                new ClassCentroid
                {
                    ClassName = "A",
                    Embedding = new float[] { 1, 0 }
                },
                new ClassCentroid
                {
                    ClassName = "B",
                    Embedding = new float[] { 0, 1 }
                }
            };

            var result = matcher.RankTopClasses(intent, centroids, 1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("A", result[0].ClassName);
        }

        /// <summary>
        /// Verifies that an empty list is returned when no centroids exist.
        /// </summary>
        [TestMethod]
        public void RankClasses_ShouldReturnEmpty_WhenCentroidsListIsEmpty()
        {
            var matcher = CreateMatcher();

            var intent = new DocumentEmbedding
            {
                FileName = "intent",
                Embedding = new float[] { 1, 0 }
            };

            var centroids = new List<ClassCentroid>();

            var result = matcher.RankClasses(intent, centroids);

            Assert.AreEqual(0, result.Count);
        }
    }
}