using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.ComponentModel;

namespace Dataway.DataAccess
{
    /// <summary>
    /// Use convention binding to automatically commit and pull data from a database.  
    /// This assumes the model properties and database columns match exactly.
    /// </summary>
    /// <typeparam name="T">Model type to operate with.</typeparam>
    public class DatabaseModelFactory<T> where T : new()
    {
        #region Private Members

        private string primaryKey;

        private FieldBindingCollection bindings;

        private FieldBindingCollection evaluatedBindings;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new factory, this is best kept in memory to allow caching on bindings and primary key.
        /// </summary>
        /// <param name="tableName">Name of the table the model belongs to.</param>
        /// <param name="connectionString">Connection string.</param>
        public DatabaseModelFactory(string tableName, string connectionString)
        {
            this.ConnectionString = connectionString;
            this.Table = tableName;
        }

        /// <summary>
        /// Construct a new factory, this is best kept in memory to allow caching on bindings and primary key.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        public DatabaseModelFactory(string connectionString)
            : this(GetPlural(typeof(T).Name), connectionString) { }

        #endregion

        #region Public Properties

        /// <summary>
        /// Get or set the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Get or set the database table name.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Get or set the database table primary key name.
        /// </summary>
        public string PrimaryKey
        {
            get
            {
                if (string.IsNullOrEmpty(this.primaryKey))
                {
                    this.primaryKey = GetPrimaryKey(this.Table, this.ConnectionString);
                }
                return this.primaryKey;
            }
            //set
            //{
            //	this.primaryKey = value;
            //}
        }

