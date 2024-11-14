using Database.Common.Data;
using Database.Common.Models;
using Database.Common.Services;
using Database.Web.MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace Database.Web.MVC.Controllers
{
    public class DatabasesController(DatabaseContext databaseContext) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var dbs = await databaseContext.LoadDatabaseAsync();
            return View(dbs);
        }

        public async Task<IActionResult> Tables(string name)
        {
            var db = await databaseContext.GetDatabaseAsync(name);
            return View(db);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Common.Models.Database database)
        {
            if (ModelState.IsValid)
            {
                if (databaseContext.GetDatabaseAsync(database.Name).Result != null)
                {
                    return View(database);
                }
                await databaseContext.CreateDatabaseAsync(database.Name);
                return RedirectToAction(nameof(Index));
            }
            return View(database);
        }

        public IActionResult CreateTable(string dbName)
        {
            var tableModel = new TableModel
            {
                DatabaseName = dbName
            };
            return View(tableModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTable(TableModel tableModel)
        {
            if (ModelState.IsValid)
            {
                if (databaseContext.GetDatabaseAsync(tableModel.DatabaseName).Result!.Tables.Any(table => table.Name == tableModel.Name))
                {
                    return View(tableModel);
                }
                var table = new Table { Name = tableModel.Name, Properties = tableModel.Properties };
                await databaseContext.AddTableToDatabaseAsync(tableModel.DatabaseName, table);
                return RedirectToAction("Tables", new { name = tableModel.DatabaseName });
            }
            return View(tableModel);
        }

        public IActionResult CreateProperty(string tableName, string dbName)
        {
            var propertyModel = new PropertyModel
            {
                DbName = dbName,
                TableName = tableName
            };
            return View(propertyModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProperty(PropertyModel propertyModel)
        {
            if (ModelState.IsValid)
            {
                if (databaseContext.GetDatabaseAsync(propertyModel.DbName).Result!.Tables.FirstOrDefault(table => table.Name == propertyModel.TableName)!.Properties.Any(prop => prop.Name == propertyModel.Name))
                {
                    return View(propertyModel);
                }
                var property = new Property { Name = propertyModel.Name, Type = propertyModel.PropertyType };
                await databaseContext.AddPropertyToTableAsync(propertyModel.DbName, propertyModel.TableName, property);
                return RedirectToAction("EditTable", new { name = propertyModel.TableName, dbName = propertyModel.DbName });
            }
            return View(propertyModel);
        }

        public async Task<IActionResult> Delete(string name)
        {
            await databaseContext.RemoveDatabaseAsync(name);
            var databases = await databaseContext.LoadDatabaseAsync();
            return View("Index", databases);
        }

        public async Task<IActionResult> DeleteTable(string name, string dbName)
        {
            await databaseContext.RemoveTableAsync(dbName, name);
            var db = await databaseContext.GetDatabaseAsync(dbName);
            return View("Tables", db);
        }

        public async Task<IActionResult> DeleteProperty(string name, string tableName, string dbName)
        {

            ViewData["DbName"] = dbName;
            await databaseContext.RemovePropertyFromTableAsync(dbName, tableName, name);
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db.Tables.FirstOrDefault(table => table.Name == tableName);
            return View("EditTable", table);
        }

        public async Task<IActionResult> EditTable(string name, string dbName)
        {
            ViewData["DbName"] = dbName;
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db?.Tables.FirstOrDefault(table => table.Name == name);
            return View(table);
        }

        public async Task<IActionResult> Fields(string tableName, string dbName, Dictionary<string, string[]> stringParams)
        {
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db.Tables.FirstOrDefault(table => table.Name == tableName);
            ViewData["DbName"] = dbName;
            ViewBag.Table = table;
            var tableModel = new TableFullModel()
            {
                TableName = table.Name,
                Columns = table.Properties,
                DbName = dbName,
                Values = table.Data
            };
            return View(tableModel);
        }

        public async Task<IActionResult> CreateField(string tableName, string dbName)
        {
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db.Tables.FirstOrDefault(table => table.Name == tableName);
            var tableFieldModel = new FieldCreateModel
            {
                DbName = dbName,
                Columns = table.Properties,
                TableName = tableName,
                Values = new Dictionary<string, dynamic>()
            };
            foreach (var column in tableFieldModel.Columns)
            {
                if (column.Type == PropertyType.StringInvl)
                {
                    tableFieldModel.Values.Add(column.Name, new StringInVl());
                }
                else
                {
                    tableFieldModel.Values.Add(column.Name, "");
                }
            }
            return View(tableFieldModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateField(IFormCollection tableFieldModelCollection)
        {
            var tableName = tableFieldModelCollection["TableName"];
            var dbName = tableFieldModelCollection["DbName"];
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db.Tables.FirstOrDefault(table => table.Name == tableName);
            var dictValues = new Dictionary<string, dynamic>();
            var keys = tableFieldModelCollection.Keys.Where(x => table.Properties.Select(prop => prop.Name).Contains(x));
            ViewBag.Table = table;
            var tableFieldModel = new FieldCreateModel
            {
                TableName = tableName,
                DbName = dbName,
                Columns = table.Properties,
                Values = new Dictionary<string, dynamic>()
            };
            foreach (var key in keys)
            {
                if (tableFieldModel.Columns.FirstOrDefault(x => x.Name == key).Type == PropertyType.StringInvl)
                {
                    var min = tableFieldModelCollection[key][0] ?? "";
                    var max = tableFieldModelCollection[key][1] ?? "";
                    if (!String.IsNullOrEmpty(tableFieldModelCollection[key][0]) && !String.IsNullOrEmpty(max) && String.Compare(min, max) > 0)
                    {
                        ViewData[$"Error_{key}"] = $"Interval is not valid";
                        return View(tableFieldModel);
                    }
                    tableFieldModel.Values.Add(key, $"{min}{DatabaseContext.StringinvlSeparator}{max}");
                }
                else
                {
                    tableFieldModel.Values.Add(key, tableFieldModelCollection[key].FirstOrDefault() ?? "");
                }
            }
            var htmlValues = tableFieldModel.Values.Where(v => table.Properties.FirstOrDefault(x => x.Name == v.Key).Type == PropertyType.Html);
            foreach (var html in htmlValues)
            {
                if (!HtmlValidator.IsValid(html.Value))
                {
                    ViewData[$"Error_{html.Key}"] = $"Html is not valid";
                    return View(tableFieldModel);
                }
            }
            if (ModelState.IsValid)
            {
                await databaseContext.InsertRowToTableAsync(dbName, tableName, tableFieldModel.Values.Where(kv => kv.Value is string).Select(kv => (string)kv.Value).ToList());
                return RedirectToAction("Fields", new { tableName, dbName });
            }
            return View(tableFieldModel);
        }

        public async Task<IActionResult> DeleteField(int index, string tableName, string dbName)
        {
            await databaseContext.RemoveRowInTableAsync(dbName, tableName, index);
            return RedirectToAction("Fields", new { tableName, dbName });
        }

        public async Task<IActionResult> EditField(int index, string tableName, string dbName)
        {
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db.Tables.FirstOrDefault(table => table.Name == tableName);
            var tableFieldModel = new FieldCreateModel
            {
                DbName = dbName,
                Columns = table.Properties,
                TableName = tableName,
                Values = new Dictionary<string, dynamic>()
            };

            for (int i = 0; i < tableFieldModel.Columns.Count; i++)
            {
                foreach (var item in table.Data.ElementAt(i))
                {
                    tableFieldModel.Values.Add(tableFieldModel.Columns[i].Name, item);
                }
            }

            foreach (var value in tableFieldModel.Values.Where(v => tableFieldModel.Columns.FirstOrDefault(f => f.Type == PropertyType.StringInvl).Name == v.Key))
            {
                var splitVal = value.Value.Split(DatabaseContext.StringinvlSeparator);
                tableFieldModel.Values[value.Key] = new StringInVl()
                {
                    Min = splitVal[0],
                    Max = splitVal[1]
                };
            }
            return View(tableFieldModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditField(IFormCollection tableFieldModelCollection)
        {
            var tableName = tableFieldModelCollection["TableName"];
            var dbName = tableFieldModelCollection["DbName"];
            var db = await databaseContext.GetDatabaseAsync(dbName);
            var table = db.Tables.FirstOrDefault(table => table.Name == tableName);
            var dictValues = new Dictionary<Guid, dynamic>();
            var keys = tableFieldModelCollection.Keys.Where(x => table.Properties.Select(prop => prop.Name).Contains(x));
            var tableFieldModel = new FieldCreateModel
            {
                TableName = tableName,
                DbName = dbName,
                Columns = table.Properties,
                Values = new Dictionary<string, dynamic>()
            };
            foreach (var key in keys)
            {
                if (tableFieldModel.Columns.FirstOrDefault(x => x.Name == key).Type == PropertyType.StringInvl)
                {
                    var min = tableFieldModelCollection[key][0] ?? "";
                    var max = tableFieldModelCollection[key][1] ?? "";
                    if (!String.IsNullOrEmpty(tableFieldModelCollection[key][0]) && !String.IsNullOrEmpty(max) && String.Compare(min, max) > 0)
                    {
                        ViewData[$"Error_{key}"] = $"Interval is not valid";
                        return View(tableFieldModel);
                    }
                    tableFieldModel.Values.Add(key, $"{min}{DatabaseContext.StringinvlSeparator}{max}");
                }
                else
                {
                    tableFieldModel.Values.Add(key, tableFieldModelCollection[key].FirstOrDefault() ?? "");
                }
            }
            var htmlValues = tableFieldModel.Values.Where(v => table.Properties.FirstOrDefault(x => x.Name == v.Key).Type == PropertyType.Html);
            foreach (var html in htmlValues)
            {
                if (!HtmlValidator.IsValid(html.Value))
                {
                    ViewData[$"Error_{html.Key}"] = $"Html is not valid";
                    return View(tableFieldModel);
                }
            }
            if (ModelState.IsValid)
            {
                await databaseContext.UpdateRowInTableAsync(dbName, tableName, tableFieldModel.Values.Where(kv => kv.Value is string).Select(kv => (string)kv.Value).ToList(), tableFieldModel.Index);
                return RedirectToAction("Fields", new { tableName, dbName });
            }
            return View(tableFieldModel);
        }

        public async Task<IActionResult> JoinTables(string name)
        {
            var db = await databaseContext.GetDatabaseAsync(name);
            var model = new JoinTablesModel
            {
                DatabaseName = name
            };
            ViewBag.Database = db;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinTables(JoinTablesModel joinTablesModel)
        {
            if (!ModelState.IsValid)
            {
                return View(joinTablesModel);
            }
            else
            {
                var joinTable = await databaseContext.JoinTablesAsync(joinTablesModel.DatabaseName, joinTablesModel.FirstTableName, joinTablesModel.SecondTableName, joinTablesModel.FirstTableParameter, joinTablesModel.SecondTableParameter);
                await databaseContext.AddTableToDatabaseAsync(joinTablesModel.DatabaseName, joinTable);
                return RedirectToAction("Tables", new { name = joinTablesModel.DatabaseName });
            }
            return View(joinTablesModel);
        }
    }
}
