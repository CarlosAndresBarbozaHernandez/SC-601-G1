using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PAP.Generators
{
    /// <summary>
    /// Datos base y utilidades compartidas por todos los generadores.
    /// Usa una semilla fija para que cada generación produzca los mismos datos.
    /// </summary>
    public static class GeneratorData
    {
        public const int Seed = 601;

        public static Random NewRandom()
        {
            return new Random(Seed);
        }

        // Integrantes del grupo (primeros clientes y empleados administradores)
        public static readonly string[][] TeamMembers =
        {
            new[] { "Brandon",       "Solís Araya" },
            new[] { "Daniela",       "Rodríguez Jiménez" },
            new[] { "Carlos Andrés", "Barboza Hernández" },
            new[] { "Sebastián",     "Rivera Sánchez" }
        };

        public static readonly string[] FirstNames =
        {
            "Brandon", "Daniela", "Carlos", "Sebastián", "Camila", "Mateo", "Jorge", "Sofía",
            "Andrés", "Luis", "Ana", "Valeria", "Diego", "María", "José", "Fernanda",
            "Gabriel", "Isabel", "Ricardo", "Paula", "Esteban", "Mónica", "Pablo", "Natalia",
            "Laura", "Adrián", "Lucía", "Tomás", "Elena", "Javier", "Rebeca", "Kevin"
        };

        public static readonly string[] LastNames =
        {
            "Solís", "Araya", "Rodríguez", "Jiménez", "Barboza", "Hernández", "Rivera", "Sánchez",
            "Núñez", "Rojas", "Quesada", "Vargas", "Salas", "Fallas", "Cordero", "Solano",
            "Mora", "Chaves", "Alvarado", "Castro", "Herrera", "Ramírez", "Villalobos", "Campos",
            "Brenes", "Segura", "Ulate", "Zúñiga"
        };

        public static readonly string[] Countries =
        {
            "Costa Rica", "México", "Chile", "Colombia", "Argentina", "España", "Perú", "Uruguay"
        };

        // Ciudad, Provincia, Código postal
        public static readonly string[][] Cities =
        {
            new[] { "San José",      "San José",   "10101" },
            new[] { "Escazú",        "San José",   "10201" },
            new[] { "Heredia",       "Heredia",    "40101" },
            new[] { "Santo Domingo", "Heredia",    "40301" },
            new[] { "Alajuela",      "Alajuela",   "20101" },
            new[] { "Grecia",        "Alajuela",   "20301" },
            new[] { "Cartago",       "Cartago",    "30101" },
            new[] { "Liberia",       "Guanacaste", "50101" },
            new[] { "Puntarenas",    "Puntarenas", "60101" },
            new[] { "Limón",         "Limón",      "70101" }
        };

        public static readonly string[] BookTitles =
        {
            "El Horizonte Silencioso", "Más Allá de las Estrellas", "El Arte de C#", "Ecos del Invierno",
            "El Último Algoritmo", "Jardines de la Luna", "Ciberseguridad Moderna", "La Ciudad de Niebla",
            "Datos que Hablan", "El Bosque Nuboso", "Arquitectura Limpia en .NET", "Memorias del Caribe",
            "Sombras en Monteverde", "Introducción a SQL Server", "El Volcán Dormido", "Cartas a una Programadora",
            "Historia de Centroamérica", "Patrones de Diseño Prácticos", "La Isla del Coco", "El Código Perdido",
            "Mareas de Fuego", "Cuentos para Dormir", "Economía para Todos", "Inteligencia Artificial Hoy",
            "El Faro del Pacífico", "Liderazgo Ágil", "El Jaguar y la Luna", "Redes desde Cero",
            "Poemas del Valle Central", "La Última Biblioteca", "Física sin Miedo", "El Detective de Limón",
            "Viaje al Centro del Bit", "Arte Precolombino", "La Casa de los Espejos", "Emprender en Costa Rica",
            "El Secreto del Quetzal", "Python para Analistas", "Crónicas de Talamanca", "Matemática Divertida",
            "El Tren de Medianoche", "Cocina Tica Tradicional", "Algoritmos en Acción", "El Mar de Cristal",
            "Biografía de un Científico", "Cielo de Guanacaste", "La Ruta del Café", "Diseño de Bases de Datos",
            "El Guardián del Bosque", "Noches de Puerto Viejo"
        };

        /// <summary>Elemento al azar de una lista.</summary>
        public static T Pick<T>(Random rnd, IList<T> list)
        {
            return list[rnd.Next(list.Count)];
        }

        /// <summary.</summary>
        public static string TwoLastNames(Random rnd)
        {
            string first = Pick(rnd, LastNames);
            string second = Pick(rnd, LastNames);

            while (second == first)
            {
                second = Pick(rnd, LastNames);
            }

            return first + " " + second;
        }

        /// <summary>.</summary>
        public static string Phone(Random rnd)
        {
            return "+506 " + Pick(rnd, new[] { "6", "7", "8" }) + rnd.Next(100, 1000) + "-" + rnd.Next(1000, 10000);
        }

        /// <summary>Fecha al azar entre dos años.</summary>
        public static DateTime DateBetween(Random rnd, int fromYear, int toYear)
        {
            var start = new DateTime(fromYear, 1, 1);
            var end = new DateTime(toYear, 12, 31);
            return start.AddDays(rnd.Next((end - start).Days));
        }

        /// <summary>Fecha de hace 1 a maxDays días.</summary>
        public static DateTime DaysAgo(Random rnd, int maxDays)
        {
            return DateTime.Now.AddDays(-rnd.Next(1, maxDays + 1)).AddMinutes(-rnd.Next(0, 1440));
        }

        /// <summary>Redondea a 2 decimales.</summary>
        public static decimal Money(decimal value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>"Sebastián Rivera" -> "sebastian.rivera" (para correos).</summary>
        public static string Slug(string text)
        {
            string normalized = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(c)) sb.Append(c);
                else if (c == ' ' && sb.Length > 0 && sb[sb.Length - 1] != '.') sb.Append('.');
            }

            return sb.ToString().Trim('.');
        }

        /// <summary>Devuelve 'count' elementos distintos de la lista, al azar.</summary>
        public static List<T> PickDistinct<T>(Random rnd, IList<T> list, int count)
        {
            return list.OrderBy(x => rnd.Next()).Take(Math.Min(count, list.Count)).ToList();
        }
    }
}