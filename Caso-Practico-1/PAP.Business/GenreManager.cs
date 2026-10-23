
using PAP.DataAccess;
using PAP.Repositories;
using System.Collections.Generic;

namespace PAP.Business
{
    public class GenreManager
    {
        private readonly GenreRepository _repository;

        public GenreManager()
        {
            _repository = new GenreRepository();
        }

        public IEnumerable<Genre> GetAll()
        {
            return _repository.GetAll();
        }

        public Genre GetById(int id)
        {
            return _repository.GetById(id);
        }

        public void Add(Genre genre)
        {
            _repository.Add(genre);
        }

        public void Update(Genre genre)
        {
            _repository.Update(genre);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }
    }
}