        /// <summary>
        /// Get or set the bindings for custom model to table column matching.
        /// </summary>
        public FieldBindingCollection Bindings
        {
            get
            {
                if (this.bindings == null)
                {
                    this.bindings = new FieldBindingCollection();
                }
                return this.bindings;
            }
            set
            {
                this.bindings = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get a single model from the database table, SELECT * FROM [Table] WHERE [PrimaryKey] = [id].
        /// </summary>
        /// <param name="id">ID of object to get.</param>
        /// <returns>Object of type T.</returns>
        public T Get(int id)
        {
            return Query.GetFirst<T>(string.Format("SELECT * FROM [{0}] WHERE [{1}]=@0", this.Table, this.PrimaryKey), this.ConnectionString, GetFromRow, id);
        }

        /// <summary>
        /// Get all models of type T from the database table, SELECT * FROM [Table]
        /// </summary>
        /// <returns>A list of type T.</returns>
        public List<T> GetAll()
        {
            return Query.GetList<T>(string.Format("SELECT  * FROM [{0}]", this.Table), this.ConnectionString, GetFromRow);
        }

        /// <summary>
        /// Get a list of models using a custom query, due to auto binding its best to use SELECT * here
        /// </summary>
        /// <param name="query">The query to get the rows from the database</param>
        /// <param name="parameters">The parameters used in the condition, these can be referenced in order ie @0, @1, @2 etc</param>
        /// <returns>A list of models</returns>
        public List<T> QueryList(string query, params object[] parameters)
        {
            return Query.GetList<T>(query, this.ConnectionString, GetFromRow, parameters);
        }

        /// <summary>
        /// Get the first or default value of type T for a custom query, due to auto binding its best to use SELECT * here
        /// </summary>
        /// <param name="query">The query to get the row from the database</param>
        /// <param name="parameters">The parameters used in the condition, these can be referenced in order ie @0, @1, @2 etc</param>
        /// <returns>A model of type T or its default value if no matching rows were found</returns>
        public T FirstOrDefault(string query, params object[] parameters)
        {
            return Query.GetFirst<T>(query, this.ConnectionString, GetFromRow, parameters);
        }

        /// <summary>
        /// Executes and returns the first column value from the first row of the matching result and type S
        /// </summary>
        /// <typeparam name="S">Type to return</typeparam>
        /// <param name="query">SQL query to obtain the data</param>
        /// <param name="parameters">The parameters used in the condition, these can be referenced in order ie @0, @1, @2 etc</param>
        /// <returns>Returns value of type S</returns>
        public S ExecuteScalar<S>(string query, params object[] parameters)
        {
            return Query.ExecuteScalar<S>(query, this.ConnectionString, parameters);
        }

        /// <summary>
        /// Executes a custom SQL query
        /// </summary>
        /// <param name="query">SQL query to execute</param>
        /// <param name="parameters">The parameters used in the condition, these can be referenced in order ie @0, @1, @2 etc</param>
        public void ExecuteNonQuery(string query, params object[] parameters)
        {
            Query.ExecuteNonQuery(query, this.ConnectionString, parameters);
        }

        /// <summary>
        /// Delete a single model from the database table, DELETE FROM [Table] WHERE [PrimaryKey] = [id].
        /// </summary>
        /// <param name="id">Id of the object to delete.</param>
        public void Delete(int id)
        {
            Delete(this.Table, this.PrimaryKey, id, this.ConnectionString);
        }

        /// <summary>
        /// Commit a model to the database, this will use insert if the primary key value is 0 otherwise it will update all the fields where the primary key macthes.
        /// </summary>
        /// <param name="model">Object to commit to the database.</param>
        public void Save(T model)
        {
            Save(model, this.Table, this.ConnectionString, this.PrimaryKey, this.GetEvaluatedBindings());
        }

        /// <summary>
        /// Insert a model into the database
        /// </summary>
        /// <param name="model">The model to insert</param>
        public void Insert(T model)
        {
            Insert(model, this.Table, this.ConnectionString, this.PrimaryKey, this.GetEvaluatedBindings());
        }

        /// <summary>
        /// Insert a model into the database using an anonymous object as the columns and values
        /// </summary>
        /// <param name="insertPropertyValues">Anonymous object to represent the column values ie new { Name = "Timothy" } will resolve to INSERT INTO Table (Timothy) VALUES (@0)</param>
        /// <returns>New ID from SELECT SCOPE_IDENTITY()</returns>
        public int Insert(object insertPropertyValues)
        {
            Query q = new Query(this.ConnectionString);

            q.QueryCommand = "INSERT INTO [" + this.Table + "] (";

            string fields = "";
            string paramVals = "";

            int i = 0;
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(insertPropertyValues))
            {
                fields += string.Format("[{0}],", property.Name); // "[" + property.Name + "],";
                paramVals += string.Format("@{0},", i); //"@" + i + ",";

                q.AddParameter(i.ToString(), property.GetValue(insertPropertyValues));
                i++;
            }

            q.QueryCommand = string.Format("INSERT INTO {0} ({1}) VALUES ({2}) SELECT SCOPE_IDENTITY();", this.Table, fields.TrimEnd(','), paramVals.TrimEnd(','));

            return q.ExecuteScalar<int>();
        }

        /// <summary>
        /// Update an existing model in the database, this will be based on matching the primary key
        /// </summary>
        /// <param name="model">The model to update</param>
        public void Update(T model)
        {
            Update(model, this.Table, this.ConnectionString, this.PrimaryKey, this.GetEvaluatedBindings());
        }

        /// <summary>
        /// Update an existing model setting custom properties using an anonymous object and a where condition
        /// </summary>
        /// <param name="setPropertiesObj">Anonymous object for setting columns ie new { Name = "Timothy" } do SET Name=@updateParam0 and add the paramter to the Sql Paramters collection</param>
        /// <param name="where">Condition to apply the update</param>
        /// <param name="whereParameters">Parameters used for the where statement</param>
        public void Update(object setPropertiesObj, string where, params object[] whereParameters)
        {
            Query q = new Query(this.ConnectionString);

            string setFieldValues = "";
            int i = 0;
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(setPropertiesObj))
            {
                var paramName = string.Format("updateParam{0}", i);
                setFieldValues += string.Format("[{0}]={1},", property.Name, paramName);
                q.AddParameter(paramName, property.GetValue(setPropertiesObj));
                i++;
            }

            i = 0;
            foreach (var val in whereParameters)
            {
                q.AddParameter(i.ToString(), val);
                i++;
            }

            q.QueryCommand = string.Format("UPDATE {0} SET {1} WHERE {2}", this.Table, setFieldValues.TrimEnd(','), where);
            q.ExecuteNonQuery();
        }

