namespace TeamTrack.Domain.Common
{
    public abstract class BaseEntity
    {
        public DateTime CreatedDate { get; private set; }

        public DateTime? UpdatedDate { get; private set; }

        public string? CreatedBy { get; private set; }

        public string? UpdatedBy { get; private set; }

        internal void SetCreated(string? user)
        {
            CreatedDate = DateTime.UtcNow;
            CreatedBy = user;
        }

        internal void SetUpdated(string? user)
        {
            UpdatedDate = DateTime.UtcNow;
            UpdatedBy = user;
        }
    }
}
