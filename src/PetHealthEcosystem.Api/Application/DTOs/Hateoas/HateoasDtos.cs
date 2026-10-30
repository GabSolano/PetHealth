namespace PetHealthEcosystem.Api.Application.DTOs.Hateoas
{
    public class LinkDto
    {
        public string Href { get; set; } = string.Empty;
        public string Rel { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;

        public LinkDto() { }

        public LinkDto(string href, string rel, string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }
    }

    public class ResourceEnvelope<T>
    {
        public T Data { get; set; } = default!;
        public List<LinkDto> Links { get; set; } = new();

        public ResourceEnvelope() { }

        public ResourceEnvelope(T data, List<LinkDto>? links = null)
        {
            Data = data;
            if (links != null)
                Links = links;
        }
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 1));
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
        public List<LinkDto> Links { get; set; } = new();
    }
}
