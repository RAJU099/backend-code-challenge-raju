Logic\MessageLogic.cs
using CodeChallenge.Api.Models;
using CodeChallenge.Api.Repositories;

namespace CodeChallenge.Api.Logic;

public class MessageLogic : IMessageLogic
{
    private readonly IMessageRepository _repository;

    public MessageLogic(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> CreateMessageAsync(Guid organizationId, CreateMessageRequest request)
    {
        var errors = new Dictionary<string, List<string>>();

        var title = request.Title?.Trim() ?? string.Empty;
        var content = request.Content?.Trim() ?? string.Empty;

        // Title validations
        if (string.IsNullOrWhiteSpace(title))
        {
            AddError(errors, "Title", "Title is required.");
        }
        else
        {
            if (title.Length < 3 || title.Length > 200)
                AddError(errors, "Title", "Title must be between 3 and 200 characters.");
        }

        // Content validations
        if (string.IsNullOrWhiteSpace(content) || content.Length < 10 || content.Length > 1000)
        {
            AddError(errors, "Content", "Content must be between 10 and 1000 characters.");
        }

        if (errors.Any())
        {
            return new ValidationError(errors.ToDictionary(k => k.Key, v => v.Value.ToArray()));
        }

        // Uniqueness
        var existingByTitle = await _repository.GetByTitleAsync(organizationId, title);
        if (existingByTitle is not null)
        {
            return new Conflict("A message with the same title already exists for this organization.");
        }

        var message = new Message
        {
            OrganizationId = organizationId,
            Title = title,
            Content = content,
            IsActive = true
        };

        var created = await _repository.CreateAsync(message);
        return new Created<Message>(created);
    }

    public async Task<Result> UpdateMessageAsync(Guid organizationId, Guid id, UpdateMessageRequest request)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing is null)
        {
            return new NotFound("Message not found.");
        }

        if (!existing.IsActive)
        {
            return new ValidationError(new Dictionary<string, string[]> { { "IsActive", new[] { "Can only update active messages." } } });
        }

        var errors = new Dictionary<string, List<string>>();

        // Determine provided updates (treat empty/whitespace as "no change")
        var newTitleRaw = request.Title?.Trim() ?? string.Empty;
        var newContentRaw = request.Content?.Trim() ?? string.Empty;
        var titleProvided = !string.IsNullOrWhiteSpace(newTitleRaw);
        var contentProvided = !string.IsNullOrWhiteSpace(newContentRaw);

        if (titleProvided)
        {
            if (newTitleRaw.Length < 3 || newTitleRaw.Length > 200)
            {
                AddError(errors, "Title", "Title must be between 3 and 200 characters.");
            }
            else if (!string.Equals(newTitleRaw, existing.Title, StringComparison.OrdinalIgnoreCase))
            {
                var conflict = await _repository.GetByTitleAsync(organizationId, newTitleRaw);
                if (conflict is not null && conflict.Id != existing.Id)
                    return new Conflict("A message with the same title already exists for this organization.");
            }
        }

        if (contentProvided)
        {
            if (newContentRaw.Length < 10 || newContentRaw.Length > 1000)
            {
                AddError(errors, "Content", "Content must be between 10 and 1000 characters.");
            }
        }

        if (errors.Any())
            return new ValidationError(errors.ToDictionary(k => k.Key, v => v.Value.ToArray()));

        // Apply updates
        if (titleProvided)
        {
            existing.Title = newTitleRaw;
        }
        if (contentProvided)
        {
            existing.Content = newContentRaw;
        }

        // Allow toggling IsActive as part of update (rule: can only update when currently active)
        existing.IsActive = request.IsActive;

        // Set UpdatedAt automatically
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        if (updated is null)
        {
            return new NotFound("Message not found during update.");
        }

        return new Updated();
    }

    public async Task<Result> DeleteMessageAsync(Guid organizationId, Guid id)
    {
        var existing = await _repository.GetByIdAsync(organizationId, id);
        if (existing is null)
        {
            return new NotFound("Message not found.");
        }

        if (!existing.IsActive)
        {
            return new ValidationError(new Dictionary<string, string[]> { { "IsActive", new[] { "Can only delete active messages." } } });
        }

        var deleted = await _repository.DeleteAsync(organizationId, id);
        if (!deleted)
        {
            return new NotFound("Message could not be deleted.");
        }

        return new Deleted();
    }

    public Task<Message?> GetMessageAsync(Guid organizationId, Guid id)
        => _repository.GetByIdAsync(organizationId, id);

    public Task<IEnumerable<Message>> GetAllMessagesAsync(Guid organizationId)
        => _repository.GetAllByOrganizationAsync(organizationId);

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var list))
        {
            list = new List<string>();
            errors[key] = list;
        }
        list.Add(message);
    }
}