using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using CodeChallenge.Api.Logic;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Tests
{
    public class MessageLogicTests
    {
        private readonly Mock<IMessageRepository> _messageRepo;
        private readonly MessageLogic _messageLogic;
        private readonly Guid _tenantId;

        public MessageLogicTests()
        {
            _messageRepo = new Mock<IMessageRepository>();
            _messageLogic = new MessageLogic(_messageRepo.Object);
            _tenantId = Guid.NewGuid();
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnCreated_WhenInputIsValid()
        {
            var payload = new CreateMessageRequest
            {
                Title = "Hello World",
                Content = "This message content meets the required length."
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantId, payload.Title))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                        .ReturnsAsync((Message m) => m);

            var outcome = await _messageLogic.CreateMessageAsync(_tenantId, payload);

            outcome.Should().BeOfType<Created<Message>>();
            var created = (Created<Message>)outcome;
            created.Value.Title.Should().Be(payload.Title);
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnConflict_WhenTitleIsTaken()
        {
            var payload = new CreateMessageRequest
            {
                Title = "ExistingTitle",
                Content = "Some proper content goes here."
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantId, payload.Title))
                        .ReturnsAsync(new Message { Title = "ExistingTitle" });

            var outcome = await _messageLogic.CreateMessageAsync(_tenantId, payload);

            outcome.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task CreateMessage_ShouldReturnValidationError_WhenContentTooShort()
        {
            var payload = new CreateMessageRequest
            {
                Title = "Sample Title",
                Content = "tiny"
            };

            var result = await _messageLogic.CreateMessageAsync(_tenantId, payload);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnNotFound_WhenMessageMissing()
        {
            var updateData = new UpdateMessageRequest
            {
                Title = "Updated Title",
                Content = "Updated details for this message.",
                IsActive = true
            };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>()))
                        .ReturnsAsync((Message?)null);

            var result = await _messageLogic.UpdateMessageAsync(_tenantId, Guid.NewGuid(), updateData);

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnValidationError_WhenMessageIsInactive()
        {
            var stored = new Message { IsActive = false };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>()))
                        .ReturnsAsync(stored);

            var updateData = new UpdateMessageRequest
            {
                Title = "Rename",
                Content = "Updating the text content here.",
                IsActive = true
            };

            var outcome = await _messageLogic.UpdateMessageAsync(_tenantId, Guid.NewGuid(), updateData);

            outcome.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            _messageRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>()))
                        .ReturnsAsync((Message?)null);

            var outcome = await _messageLogic.DeleteMessageAsync(_tenantId, Guid.NewGuid());

            outcome.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task DeleteMessage_ShouldReturnValidationError_WhenItemInactive()
        {
            var storedMessage = new Message { IsActive = false };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>()))
                        .ReturnsAsync(storedMessage);

            var outcome = await _messageLogic.DeleteMessageAsync(_tenantId, Guid.NewGuid());

            outcome.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task UpdateMessage_ShouldReturnConflict_WhenTitleBelongsToAnotherRecord()
        {
            var existingDifferentMsg = new Message
            {
                Id = Guid.NewGuid(),
                Title = "ClashingTitle",
                IsActive = true
            };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantId, It.IsAny<Guid>()))
                        .ReturnsAsync(new Message
                        {
                            Id = Guid.NewGuid(),
                            IsActive = true
                        });

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantId, "ClashingTitle"))
                        .ReturnsAsync(existingDifferentMsg);

            var updateRequest = new UpdateMessageRequest
            {
                Title = "ClashingTitle",
                Content = "Valid body content for update.",
                IsActive = true
            };

            var result = await _messageLogic.UpdateMessageAsync(_tenantId, Guid.NewGuid(), updateRequest);

            result.Should().BeOfType<Conflict>();
        }
    }
}
