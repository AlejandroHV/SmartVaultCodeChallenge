using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SmartVault.DAL;
using SmartVault.Library;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace SmartVault.DataGeneration
{
    partial class Program
    {
        /// <summary>
        /// Updated the data generation layer with a new Async implementation  DatabaseCreation.CreateDatabase()
        /// Even updated the implementation send with the tests:  OldSolution.CreateDatabase(configuration)
        /// The database file is created in a new folder called "Database" in the root of the solution.
        /// This solution is executing both datacreation implementations but the old is on a separate file that is not referenced in the solution layer Program.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            var startDate = DateTime.Now;
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            var sqlLiteConnection = DatabaseCreation.CreateDatabaseIntance(configuration);
            var solution = new DatabaseCreation(sqlLiteConnection);

            try
            {
                await solution.CreateDatabase(configuration);
                OldSolution.CreateDatabase(configuration);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {

                solution.CloseConnection();

                var finishDate = DateTime.Now;
                TimeSpan interval = finishDate - startDate;
                Console.WriteLine($"Finishing creating and seeding the database in time: {interval.TotalSeconds} sec");
                File.WriteAllText("Result2.txt", $"Finishing creating and seeding the database in time: {interval.TotalSeconds} sec");

            }
            
            Console.ReadLine();
        }

    }
}