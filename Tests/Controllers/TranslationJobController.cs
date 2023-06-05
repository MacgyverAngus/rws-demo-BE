using DomainObjects;
using External.ThirdParty.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Repositories;
using System.ComponentModel.DataAnnotations;
using Tests.Helpers;
using TranslationManagement.Api.Controlers;
using TranslationManagement.Api.Controllers;
using TranslationManagement.Api.DataContracts;

namespace Tests.Controllers
{
    [TestFixture]
    public class Tests
    {
        private Mock<AppDbContext> _contextMock;
        private Mock<IServiceScopeFactory> _scopeFactoryMock;
        private Mock<IServiceProvider> _serviceProviderMock;
        private Mock<IValidator<CreateTranslationJobDto>> _createTranslationJobValidatorMock;
        private Mock<ITranslationJobRepository> _translationJobRepositoryMock;
        private Mock<ILogger<TranslatorManagementController>> _loggerMock;
        private TranslationJobController _controller;

        [SetUp]
        public void SetupBeforeEachTest()
        {
            //DBContext
            _contextMock = new Mock<AppDbContext>();
            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _createTranslationJobValidatorMock = new Mock<IValidator<CreateTranslationJobDto>>();
            _translationJobRepositoryMock = new Mock<ITranslationJobRepository>();
            _loggerMock = new Mock<ILogger<TranslatorManagementController>>();
            _scopeFactoryMock.Setup(factory => factory.CreateScope()).Returns(Mock.Of<IServiceScope>());
            _scopeFactoryMock.Setup(factory => factory.CreateScope().ServiceProvider).Returns(_serviceProviderMock.Object);

            _controller = new TranslationJobController(
                _scopeFactoryMock.Object,
                _createTranslationJobValidatorMock.Object,
                _translationJobRepositoryMock.Object,
                _loggerMock.Object);
        }


        [Test]
        public void TranslationJobController_Constructor_InitializesDependencies()
        {
            Assert.IsInstanceOf<TranslationJobController>(_controller);
        }

        [Test]
        public void CreateJob_ValidInput_ReturnsCreatedResult()
        {
            // Arrange
            var jobDto = new CreateTranslationJobDto
            {
                CustomerName = "John Doe",
                OriginalContent = "Original content",
                TranslatedContent = "Translated content"
            };

            var createJobValidatorMock = new Mock<IValidator<CreateTranslationJobDto>>();
            createJobValidatorMock.Setup(validator => validator.Validate(It.IsAny<CreateTranslationJobDto>())).Returns(new FluentValidation.Results.ValidationResult());

            var translationJobRepositoryMock = new Mock<ITranslationJobRepository>();
            translationJobRepositoryMock.Setup(repo => repo.SaveTranslationJob(It.IsAny<TranslationJob>()));

            var notificationServiceMock = new Mock<INotificationService>();
            notificationServiceMock.Setup(service => service.SendNotification(It.IsAny<string>())).ReturnsAsync(true);

            
            // Act
            var result = _controller.CreateJob(jobDto);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            translationJobRepositoryMock.Verify(repo => repo.SaveTranslationJob(It.IsAny<TranslationJob>()), Times.Once);
            notificationServiceMock.Verify(service => service.SendNotification(It.IsAny<string>()), Times.Once);
        }
    }
}