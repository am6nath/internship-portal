namespace InternshipPortal.API.Entities
{
    public class AuditLog : BaseEntity
    {
        public string TableName { get; set; }

        public string Action { get; set; }

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public Guid? UserId { get; set; }

        public string? UserEmail { get; set; }
    }
}