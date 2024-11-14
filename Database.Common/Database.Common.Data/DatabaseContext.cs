using Database.Common.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Database.Common.Data
{
    public class DatabaseContext
    {
        private DatabaseContextOptions options;
        private ILogger<DatabaseContext> logger;
        public static string StringinvlSeparator { get; set; } = "|";

        public DatabaseContext(ILogger<DatabaseContext> logger, DatabaseContextOptions? options = null)
        {
            this.logger = logger;
            if (options == null)
            {
                options = new DatabaseContextOptions()
                {
                    FileName = "database.json"
                };
            }
            this.options = options;
            logger.LogInformation("Current directory" + Directory.GetCurrentDirectory());
        }
        public async Task<List<Models.Database>> LoadDatabaseAsync()
        {
            try
            {
                if (File.Exists(options.FileName))
                {
                    string jsonData = await File.ReadAllTextAsync(options.FileName);
                    var databases = JsonSerializer.Deserialize<List<Models.Database>>(jsonData);
                    return databases ?? new List<Models.Database>();
                }
                else
                {
                    logger.LogInformation("File not found.");
                    return new List<Models.Database>();
                }
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error durring loading database: {ex.Message}");
                return new List<Models.Database>();
            }
        }

        public async Task SaveDatabaseAsync(List<Models.Database> databases)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(databases, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(options.FileName, jsonData);
                logger.LogInformation("Database successfully saved.");
            }
            catch (Exception ex)
            {
                logger.LogInformation($"Error durring saving database: {ex.Message}");
            }
        }

        public async Task CreateDatabaseAsync(string databaseName)
        {
            var databases = await LoadDatabaseAsync();

            if (!databases.Any(database => database.Name.Equals(databaseName)))
            {
                var database = new Models.Database()
                {
                    Name = databaseName
                };

                databases.Add(database);

                await SaveDatabaseAsync(databases);
            }
        }

        public async Task RemoveDatabaseAsync(string databaseName)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database != null)
            {
                databases.Remove(database);
                await SaveDatabaseAsync(databases);
            }
        }

        public async Task<Models.Database?> GetDatabaseAsync(string databaseName)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            return database;
        }

        public async Task AddTableToDatabaseAsync(string databaseName, string tableName)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            if (!database.Tables.Any(table => table.Name.Equals(tableName)))
            {
                var table = new Table()
                {
                    Name = tableName
                };

                database.Tables.Add(table);

                await SaveDatabaseAsync(databases);
            }
        }

        public async Task AddTableToDatabaseAsync(string databaseName, Table table)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }
            var check = database.Tables.Any(t => t.Name == table.Name);
            if (!check)
            {
                database.Tables.Add(table);

                await SaveDatabaseAsync(databases);
            }
        }

        public async Task AddTableToDatabaseAsync(string databaseName, string tableName, List<Property> properties)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            if (!database.Tables.Any(table => table.Name.Equals(tableName)))
            {
                var table = new Table()
                { Name = tableName };
                table.Properties.AddRange(properties);
                foreach (var prop in properties)
                {
                    table.Data.Add(new());
                }
                database.Tables.Add(table);
                await SaveDatabaseAsync(databases);
            }
        }

        public async Task RemoveTableAsync(string databaseName, string tableName)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            if (table != null)
            {
                database.Tables.Remove(table);

                await SaveDatabaseAsync(databases);
            }
        }
        public async Task InsertRowToTableAsync(string databaseName, string tableName, List<string> parameters)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            if (table == null)
            {
                throw new ArgumentException($"Table {tableName} not found in database {databaseName}.");
            }

            var properties = table.Properties;
            var i = 0;
            foreach (var prop in properties)
            {
                table.Data[i].Add(parameters[i]);
                i++;
            }

            await SaveDatabaseAsync(databases);
        }

        public async Task UpdateRowInTableAsync(string databaseName, string tableName, List<string> parameters, int index)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            if (table == null)
            {
                throw new ArgumentException($"Table {tableName} not found in database {databaseName}.");
            }

            var properties = table.Properties;
            var i = 0;
            foreach (var prop in properties)
            {
                table.Data[i][index] = parameters[i];
                i++;
            }

            await SaveDatabaseAsync(databases);
        }

        public async Task RemoveRowInTableAsync(string databaseName, string tableName, int rowId)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            if (table == null)
            {
                throw new ArgumentException($"Table {tableName} not found in database {databaseName}.");
            }
            var properties = table.Properties;
            for (int i = 0; i < properties.Count; i++)
            {
                var itemToRemove = table.Data[i].ElementAt(rowId);
                table.Data[i].Remove(itemToRemove);
            }

            await SaveDatabaseAsync(databases);
        }

        public async Task AddPropertyToTableAsync(string databaseName, string tableName, Property property)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            if (table == null)
            {
                throw new ArgumentException($"Table {tableName} not found in database {databaseName}.");
            }

            if (!table.Properties.Any(prop => prop.Name.Equals(property.Name)))
            {
                var rowsNumber = (table.Data != null && table.Data.Any()) ? table.Data.First().Count : 0;
                var data = new List<string>();
                for (int i = 0; i < rowsNumber; i++)
                {
                    data.Add(string.Empty);
                }
                table.Properties.Add(property);
                table.Data.Add(data);
                await SaveDatabaseAsync(databases);
            }
        }

        public async Task<Table> JoinTablesAsync(string databaseName, string firstTableName, string secondTableName, string firstTablePropertyName, string secondTablePropertyName)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));

            if (firstTableName == secondTableName)
            {
                throw new ArgumentException($"Cannot join table with itself.");
            }

            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var firstTable = database.Tables.FirstOrDefault(table => table.Name.Equals(firstTableName));

            if (firstTable == null)
            {
                throw new ArgumentException($"Table {firstTableName} not found in database {databaseName}.");
            }

            var secondTable = database.Tables.FirstOrDefault(table => table.Name.Equals(secondTableName));
            if (secondTable == null)
            {
                throw new ArgumentException($"Table {secondTableName} not found in database {databaseName}.");
            }

            var firstTableProperty = firstTable.Properties.FirstOrDefault(prop => prop.Name.Equals(firstTablePropertyName));

            if (firstTableProperty == null)
            {
                throw new ArgumentException($"Table {firstTableName} doesn't contain property {firstTablePropertyName}.");
            }

            var firstPropertyIndex = firstTable.Properties.IndexOf(firstTableProperty);

            var secondTableProperty = secondTable.Properties.FirstOrDefault(prop => prop.Name.Equals(secondTablePropertyName));

            if (secondTableProperty == null)
            {
                throw new ArgumentException($"Table {secondTableName} doesn't contain property {secondTablePropertyName}.");
            }

            if (firstTableProperty.Type != secondTableProperty.Type)
            {
                throw new ArgumentException($"Join properties type missmatch. ");
            }

            var secondPropertyIndex = secondTable.Properties.IndexOf(secondTableProperty);

            var joinTable = new Table() { Name = $"{firstTableName} joined {secondTableName}" };

            for (int k = 0; k < firstTable.Properties.Count; k++)
            {
                joinTable.Properties.Add(firstTable.Properties[k]);
            }

            for (int k = 0; k < secondTable.Properties.Count; k++)
            {
                if (k == secondPropertyIndex)
                {
                    continue;
                }

                joinTable.Properties.Add(secondTable.Properties[k]);
            }

            for (int index = 0; index < firstTable.Properties.Count + secondTable.Properties.Count - 1; index++)
            {
                joinTable.Data.Add(new List<string>());
            }

            int i = 0;

            int joinTableDataIndex = 0;

            foreach (var firstTableData in firstTable.Data[firstPropertyIndex])
            {
                int j = 0;
                foreach (var secondTableData in secondTable.Data[secondPropertyIndex])
                {
                    if (firstTableData == secondTableData)
                    {
                        for (int k = 0; k < firstTable.Properties.Count; k++)
                        {
                            joinTable.Data[joinTableDataIndex].Add(firstTable.Data[k][i]);
                            joinTableDataIndex++;
                        }

                        for (int k = 0; k < secondTable.Properties.Count; k++)
                        {
                            if (k == secondPropertyIndex)
                            {
                                continue;
                            }
                            joinTable.Data[joinTableDataIndex].Add(secondTable.Data[k][j]);
                            joinTableDataIndex++;
                        }
                        joinTableDataIndex = 0;
                    }
                    j++;
                }
                i++;
            }

            return joinTable;
        }

        public async Task RemovePropertyFromTableAsync(string databaseName, string tableName, string propertyName)
        {
            var databases = await LoadDatabaseAsync();
            var database = databases.FirstOrDefault(db => db.Name.Equals(databaseName));
            if (database == null)
            {
                throw new ArgumentException($"Database with name={databaseName} not found.");
            }

            var table = database.Tables.FirstOrDefault(table => table.Name.Equals(tableName));

            if (table == null)
            {
                throw new ArgumentException($"Table {tableName} not found in database {databaseName}.");
            }

            var property = table.Properties.FirstOrDefault(prop => prop.Name.Equals(propertyName));
            if (property != null)
            {
                var index = table.Properties.IndexOf(property);
                table.Properties.Remove(property);
                var listToRemove = table.Data.ElementAt(index);
                table.Data.Remove(listToRemove);
                await SaveDatabaseAsync(databases);
            }
        }
    }
}
