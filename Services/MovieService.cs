using MovieMVC.DTOs;
using MovieMVC.Models;
using MovieMVC.Repositories;

namespace MovieMVC.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _repository;

        public MovieService(IMovieRepository repository)
        {
            _repository = repository;
        }

        public async Task<Movie?> GetMovieDetailsAsync(int id)
        {
            return await _repository.GetWithDetailsAsync(id);
        }

        public async Task<MovieSelections> GetSelectedIdsAsync(Movie movie)
        {
            var directorPosId = await _repository.GetPositionIdAsync("director");
            var producerPosId = await _repository.GetPositionIdAsync("producer");
            var writerPosId = await _repository.GetPositionIdAsync("writer");
            var actorPosId = await _repository.GetPositionIdAsync("actor");

            return new MovieSelections
            {
                DirectorIds = movie.MoviePeople?.Where(mp => mp.PositionId == directorPosId).Select(mp => mp.NamesId).ToArray() ?? [],
                ProducerIds = movie.MoviePeople?.Where(mp => mp.PositionId == producerPosId).Select(mp => mp.NamesId).ToArray() ?? [],
                WriterIds = movie.MoviePeople?.Where(mp => mp.PositionId == writerPosId).Select(mp => mp.NamesId).ToArray() ?? [],
                ActorIds = movie.MoviePeople?.Where(mp => mp.PositionId == actorPosId).Select(mp => mp.NamesId).ToArray() ?? [],
                CategoryIds = movie.MovieCategory?.Select(mc => mc.CategoryId).ToArray() ?? []
            };
        }

        public async Task CreateMovieAsync(Movie movie, int[]? selectedDirectors, int[]? selectedProducers, int[]? selectedWriters, int[]? selectedActors, int[]? selectedCategories)
        {
            _repository.Add(movie);

            await AddPeopleByRole(movie, "director", selectedDirectors);
            await AddPeopleByRole(movie, "producer", selectedProducers);
            await AddPeopleByRole(movie, "writer", selectedWriters);
            await AddPeopleByRole(movie, "actor", selectedActors);
            AddCategories(movie, selectedCategories);

            await _repository.SaveChangesAsync();
        }

        public async Task<Movie?> UpdateMovieAsync(int id, Movie updatedMovie, int[]? selectedDirectors, int[]? selectedProducers, int[]? selectedWriters, int[]? selectedActors, int[]? selectedCategories)
        {
            var movie = await _repository.GetWithRelationsAsync(id);
            if (movie == null) return null;

            movie.Title = updatedMovie.Title;
            movie.Description = updatedMovie.Description;

            // replace all people links
            _repository.RemovePeople(movie.MoviePeople!);
            await AddPeopleByRole(movie, "director", selectedDirectors);
            await AddPeopleByRole(movie, "producer", selectedProducers);
            await AddPeopleByRole(movie, "writer", selectedWriters);
            await AddPeopleByRole(movie, "actor", selectedActors);

            // replace all category links
            _repository.RemoveCategories(movie.MovieCategory!);
            AddCategories(movie, selectedCategories);

            await _repository.SaveChangesAsync();
            return movie;
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _repository.FindAsync(id);
            if (movie == null) return false;

            _repository.Remove(movie);
            await _repository.SaveChangesAsync();
            return true;
        }

        public List<NamesLU> GetAllNames()
        {
            return _repository.GetAllNames();
        }

        public List<CategoryLU> GetAllCategories()
        {
            return _repository.GetAllCategories();
        }

        private async Task AddPeopleByRole(Movie movie, string positionName, int[]? selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0) return;

            var posId = await _repository.GetPositionIdAsync(positionName);
            if (posId == 0) return;

            foreach (var nameId in selectedIds.Distinct())
            {
                _repository.AddPerson(new MovieNamesPosition
                {
                    Movie = movie,
                    NamesId = nameId,
                    PositionId = posId
                });
            }
        }

        private void AddCategories(Movie movie, int[]? selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0) return;

            foreach (var catId in selectedIds.Distinct())
            {
                _repository.AddCategory(new MovieCategory
                {
                    Movie = movie,
                    CategoryId = catId
                });
            }
        }
    }
}
