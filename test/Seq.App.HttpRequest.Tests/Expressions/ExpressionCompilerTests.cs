﻿using System;
using Seq.App.HttpRequest.Expressions;
using Seq.App.HttpRequest.Tests.Support;
using Serilog.Events;
using Xunit;

// ReSharper disable CoVariantArrayConversion

namespace Seq.App.HttpRequest.Tests.Expressions
{
    public class ExpressionCompilerTests
    {
        [Fact]
        public void ExpressionsEvaluateStringEquality()
        {
            AssertEvaluation("Fruit = 'Apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent(),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ComparisonsAreCaseSensitive()
        {
            AssertEvaluation("Fruit = 'Apple'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "APPLE"));
        }

        [Fact]
        public void ExpressionsEvaluateStringContent()
        {
            AssertEvaluation("Fruit like '%pp%'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ExpressionsEvaluateStringPrefix()
        {
            AssertEvaluation("Fruit like 'Ap%'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void ExpressionsEvaluateStringSuffix()
        {
            AssertEvaluation("Fruit like '%le'",
                Some.InformationEvent("Snacking on {Fruit}", "Apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Acerola"));
        }

        [Fact]
        public void LikeIsCaseSensitive()
        {
            AssertEvaluation("Fruit like 'apple'",
                Some.InformationEvent("Snacking on {Fruit}", "apple"),
                Some.InformationEvent("Snacking on {Fruit}", "Apple"));
        }

        [Fact]
        public void ExpressionsEvaluateNumericComparisons()
        {
            AssertEvaluation("Volume > 11",
                Some.InformationEvent("Adding {Volume} L", 11.5),
                Some.InformationEvent("Adding {Volume} L", 11));
        }

        [Fact]
        public void ExpressionsEvaluateWildcardsOnCollectionItems()
        {
            AssertEvaluation("Items[?] like 'C%'",
                Some.InformationEvent("Cart contains {@Items}", new[] { new[] { "Tea", "Coffee" } }), // Test helper doesn't correct this case
                Some.InformationEvent("Cart contains {@Items}", new[] { new[] { "Apricots" } }));
        }

        [Fact]
        public void ExpressionsEvaluateBuiltInProperties()
        {
            AssertEvaluation("@l = 'Information'",
                Some.InformationEvent(),
                Some.WarningEvent());
        }

        [Fact]
        public void ExpressionsEvaluateExistentials()
        {
            AssertEvaluation("AppId is not null",
                Some.InformationEvent("{AppId}", 10),
                Some.InformationEvent("{AppId}", new object?[] {null}),
                Some.InformationEvent());
        }

        [Fact]
        public void ExpressionsLogicalOperations()
        {
            AssertEvaluation("A and B",
                Some.InformationEvent("{A} {B}", true, true),
                Some.InformationEvent("{A} {B}", true, false),
                Some.InformationEvent());
        }

        [Fact]
        public void ExpressionsEvaluateSubproperties()
        {
            AssertEvaluation("Cart.Total > 10",
                Some.InformationEvent("Checking out {@Cart}", new { Total = 20 }),
                Some.InformationEvent("Checking out {@Cart}", new { Total = 5 }));
        }


        [Fact]
        public void SequenceLengthCanBeDetermined()
        {
            AssertEvaluation("length(Items) > 1",
                Some.InformationEvent("Checking out {Items}", new object[] { new[] { "pears", "apples" }}),
                Some.InformationEvent("Checking out {Items}", new object[] { new[] { "pears" }}));
        }

        [Fact]
        public void InMatchesLiterals()
        {
            AssertEvaluation("@l in ['Warning', 'Error']",
                Some.LogEvent(LogEventLevel.Error, "Hello"),
                Some.InformationEvent("Hello"));
        }

        [Fact]
        public void InExaminesSequenceValues()
        {
            AssertEvaluation("5 not in Numbers",
                Some.InformationEvent("{Numbers}", new []{1, 2, 3}),
                Some.InformationEvent("{Numbers}", new [] { 1, 5, 3 }),
                Some.InformationEvent());
        }

        [Theory]
        [InlineData("now(1)", "The function `now` accepts no arguments.")]
        [InlineData("length()", "The function `length` accepts one argument, `value`.")]
        [InlineData("length(1, 2)", "The function `length` accepts one argument, `value`.")]
        [InlineData("round()", "The function `round` accepts two arguments, `number` and `places`.")]
        [InlineData("substring()", "The function `substring` accepts arguments `string`, `startIndex`, and `length` (optional).")]
        public void ReportsArityMismatches(string call, string expectedError)
        {
            // These will eventually be reported gracefully by `TryCompile()`...
            var ex = Assert.Throws<ArgumentException>(() => SerilogExpression.Compile(call));
            Assert.Equal(expectedError, ex.Message);
        }

        static void AssertEvaluation(string expression, LogEvent match, params LogEvent[] noMatches)
        {
            var pred = SerilogExpression.Compile(expression);

            Assert.True(ExpressionResult.IsTrue(pred(match)));
            Assert.All(noMatches, evt => Assert.False(ExpressionResult.IsTrue(pred(evt))));
        }
    }
}
