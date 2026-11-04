using PetHealthEcosystem.Api.Application.DTOs.Hateoas;
using PetHealthEcosystem.Api.Models;

namespace PetHealthEcosystem.Api.Application.Common
{
    public static class HateoasHelper
    {
        public static Pet AddHateoasLinks(this Pet pet, string baseUrl = "/api/pets")
        {
            pet.Links = new List<LinkDto>
            {
                new LinkDto($"{baseUrl}/{pet.Id}", "self", "GET"),
                new LinkDto($"{baseUrl}/{pet.Id}", "update", "PUT"),
                new LinkDto($"{baseUrl}/{pet.Id}", "delete", "DELETE"),
                new LinkDto($"/api/petcarelogs/pet/{pet.Id}", "care-logs", "GET"),
                new LinkDto(baseUrl, "all-pets", "GET")
            };
            return pet;
        }

        public static PagedResult<Pet> AddHateoasLinks(this PagedResult<Pet> result, string baseUrl = "/api/pets")
        {
            foreach (var item in result.Items)
            {
                item.AddHateoasLinks(baseUrl);
            }

            result.Links = new List<LinkDto>
            {
                new LinkDto($"{baseUrl}?pageNumber={result.PageNumber}&pageSize={result.PageSize}", "self", "GET"),
                new LinkDto($"{baseUrl}?pageNumber=1&pageSize={result.PageSize}", "first", "GET")
            };

            if (result.HasPrevious)
            {
                result.Links.Add(new LinkDto($"{baseUrl}?pageNumber={result.PageNumber - 1}&pageSize={result.PageSize}", "previous", "GET"));
            }

            if (result.HasNext)
            {
                result.Links.Add(new LinkDto($"{baseUrl}?pageNumber={result.PageNumber + 1}&pageSize={result.PageSize}", "next", "GET"));
            }

            if (result.TotalPages > 0)
            {
                result.Links.Add(new LinkDto($"{baseUrl}?pageNumber={result.TotalPages}&pageSize={result.PageSize}", "last", "GET"));
            }

            return result;
        }
    }
}
