using PAP.DataAccess;
using PAP.Repositories;
using System;

namespace PAP.Generators
{
    public class PublishersGenerator
    {
        private readonly PublisherRepository _repository;

        public PublishersGenerator()
        {
            _repository = new PublisherRepository();
        }

        public bool HasData()
        {
            foreach (var publisher in _repository.GetAll())
            {
                return true;
            }

            return false;
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Publishers already contains data.");
            }

            Publisher publisher1 = new Publisher
            {
                Name = "Penguin Books",
                Country = "United Kingdom",
                Website = "www.penguin.com"
            };

            Publisher publisher2 = new Publisher
            {
                Name = "HarperCollins",
                Country = "United States",
                Website = "www.harpercollins.com"
            };

            Publisher publisher3 = new Publisher
            {
                Name = "Editorial Planeta",
                Country = "Spain",
                Website = "www.planetadelibros.com"
            };

            _repository.Add(publisher1);
            _repository.Add(publisher2);
            _repository.Add(publisher3);
        }
    }
}