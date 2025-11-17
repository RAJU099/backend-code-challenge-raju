using FluentAssertions;
using Moq;
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;
using CodeChallenge.Api.Logic;

namespace CodeChallenge.Tests
{
    public class AnnouncementLogicTests
    {
        private readonly Mock<IMessageRepository> _messageRepo;
        private readonly MessageLogic _announcementLogic;
        private readonly Guid _tenantKey;

        public AnnouncementLogicTests()
        {
            _messageRepo = new Mock<IMessageRepository>();
            _announcementLogic = new MessageLogic(_messageRepo.Object);
            _tenantKey = Guid.NewGuid();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldSucceed_WhenDataIsValid()
        {
            var request = new CreateMessageRequest
            {
                Title = "Welcome Note",
                Content = "This is a sample announcement body."
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, request.Title))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                        .ReturnsAsync(new Message());

            var result = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldReturnConflict_WhenTitleExists()
        {
            var request = new CreateMessageRequest
            {
                Title = "ExistingHeading",
                Content = "Sample text here."
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, request.Title))
                        .ReturnsAsync(new Message { Title = "ExistingHeading" });

            var result = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldReturnValidationError_WhenContentTooShort()
        {
            var request = new CreateMessageRequest
            {
                Title = "CorrectTitle",
                Content = "bad"
            };

            var result = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task ModifyAnnouncementAsync_ShouldReturnNotFound_WhenRecordMissing()
        {
            var request = new UpdateMessageRequest
            {
                Title = "Edited",
                Content = "Updated body",
                IsActive = true
            };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, It.IsAny<Guid>()))
                        .ReturnsAsync((Message?)null);

            var result = await _announcementLogic.UpdateMessageAsync(_tenantKey, Guid.NewGuid(), request);

            result.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task ModifyAnnouncementAsync_ShouldFailValidation_WhenRecordIsInactive()
        {
            var inactive = new Message { IsActive = false };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, It.IsAny<Guid>()))
                        .ReturnsAsync(inactive);

            var request = new UpdateMessageRequest
            {
                Title = "EditedTitle",
                Content = "Updated body text",
                IsActive = true
            };

            var result = await _announcementLogic.UpdateMessageAsync(_tenantKey, Guid.NewGuid(), request);

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task RemoveAnnouncementAsync_ShouldReturnNotFound_WhenRecordMissing()
        {
            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, It.IsAny<Guid>()))
                        .ReturnsAsync((Message?)null);

            var response = await _announcementLogic.DeleteMessageAsync(_tenantKey, Guid.NewGuid());

            response.Should().BeOfType<NotFound>();
        }

        [Fact]
        public async Task RemoveAnnouncementAsync_ShouldReturnValidationError_WhenInactive()
        {
            var inactive = new Message { IsActive = false };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, It.IsAny<Guid>()))
                        .ReturnsAsync(inactive);

            var result = await _announcementLogic.DeleteMessageAsync(_tenantKey, Guid.NewGuid());

            result.Should().BeOfType<ValidationError>();
        }

        [Fact]
        public async Task ModifyAnnouncementAsync_ShouldReturnConflict_WhenDuplicateTitleFound()
        {
            var duplicateTitleRecord = new Message
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                Title = "BusyTitle"
            };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, It.IsAny<Guid>()))
                        .ReturnsAsync(new Message { Id = Guid.NewGuid(), IsActive = true });

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, "BusyTitle"))
                        .ReturnsAsync(duplicateTitleRecord);

            var request = new UpdateMessageRequest
            {
                Title = "BusyTitle",
                Content = "Valid change",
                IsActive = true
            };

            var result = await _announcementLogic.UpdateMessageAsync(_tenantKey, Guid.NewGuid(), request);

            result.Should().BeOfType<Conflict>();
        }

        [Fact]
        public async Task ModifyAnnouncementAsync_ShouldReturnUpdated_WhenValid()
        {
            var messageId = Guid.NewGuid();

            var existing = new Message
            {
                Id = messageId,
                OrganizationId = _tenantKey,
                Title = "OldHeading",
                Content = "Old message content",
                IsActive = true
            };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, messageId))
                        .ReturnsAsync(existing);

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, "FreshHeading"))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.UpdateAsync(It.IsAny<Message>()))
                        .ReturnsAsync(new Message());

            var request = new UpdateMessageRequest
            {
                Title = "FreshHeading",
                Content = "Updated content body",
                IsActive = true
            };

            var result = await _announcementLogic.UpdateMessageAsync(_tenantKey, messageId, request);

            result.Should().BeOfType<Updated>();
        }

        [Fact]
        public async Task RemoveAnnouncementAsync_ShouldReturnDeleted_WhenValid()
        {
            var messageId = Guid.NewGuid();

            var stored = new Message
            {
                Id = messageId,
                OrganizationId = _tenantKey,
                Title = "Cleanup",
                Content = "Delete me",
                IsActive = true
            };

            _messageRepo.Setup(r => r.GetByIdAsync(_tenantKey, messageId))
                        .ReturnsAsync(stored);

            _messageRepo.Setup(r => r.DeleteAsync(_tenantKey, messageId))
                        .ReturnsAsync(true);

            var result = await _announcementLogic.DeleteMessageAsync(_tenantKey, messageId);

            result.Should().BeOfType<Deleted>();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldPass_WhenTitleLengthIs3()
        {
            var request = new CreateMessageRequest
            {
                Title = new string('A', 3),
                Content = new string('B', 12)
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, request.Title))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                        .ReturnsAsync(new Message());

            var result = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldPass_WhenContentLengthIs10()
        {
            var request = new CreateMessageRequest
            {
                Title = "Alpha",
                Content = new string('X', 10)
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, request.Title))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                        .ReturnsAsync(new Message());

            var result = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldPass_WhenTitleLengthIs200()
        {
            var request = new CreateMessageRequest
            {
                Title = new string('T', 200),
                Content = "Sufficient content"
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, request.Title))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                        .ReturnsAsync(new Message());

            var result = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            result.Should().BeOfType<Created<Message>>();
        }

        [Fact]
        public async Task AddAnnouncementAsync_ShouldPass_WhenContentLengthIs1000()
        {
            var request = new CreateMessageRequest
            {
                Title = "LongContentCase",
                Content = new string('C', 1000)
            };

            _messageRepo.Setup(r => r.GetByTitleAsync(_tenantKey, request.Title))
                        .ReturnsAsync((Message?)null);

            _messageRepo.Setup(r => r.CreateAsync(It.IsAny<Message>()))
                        .ReturnsAsync(new Message());

            var response = await _announcementLogic.CreateMessageAsync(_tenantKey, request);

            response.Should().BeOfType<Created<Message>>();
        }
    }
}
