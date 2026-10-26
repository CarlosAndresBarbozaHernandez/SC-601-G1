using PAP.DataAccess;
using PAP.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

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
            return _repository.GetAll().Any();
        }

        public void Generate()
        {
            if (HasData())
            {
                throw new Exception("Publishers already contains data.");
            }

            // Nombre, País
            string[][] data =
            {
                new[] { "Penguin Random House", "Estados Unidos" },
                new[] { "HarperCollins",        "Estados Unidos" },
                new[] { "Editorial Planeta",    "España" },
                new[] { "Anagrama",             "España" },
                new[] { "O'Reilly Media",       "Estados Unidos" },
                new[] { "Alfaguara",            "España" },
                new[] { "Editorial Costa Rica", "Costa Rica" },
                new[] { "Tor Books",            "Estados Unidos" },
                new[] { "Wiley",                "Estados Unidos" },
                new[] { "Siglo XXI",            "México" }
            };

            var publishers = new List<Publisher>();

            foreach (var p in data)
            {
                // "Editorial Planeta" -> "editorialplaneta"
                string slug = GeneratorData.Slug(p[0]).Replace(".", "");

                publishers.Add(new Publisher
                {
                    Name = p[0],
                    Country = p[1],
                    Website = "https://www." + slug + ".com",
                    Email = "contacto@" + slug + ".com",
                    CreatedAt = DateTime.Now
                });
            }

            _repository.AddRange(publishers);
        }
    }
}