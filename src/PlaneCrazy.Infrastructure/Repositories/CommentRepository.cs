using PlaneCrazy.Domain.Entities;
using PlaneCrazy.Domain.Commands;

namespace PlaneCrazy.Infrastructure.Repositories;

public class CommentRepository : JsonFileRepository<Comment>
{
    public CommentRepository() 
        : base("comments.json")
    {
    }

    protected override string GetEntityId(Comment entity) => entity.Id.ToString();
    
    /// <summary>
    /// Gets all comments for a specific entity, ordered by creation date descending.
    /// </summary>
    public async Task<IEnumerable<Comment>> GetByEntityAsync(string entityType, string entityId)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.EntityType == entityType && c.EntityId == entityId)
                  .OrderByDescending(c => c.CreatedAt);
    }
    
    /// <summary>
    /// Gets only active (non-deleted) comments for a specific entity.
    /// </summary>
    public async Task<IEnumerable<Comment>> GetActiveByEntityAsync(string entityType, string entityId)
    {
        var all = await GetAllAsync();
        return all.Where(c => c.EntityType == entityType && 
                             c.EntityId == entityId && 
                             !c.IsDeleted)
                  .OrderByDescending(c => c.CreatedAt);
    }
    
    /// <summary>
    /// Adds a new comment to an entity.
    /// </summary>
    /// <param name="entityType">The type of entity to comment on.</param>
    /// <param name="entityId">The identifier of the entity to comment on.</param>
    /// <param name="text">The text content of the comment.</param>
    /// <param name="user">The user adding the comment (optional).</param>
    /// <returns>The GUID of the newly created comment.</returns>
    public async Task<Guid> AddCommentAsync(string entityType, string entityId, string text, string? user = null)
    {
        var comment = new Comment
        {
            EntityType = entityType,
            EntityId = entityId,
            Text = text,
            CreatedBy = user,
            IsDeleted = false
        };
        
        await SaveAsync(comment);
        return comment.Id;
    }
    
    /// <summary>
    /// Edits an existing comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to edit.</param>
    /// <param name="entityType">The type of entity the comment is on.</param>
    /// <param name="entityId">The identifier of the entity the comment is on.</param>
    /// <param name="newText">The new text for the comment.</param>
    /// <param name="user">The user editing the comment (optional).</param>
    /// <param name="previousText">The previous text (optional, for audit trail).</param>
    public async Task EditCommentAsync(Guid commentId, string entityType, string entityId, string newText, string? user = null, string? previousText = null)
    {
        var comment = await GetByIdAsync(commentId.ToString());
        
        if (comment == null)
            throw new InvalidOperationException($"Comment with ID {commentId} not found.");
        
        if (comment.EntityType != entityType || comment.EntityId != entityId)
            throw new InvalidOperationException($"Comment {commentId} does not belong to entity {entityType}/{entityId}.");
        
        comment.Text = newText;
        comment.UpdatedAt = DateTime.UtcNow;
        comment.UpdatedBy = user;
        
        await SaveAsync(comment);
    }
    
    /// <summary>
    /// Deletes (soft deletes) a comment.
    /// </summary>
    /// <param name="commentId">The ID of the comment to delete.</param>
    /// <param name="entityType">The type of entity the comment is on.</param>
    /// <param name="entityId">The identifier of the entity the comment is on.</param>
    /// <param name="user">The user deleting the comment (optional).</param>
    /// <param name="reason">The reason for deletion (optional).</param>
    public async Task DeleteCommentAsync(Guid commentId, string entityType, string entityId, string? user = null, string? reason = null)
    {
        var comment = await GetByIdAsync(commentId.ToString());
        
        if (comment == null)
            throw new InvalidOperationException($"Comment with ID {commentId} not found.");
        
        if (comment.EntityType != entityType || comment.EntityId != entityId)
            throw new InvalidOperationException($"Comment {commentId} does not belong to entity {entityType}/{entityId}.");
        
        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.DeletedBy = user;
        comment.DeletionReason = reason;
        
        await SaveAsync(comment);
    }
    
    /// <summary>
    /// Adds a comment using a command object.
    /// </summary>
    public async Task<Guid> AddCommentAsync(AddCommentCommand command)
    {
        command.Validate();
        
        return await AddCommentAsync(
            command.EntityType,
            command.EntityId,
            command.Text,
            command.User ?? command.IssuedBy
        );
    }
    
    /// <summary>
    /// Edits a comment using a command object.
    /// </summary>
    public async Task EditCommentAsync(EditCommentCommand command)
    {
        command.Validate();
        
        await EditCommentAsync(
            command.CommentId,
            command.EntityType,
            command.EntityId,
            command.NewText,
            command.User ?? command.IssuedBy,
            command.PreviousText
        );
    }
    
    /// <summary>
    /// Deletes a comment using a command object.
    /// </summary>
    public async Task DeleteCommentAsync(DeleteCommentCommand command)
    {
        command.Validate();
        
        await DeleteCommentAsync(
            command.CommentId,
            command.EntityType,
            command.EntityId,
            command.User ?? command.IssuedBy,
            command.Reason
        );
    }
}
