using Xunit;
using SWP391_FinalProject;
using SWP391_FinalProject.Repository;
using System;

namespace UnitTest
{
    public class UnitTest1
    {
        [Theory]
        [InlineData(10000000, 0.1, 9000000)] // 10% discount on a positive selling price
        [InlineData(0, 0.1, 0)]              // Selling price is 0, any discount should result in 0
        [InlineData(10000000, 0, 10000000)]  // No discount (0%), original selling price should be returned
        [InlineData(10000000, 1, 0)]         // 100% discount, resulting price should be 0
        [InlineData(0, 0, 0)]                // Both selling price and discount are 0, expected result is 0
        [InlineData(0, 1, 0)]                // Selling price is 0 with 100% discount, expected result is 0
        public void CalculatePriceAfterDiscount_ShouldReturnExpectedValue(decimal originalPrice, decimal discountRate, decimal expectedValue)
        {
            // Act
            var actualValue = ProductRepository.CalculatePriceAfterDiscount(originalPrice, discountRate);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Theory]
        [InlineData(-10000000, 0.10)]  // Negative selling price, should throw exception
        [InlineData(10000000, -0.1)]   // Negative discount, should throw exception
        [InlineData(10000000, 1.5)]    // Discount greater than 1 (100%), should throw exception
        [InlineData(-10000000, 1.5)]   // Both selling price is negative and discount > 1, should throw exception
        [InlineData(0, -0.1)]          // Selling price is 0 but discount is negative, should throw exception
        public void CalculatePriceAfterDiscount_ShouldThrowArgumentException(decimal originalPrice, decimal discountRate)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => ProductRepository.CalculatePriceAfterDiscount(originalPrice, discountRate));
        }
    }
}
