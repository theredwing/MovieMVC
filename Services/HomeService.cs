using System.Linq.Expressions;
using MovieMVC.DTOs;
using MovieMVC.Models;
using MovieMVC.Repositories;

namespace MovieMVC.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _repository;
        private readonly ILookupRepository _lookupRepository;

        public HomeService(IHomeRepository repository, ILookupRepository lookupRepository)
        {
            _repository = repository;
            _lookupRepository = lookupRepository;
        }

        public List<MovieDto> GetMovies(string? search, string? sort, bool desc = false)
        {
            var positionIds = _lookupRepository.GetAllPositionIds();
            var directorId = positionIds.GetValueOrDefault("director");
            var producerId = positionIds.GetValueOrDefault("producer");
            var writerId = positionIds.GetValueOrDefault("writer");
            var actorId = positionIds.GetValueOrDefault("actor");

            var query = _repository.GetAllWithIncludes(search);

            Expression<Func<Movie, object?>> ByPosition(int positionId) =>
                m => m.MoviePeople
                    .Where(mp => mp.PositionId == positionId)
                    .OrderBy(mp => mp.Name!.Name)
                    .Select(mp => mp.Name!.Name)
                    .FirstOrDefault();

            Expression<Func<Movie, object?>> keySelector = sort switch
            {
                "title" => m => m.Title,
                "director" => ByPosition(directorId),
                "producer" => ByPosition(producerId),
                "writer" => ByPosition(writerId),
                "actor" => ByPosition(actorId),
                "category" => m => m.MovieCategory
                    .OrderBy(mc => mc.Category!.Category)
                    .Select(mc => mc.Category!.Category)
                    .FirstOrDefault(),
                _ => m => m.Id
            };

            var sorted = desc
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);

            return sorted.Select(m => MapToDto(m, directorId, producerId, writerId, actorId)).ToList();
        }

        private static MovieDto MapToDto(Movie m, int directorId, int producerId, int writerId, int actorId)
        {
            return new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Actors = GetNamesByPosition(m.MoviePeople, actorId),
                Directors = GetNamesByPosition(m.MoviePeople, directorId),
                Writers = GetNamesByPosition(m.MoviePeople, writerId),
                Producers = GetNamesByPosition(m.MoviePeople, producerId),
                Categories = m.MovieCategory?
                    .Select(mc => mc.Category?.Category ?? "").ToList() ?? []
            };
        }

        private static List<string> GetNamesByPosition(List<MovieNamesPosition>? people, int positionId)
        {
            return people?
                .Where(mp => mp.PositionId == positionId)
                .Select(mp => mp.Name?.Name ?? "")
                .ToList() ?? [];
        }
    }
}
