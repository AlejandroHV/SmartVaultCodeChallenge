using Microsoft.Extensions.Configuration;
using SmartVault.DAL;
using SmartVault.DataGeneration.Models;
using SmartVault.DataGeneration.Util;
using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

namespace SmartVault.Program
{
    /// <summary>
    /// Can implement a common method to log errors.
    /// </summary>
    partial class Program
    {
        async static Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json").Build();

            var sqlLiteConnection = DatabaseCreation.CreateDatabaseIntance(configuration);
            var databaseInstance = new DatabaseCreation(sqlLiteConnection);
            var databaseQuerying = new DatabaseQuerying(sqlLiteConnection);

            try
            {
                await WriteEveryThirdFileToFile(args[0], databaseQuerying);
                await GetAllFileSizes(databaseQuerying);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                databaseInstance.CloseConnection();

            }
            Console.ReadLine();

        }

        private async static Task GetAllFileSizes(DatabaseQuerying databaseQuerying)
        {
            var query = "SELECT SUM(Length) FROM Document";
            var result = await databaseQuerying.GetScalar<long?>(query);
            if (result == null)
            {
                Console.WriteLine("Error: No data could be retrieved");
                return;
            }
            Console.WriteLine($"The total size of all the documents in the database is:{result}"); 
        }

        private static async Task WriteEveryThirdFileToFile(string accountIdString, DatabaseQuerying databaseQuerying )
        {
           bool isInt=  Int32.TryParse(accountIdString, out var accountId);

            if (!isInt)
            {
                Console.WriteLine("Error: Invalid");
                return; 
            }

            var documents = await databaseQuerying.GetEntity<Document>(tableName: Constants.TABLE_NAME_DOCUMENT,
                                                                        filter: $"AccountId ={accountId}", 
                                                                        orderByColumnName: "Id", 
                                                                        orderByType: "ASC",
                                                                        limit: true,
                                                                        nValue: 3);

            if (!documents.Any())
            {
                Console.WriteLine("Error: No Data");
                return;
            }

            var thirdDocumentFromAccount = documents.FirstOrDefault();
            if(string.IsNullOrEmpty(thirdDocumentFromAccount.FilePath))
            {
                Console.WriteLine("Error: No FilePath");
                return;
            }

            Console.WriteLine($"Third Document Id {thirdDocumentFromAccount.Id} for account{thirdDocumentFromAccount.AccountId}");

            using (var stream = new StreamReader(thirdDocumentFromAccount.FilePath))
            {
                string fileContent = string.Empty;
                string documentContents;

                // Read and process each line in the file
                while ((documentContents = stream.ReadLine()) != null)
                {
                    fileContent += documentContents;
                    
                }
                File.WriteAllText("ThirdFileOutPut.txt", fileContent);
            }

        }
    }
}