using System.Linq.Expressions;
using MovieMVC.DTOs;
using MovieMVC.Models;
using MovieMVC.Repositories;

namespace MovieMVC.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _repository;

        public HomeService(IHomeRepository repository)
        {
            _repository = repository;
        }

        public List<MovieDto> GetMovies(string? search, string? sort, bool desc = false)
        {
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
                "director" => ByPosition(1),
                "producer" => ByPosition(2),
                "writer" => ByPosition(3),
                "actor" => ByPosition(4),
                "category" => m => m.MovieCategory
                    .OrderBy(mc => mc.Category!.Category)
                    .Select(mc => mc.Category!.Category)
                    .FirstOrDefault(),
                _ => m => m.Id
            };

            var sorted = desc
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);

            return sorted.Select(m => MapToDto(m)).ToList();
        }

        private static MovieDto MapToDto(Movie m)
        {
            return new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                Actors = GetNamesByPosition(m.MoviePeople, 4),
                Directors = GetNamesByPosition(m.MoviePeople, 1),
                Writers = GetNamesByPosition(m.MoviePeople, 3),
                Producers = GetNamesByPosition(m.MoviePeople, 2),
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
