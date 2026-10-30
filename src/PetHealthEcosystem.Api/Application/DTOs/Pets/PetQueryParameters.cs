namespace PetHealthEcosystem.Api.Application.DTOs.Pets
{
    public class PetQueryParameters
    {
        private const int MaxPageSize = 50;
        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
        }

        public string? Name { get; set; }
        public string? Breed { get; set; }
        public string? TutorName { get; set; }
        public bool? NeedsPostOpCare { get; set; }
        public string? SortBy { get; set; } = "name"; // "id", "name", "breed", "age", "tutor"
        public string? SortOrder { get; set; } = "asc"; // "asc", "desc"
    }
}
