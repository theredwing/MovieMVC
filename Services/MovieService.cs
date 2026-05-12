using MovieMVC.DTOs;
using MovieMVC.Models;
using MovieMVC.Repositories;

namespace MovieMVC.Services
{
    public class MovieService : IMovieService
    {
        private readonly IMovieRepository _repository;
        private readonly ILookupRepository _lookupRepository;

        public MovieService(IMovieRepository repository, ILookupRepository lookupRepository)
        {
            _repository = repository;
            _lookupRepository = lookupRepository;
        }

        public async Task<Movie?> GetMovieDetailsAsync(int id)
        {
            return await _repository.GetWithDetailsAsync(id);
        }

        public async Task<MovieSelections> GetSelectedIdsAsync(Movie movie)
        {
            var positionIds = await _lookupRepository.GetAllPositionIdsAsync();
            var directorPosId = positionIds.GetValueOrDefault("director");
            var producerPosId = positionIds.GetValueOrDefault("producer");
            var writerPosId = positionIds.GetValueOrDefault("writer");
            var actorPosId = positionIds.GetValueOrDefault("actor");

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

            var positionIds = await _lookupRepository.GetAllPositionIdsAsync();
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("director"), selectedDirectors);
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("producer"), selectedProducers);
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("writer"), selectedWriters);
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("actor"), selectedActors);
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

            var positionIds = await _lookupRepository.GetAllPositionIdsAsync();
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("director"), selectedDirectors);
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("producer"), selectedProducers);
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("writer"), selectedWriters);
            AddPeopleByRole(movie, positionIds.GetValueOrDefault("actor"), selectedActors);

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
            return _lookupRepository.GetAllNames();
        }

        public List<NamesLU> GetAllNamesWithMovieCount()
        {
            return _lookupRepository.GetAllNamesWithMovieCount();
        }

        public List<CategoryLU> GetAllCategories()
        {
            return _lookupRepository.GetAllCategories();
        }

        public async Task MergeNamesAsync(int targetId, List<int> sourceIds)
        {
            await _repository.MergeNamesAsync(targetId, sourceIds);
        }

        public async Task<NamesLU?> GetNameByIdAsync(int id)
        {
            return await _repository.GetNameByIdAsync(id);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
        {
            return await _repository.NameExistsAsync(name, excludeId);
        }

        public async Task AddNameAsync(NamesLU name)
        {
            await _repository.AddNameAsync(name);
        }

        public async Task UpdateNameAsync(NamesLU name)
        {
            await _repository.UpdateNameAsync(name);
        }

        public async Task<bool> DeleteNameAsync(int id)
        {
            return await _repository.DeleteNameAsync(id);
        }

        private void AddPeopleByRole(Movie movie, int positionId, int[]? selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0 || positionId == 0) return;

            foreach (var nameId in selectedIds.Distinct())
            {
                _repository.AddPerson(new MovieNamesPosition
                {
                    Movie = movie,
                    NamesId = nameId,
                    PositionId = positionId
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
