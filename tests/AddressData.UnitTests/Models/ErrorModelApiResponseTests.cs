namespace AddressData.UnitTests.Models;

using System;
using AddressData.Core.Models.ApiResponse;
using NUnit.Framework;

public class ErrorModelApiResponseTests
{
    [TestCase(typeof(InvalidOperationException), "Unsupported operation")]
    [TestCase(typeof(ArgumentNullException), "Argument cannot be null")]
    [TestCase(typeof(FormatException), "Invalid format")]
    [TestCase(typeof(NullReferenceException), "Object reference not set to an instance of an object")]
    [TestCase(typeof(IndexOutOfRangeException), "Index was outside the bounds of the array")]
    [TestCase(typeof(ArgumentOutOfRangeException), "Specified argument was out of the range of valid values")]
    [TestCase(typeof(DivideByZeroException), "Attempted to divide by zero")]
    [TestCase(typeof(StackOverflowException), "The requested operation caused a stack overflow")]
    [TestCase(typeof(UnauthorizedAccessException), "Access is denied")]
    public void ErrorModelConstructedWithExceptionExceptionPropertiesMatchModelProperties(Type exceptionType, string message)
    {
        // Arrange
        var comparedException = (Exception)Activator.CreateInstance(exceptionType, message);

        // Act
        var errorModel = new ErrorModelApiResponse(comparedException);
        Assert.Multiple(() =>
        {

            // Assert
            Assert.That(errorModel.Type, Is.EqualTo(comparedException.GetType().Name));
            Assert.That(errorModel.Message, Is.EqualTo(comparedException.Message));
            Assert.That(errorModel.StackTrace, Is.EqualTo(comparedException.ToString()));
        });
    }
}