        /// <summary>
        /// Get a list of model using a custom SQL where clause
        /// </summary>
        /// <param name="condition">Where condition, does not require the word 'WHERE' to be added</param>
        /// <param name="parameters">The parameters used in the condition, these can be referenced in order ie @0, @1, @2 etc</param>
        /// <returns>A list of models</returns>
        public List<T> Where(string condition, params object[] parameters)
        {
            return Query.GetList<T>(string.Format("SELECT * FROM {0} WHERE {1}", Table, condition), this.ConnectionString, GetFromRow, parameters);
        }

        /// <summary>
        /// Get the first model using a custom SQL where clase, will return the default value of T if no results match
        /// </summary>
        /// <param name="condition">Where condition, does not require the word 'WHERE' to be added</param>
        /// <param name="parameters">The parameters used in the condition, these can be referenced in order ie @0, @1, @2 etc</param>
        /// <returns>Single model</returns>
        public T FirstWhere(string condition, params object[] parameters)
        {
            return Query.GetFirst<T>(string.Format("SELECT * FROM {0} WHERE {1}", Table, condition), this.ConnectionString, GetFromRow, parameters);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Commit an object to the database using conventions, models will be inserted if their ID is 0, otherwise they'll be updated.
        /// </summary>
        /// <param name="model">Object to commit to the database.</param>
        /// <param name="tableName">Name of the table the object belongs too.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="primaryKey">Primary key name.</param>
        /// <param name="evaluatedBindings">Bindings</param>
        /// <returns>The object with it's new ID if relevant.</returns>
        public static object Save(object model, string tableName, string connectionString, string primaryKey, List<FieldBinding> evaluatedBindings)
        {
            var pk = evaluatedBindings.FirstOrDefault(b => b.DatabaseColumn == primaryKey);

            int? id = null;

            if (pk == null)
            {
                id = model.GetType().GetProperty(primaryKey).GetValue(model, null) as int?;
            }
            else
            {
                id = model.GetType().GetProperty(evaluatedBindings.First(b => b.DatabaseColumn == primaryKey).ModelField).GetValue(model, null) as int?;
            }

            if (!id.HasValue)
            {
                throw new NotSupportedException("Could not find primary key.");
            }
            else if (id.Value == 0)
            {
                return Insert(model, tableName, connectionString, primaryKey, evaluatedBindings);
            }
            else
            {
                return Update(model, tableName, connectionString, primaryKey, evaluatedBindings);
            }
        }

        /// <summary>
        /// Inserts an object in to the database using conventions.
        /// </summary>
        /// <param name="model">Object to commit to the database.</param>
        /// <param name="tableName">Name of the table to insert the object in to.</param>
        /// <param name="connectionString">Connection string.</param>
        /// /// <param name="primaryKey">Primary key name.</param>
        /// /// <param name="evaluationBindings">Bindings.</param>
        /// <returns>The object with it's new ID set.</returns>
        public static object Insert(object model, string tableName, string connectionString, string primaryKey, List<FieldBinding> evaluationBindings)
        {
            Query q = new Query(connectionString);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("INSERT INTO [{0}]", tableName);

            string columns = "";
            string paramters = "";

            int index = 0;
            string modelIdPropertyName = "";

            foreach (var binding in evaluationBindings)
            {
                if (binding.DatabaseColumn == primaryKey)
                {
                    modelIdPropertyName = binding.ModelField;
                }
                else
                {
                    columns += string.Format("[{0}],", binding.DatabaseColumn);// binding.DatabaseColumn + ",";
                    paramters += string.Format("@{0},", index); //"@" + index + ",";
                    q.AddParameter(index.ToString(), model.GetType().GetProperty(binding.ModelField).GetValue(model, null));
                    index++;
                }
            }

            sb.AppendFormat(" ({0}) VALUES ({1})", columns.TrimEnd(','), paramters.TrimEnd(','));
            sb.Append(" SELECT SCOPE_IDENTITY();");

            q.QueryCommand = sb.ToString();

            int id = q.ExecuteScalar<int>();
            var primProperty = model.GetType().GetProperty(modelIdPropertyName);
            primProperty.SetValue(model, id, null);

            return model;
        }

        /// <summary>
        /// Updates an object in the database using conventions.
        /// </summary>
        /// <param name="model">Object to update in the database.</param>
        /// <param name="tableName">Name of the table to update.</param>
        /// <param name="connectionString">Connection string.</param>
        /// <param name="primaryKey">Primary key name.</param>
        /// <param name="evaluationBindings">Bindings.</param>
        /// <returns>Returns the object.</returns>
        public static object Update(object model, string tableName, string connectionString, string primaryKey, List<FieldBinding> evaluationBindings)
        {
            Query q = new Query(connectionString);

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("UPDATE [{0}]", tableName);

            var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            string setValues = "";

            int primaryKeyIdVal = 0;

            int index = 0;

            foreach (var binding in evaluationBindings)
            {
                if (binding.DatabaseColumn == primaryKey)
                {
                    primaryKeyIdVal = (int)model.GetType().GetProperty(binding.ModelField).GetValue(model, null);
                    q.AddParameter("Id", primaryKeyIdVal);
                }
                else
                {
                    //setValues += string.Format binding.DatabaseColumn + "=@" + index + ",";
                    setValues += string.Format("[{0}]=@{1}, ", binding.DatabaseColumn, index); // binding.DatabaseColumn + "=@" + index + ",";
                    q.AddParameter(index.ToString(), model.GetType().GetProperty(binding.ModelField).GetValue(model, null));
                    index++;
                }
            }

            sb.AppendFormat(" SET {0}", setValues.Trim().TrimEnd(','));
            sb.AppendFormat(" WHERE [{0}]=@Id", primaryKey);

            q.QueryCommand = sb.ToString();

            int id = q.ExecuteNonQuery();
            return model;
        }

        /// <summary>
        /// Delete an object from the database.
        /// </summary>
        /// <param name="tableName">Name of the table to remove the row from.</param>
        /// <param name="primaryKeyColumn">Primary key name to delete by.</param>
        /// <param name="id">ID value to delete.</param>
        /// <param name="connectionString">Connection string.</param>
        public static void Delete(string tableName, string primaryKeyColumn, int id, string connectionString)
        {
            Query.ExecuteNonQuery(string.Format("DELETE FROM [{0}] WHERE [{1}]=@0", tableName, primaryKeyColumn), connectionString, id);
        }

        #endregion

        #region Private Methods

        private List<FieldBinding> GetEvaluatedBindings()
        {
            if (evaluatedBindings == null)
            {
                var properties = typeof(T).GetProperties();

                var columns = GetTableColumns(this.Table, this.ConnectionString);

                this.evaluatedBindings = new FieldBindingCollection();

                foreach (var property in properties)
                {
                    if (columns.Contains(property.Name))
                    {
                        this.evaluatedBindings.Add(property.Name, property.Name);
                    }
                }

                this.evaluatedBindings.AddRange(this.Bindings);
            }

            return this.evaluatedBindings;
        }

        public event Dataway.DataAccess.DBComm.GetModelFromRow<T> GetModelFromRow;

        /// <summary>
        /// Products a model of type T from the
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public T GetFromRow(DataRow row)
        {
            if (GetModelFromRow != null)
            {
                return GetModelFromRow(row);
            }

            if (row == null)
            {
                return default(T);
            }

            var model = new T();

            // automatically set matching fields
            var properties = model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    object val = row[property.Name];
                    if (val == null || DBNull.Value.Equals(val))
                    {
                        val = default(T);
                    }
                    property.SetValue(model, val, null);
                }
            }

