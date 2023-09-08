using Dapper;
using Microsoft.Extensions.Configuration;
using SmartVault.DataGeneration.Models;
using SmartVault.DataGeneration.Util;
using SmartVault.Library;
using System.Data;
using System.Data.SQLite;
using System.Xml.Serialization;

namespace SmartVault.DAL
{
    /// <summary>
    /// TO DO: 
    ///     Make table names a CONSTANT. 
    ///     Create Dynamic queries per object to insert the values dynamically in the database and allow adding an interface for default fields like Created On
    ///     Define a better strategy for the ID of the Entities. The document is the only autoincrement in order to improve performance by avoiding using a Lock on the documentId for paralellism.
    ///     Seed OAuth user
    /// </summary>
    public class DatabaseCreation
    {
        private readonly SQLiteConnection? _connection;
        private readonly string _defaultFields = ",CreatedOn";
        private readonly string defaultParameters = ",@CreatedOn";

        public DatabaseCreation(SQLiteConnection connection)
        {
            _connection = connection;
            _connection.OpenAsync().Wait();

        }

        public static SQLiteConnection CreateDatabaseIntance(IConfigurationRoot configuration)
        {
            var databaseFileName = configuration["DatabaseFileName"];
            var absolutePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, databaseFileName));

            var connectionString = string.Format(configuration?["ConnectionStrings:DefaultConnection"] ?? "", absolutePath);
            if(!File.Exists(absolutePath))
            {
                SQLiteConnection.CreateFile(databaseFileName);
            }

            return new SQLiteConnection(connectionString);
        }

        public void CloseConnection()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        public async Task CreateDatabase(IConfiguration configuration)
        {
            var startDate = DateTime.Now;
            var elementsToSeedCount = Convert.ToInt16(configuration["DataGeneration:SeedElementsCount"] ?? "100");
            var documentsToSeedCount = Convert.ToInt16(configuration["DataGeneration:SeedDocumentsCount"] ?? "10000");


            File.WriteAllText("TestDoc.txt", $"This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}This is my test document{Environment.NewLine}");

            var documentInfo = new FileInfo("TestDoc.txt");

            var tablesExists = ValidateTableExist();
            if (!tablesExists)
            {
                await CreateTables();
            }

            await SeedTablesAsync(documentInfo, elementsToSeedCount, documentsToSeedCount);

            await LogCountOnInsertedInformation();

        }


        private async Task CreateTables()
        {

            var files = Directory.GetFiles(@"..\..\..\..\BusinessObjectSchema");
            for (int i = 0; i < files.Length; i++)
            {
                var serializer = new XmlSerializer(typeof(BusinessObject));
                var businessObject = serializer.Deserialize(new StreamReader(files[i])) as BusinessObject;
                //var escapedString = businessObject?.Script.Replace("\t", " ").Replace("\n", " ").Trim();
                await _connection?.ExecuteAsync(businessObject?.Script);

            }

        }

        private async Task SeedTablesAsync(FileInfo fileInfo, int elementsToSeedCount, int documentsToSeedCount)
        {
            var random = new Random();

            var tasks = new Task[elementsToSeedCount];
            for (int i = 0; i < tasks.Length; i++)
            {

                int localId = i;
                tasks[i] = Task.Run(async () =>
                {
                    using (var transaction = await _connection.BeginTransactionAsync())
                    {

                        var userId = localId;
                        var accountId = localId;

                        await InsertUser(userId, accountId, random, transaction);

                        await InsertAccount(accountId, transaction);

                        await InsertDocuments(localId, accountId, fileInfo, documentsToSeedCount, transaction);

                        transaction.Commit();
                    }
                });
            }

            await Task.WhenAll(tasks);

        }

        private async Task LogCountOnInsertedInformation()
        {
            var accountCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Account");
            Console.WriteLine($"AccountCount: {accountCount}");

            var documentCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Document");
            Console.WriteLine($"DocumentCount: {documentCount}");

            var userCount = _connection.ExecuteScalar<int>("SELECT COUNT(*) FROM User");
            Console.WriteLine($"UserCount: {userCount}");


        }

        /// <summary>
        /// This can be done before seeding each table. 
        /// </summary>
        /// <returns></returns>
        private bool ValidateTableExist()
        {
            var acountTableString = _connection.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='Account'");

            var documentTableString = _connection.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='Document'");

            var userTableString = _connection.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE type='table' AND name='User'");

            return !string.IsNullOrEmpty(acountTableString) && !string.IsNullOrEmpty(documentTableString) && !string.IsNullOrEmpty(userTableString);

        }

        private async Task InsertUser(int userId, int accountId, Random random, IDbTransaction transaction)
        {
            // This can be done in a better fashion if the queries are created dynamically with the entity definition.
            // If I move the BussineesObjects to a separate layer to get the definition generated by the Object generator
            string insertQuery = string.Format(Constants.INSERT_USER_QUERY, _defaultFields, defaultParameters);

            var newUser = new User
            {
                Id = userId,
                FirstName = $"FName{userId}",
                LastName = $"LName{userId}",
                DateOfBirth = Utilities.RandomDay(random).ToString("yyyy-MM-dd"),
                AccountId = accountId,
                Username = $"UserName-{userId}",
                Password = "e10adc3949ba59abbe56e057f20f883e",
                CreatedOn = DateTime.Now,
            };

            await _connection.ExecuteAsync(insertQuery, newUser, transaction);
        }

        private async Task InsertUser(int accountId, IDbTransaction transaction)
        {
            // This can be done in a better fashion if the queries are created dynamically with the entity definition.
            // If I move the BussineesObjects to a separate layer to get the definition generated by the Object generator
            string insertQuery = string.Format(Constants.INSERT_ACCOUNT_QUERY, _defaultFields, defaultParameters);

            var newAccount = new Account
            {
                Id = accountId,
                Name = $"Account{accountId}",
                CreatedOn = DateTime.Now
            };

            await _connection.ExecuteAsync(insertQuery, newAccount, transaction);

        }

        private async Task InsertAccount(int accountId, IDbTransaction transaction)
        {
            // This can be done in a better fashion if the queries are created dynamically with the entity definition.
            // If I move the BussineesObjects to a separate layer to get the definition generated by the Object generator
            string insertQuery = string.Format(Constants.INSERT_ACCOUNT_QUERY, _defaultFields, defaultParameters);

            var newAccount = new Account
            {
                Id = accountId,
                Name = $"Account{accountId}",
                CreatedOn = DateTime.Now
            };

            await _connection.ExecuteAsync(insertQuery, newAccount, transaction);

        }

        private async Task InsertDocuments(int index, int accountId, FileInfo fileInfo, int documentsCount, IDbTransaction transaction)
        {
            // This can be done in a better fashion if the queries are created dynamically with the entity definition.
            // If I move the BussineesObjects to a separate layer to get the definition generated by the Object generator
            string insertQuery = string.Format(Constants.INSERT_DOCUMENT_QUERY, _defaultFields, defaultParameters);

            // Generate a batch the insert the documents.
            var documentsToInsert = new List<Document>();

            for (int d = 0; d < documentsCount; d++)
            {
                var newDocument = new Document
                {
                    Name = $"Document{index}-{d}.txt",
                    FilePath = fileInfo.FullName,
                    Length = (int)fileInfo.Length,
                    AccountId = accountId,
                    CreatedOn = DateTime.Now
                };
                documentsToInsert.Add(newDocument);

            }

            await _connection.ExecuteAsync(insertQuery, documentsToInsert, transaction);
        }




    }
}