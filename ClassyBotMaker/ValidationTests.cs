using NUnit.Framework;

namespace ClassyBotMaker.Tests
{
    public class ValidationTests
    {
        private ValidationManager validationManager;

        [SetUp]
        public void Setup()
        {
            var loggingManager = new LoggingManager(LogLevel.Info, @"C:\AiAIBot\logs");  // Correct the order of parameters

            validationManager = new ValidationManager(@"C:\AiAIBot", loggingManager);
        }

        [Test]
        public void TestRunValidationTests()
        {
            bool result = validationManager.RunValidationTests("TestClassName");
            Assert.That(result, Is.True, "Validation should pass.");
        }
    }
}