            // Bind custom bound fields
            if (bindings != null && bindings.Any())
            {
                foreach (var binding in bindings)
                {
                    var property = properties.FirstOrDefault(p => p.Name == binding.ModelField);
                    if (property != null)
                    {
                        object val = row[binding.DatabaseColumn];

                        if (val == null || DBNull.Value.Equals(val))
                        {
                            val = default(T);
                        }

                        property.SetValue(model, val, null);
                    }
                }
            }

            return model;
        }

        #endregion

        #region Private Static Methods

        private static List<string> GetPrimaryKeys(string tableName, string connectionString)
        {
            List<string> columns = new List<string>();

            return Query.GetList<string>(@"
				SELECT column_name FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
				WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1
				AND table_name = @0", connectionString, r => r[0].ToString(), tableName);
        }

        private static List<string> GetTableColumns(string tableName, string connectionString)
        {
            return Query.GetList<string>(@"
                SELECT column_name,* FROM information_schema.columns
                WHERE table_name = @0
                ORDER BY ordinal_position", connectionString, r => r[0].ToString(), tableName);
        }

        private static string GetPrimaryKey(string tableName, string connectionString)
        {
            var primaryKeys = GetPrimaryKeys(tableName, connectionString);

            if (primaryKeys.Count == 1)
            {
                return primaryKeys[0];
            }
            else if (primaryKeys.Count > 1)
            {
                string pkStr = "";
                foreach (var key in primaryKeys)
                {
                    pkStr += key + ", ";
                }
                pkStr = pkStr.TrimEnd(',');
                throw new NotSupportedException(string.Format("There are more than one primary keys on table '{0}', the primary keys are: {1}", tableName, pkStr));
            }
            throw new NotSupportedException(string.Format("No primary keys found for table '{0}'", tableName));
        }

        private static string GetPlural(string stringObj)
        {
            if ((stringObj.EndsWith("x", StringComparison.OrdinalIgnoreCase) || stringObj.EndsWith("ch", StringComparison.OrdinalIgnoreCase)) || (stringObj.EndsWith("ss", StringComparison.OrdinalIgnoreCase) || stringObj.EndsWith("sh", StringComparison.OrdinalIgnoreCase)))
            {
                stringObj = stringObj + "es";
                return stringObj;
            }
            if ((stringObj.EndsWith("y", StringComparison.OrdinalIgnoreCase) && (stringObj.Length > 1)) && !IsVowel(stringObj[stringObj.Length - 2]))
            {
                stringObj = stringObj.Remove(stringObj.Length - 1, 1);
                stringObj = stringObj + "ies";
                return stringObj;
            }
            if (!stringObj.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                stringObj = stringObj + "s";
            }
            return stringObj;
        }

        private static bool IsVowel(char c)
        {
            switch (c)
            {
                case 'O':
                case 'U':
                case 'Y':
                case 'A':
                case 'E':
                case 'I':
                case 'o':
                case 'u':
                case 'y':
                case 'a':
                case 'e':
                case 'i':
                    return true;
            }
            return false;
        }

        #endregion
    }

    /// <summary>
    /// Bind a objects property to a database column.
    /// </summary>
    public class FieldBinding
    {
        /// <summary>
        /// Construct a new binding.
        /// </summary>
        /// <param name="modelField">Object property name.</param>
        /// <param name="databaseColumn">Table column name.</param>
        public FieldBinding(string modelField, string databaseColumn)
        {
            this.ModelField = modelField;
            this.DatabaseColumn = databaseColumn;
        }

        /// <summary>
        /// Object property name.
        /// </summary>
        public string ModelField { get; set; }

        /// <summary>
        /// Database table column name.
        /// </summary>
        public string DatabaseColumn { get; set; }
    }

    /// <summary>
    /// Represents a collection of field bindings.
    /// </summary>
    public class FieldBindingCollection : List<FieldBinding>
    {
        /// <summary>
        /// Add new field binding
        /// </summary>
        /// <param name="propertyName">Name of the property on the class model</param>
        /// <param name="columnName">Name of the column in the database</param>
        public void Add(string propertyName, string columnName)
        {
            Add(new FieldBinding(propertyName, columnName));
        }
    }
}