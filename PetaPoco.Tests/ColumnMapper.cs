using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using PetaTest;

namespace PetaPoco.Tests
{
	public class Poco2
	{
		public string prop1 { get; set; }
		public string prop2 { get; set; }
		public string prop3 { get; set; }
		public string prop4 { get; set; }
	}

	public class MyColumnMapper : PetaPoco.IMapper
	{
		public void GetTableInfo(Type t, TableInfo ti)
		{
			if (t == typeof(Poco2))
			{
				ti.TableName = "petapoco";
				ti.PrimaryKey = "id";
			}
		}
		public bool MapPropertyToColumn(System.Reflection.PropertyInfo pi, ref string columnName, ref bool resultColumn)
		{
			if (pi.DeclaringType == typeof(Poco2))
			{
				switch (pi.Name)
				{
					case "prop1":
						// Leave this property as is
						return true;

					case "prop2":
						// Rename this column
						columnName = "remapped2";
						return true;

					case "prop3":
						// Mark this as a result column
						resultColumn = true;
						return true;

					case "prop4":
						// Ignore this property
						return false;
				}
			}

			// Do default property mapping
			return true;
		}


		public Func<object, object> GetFromDbConverter(System.Reflection.PropertyInfo pi, Type SourceType)
		{
			return null;
		}

		public Func<object, object> GetToDbConverter(Type SourceType)
		{
			return null;
		}
	}

	[TestFixture]
	public class ColumnMapper 
	{
        private class DbHandler : Database.ISpecificDatabaseHandler
        {
            public Action<IDbCommand> PreExecute
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public Action<IDbCommand> PostExecute
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public string EscapeSqlIdentifier(string sql)
            {
                return sql;
            }

            public string ParamPrefix
            {
                get { return "@"; }
            }

            public MethodInfo GetGetMethod(Type srcType)
            {
                throw new NotImplementedException();
            }

            public IDbCommand CreateCommand(IDbConnection connection, string sql, IDbTransaction transaction, params object[] args)
            {
                throw new NotImplementedException();
            }

            public void AddParam(IDbCommand cmd, object item, string ParameterPrefix)
            {
                throw new NotImplementedException();
            }

            public object InsertAutoincrement(IDbCommand cmd, string primaryKeyName, IDbTransaction transaction)
            {
                throw new NotImplementedException();
            }

            public object InsertAutoincrement(IDbCommand cmd, string primaryKeyName, Action<IDbCommand> preExecute, Action<IDbCommand> executedCommand)
            {
                throw new NotImplementedException();
            }
        }

	    [Test]
		public void NoColumnMapper()
		{

			PetaPoco.Database.Mapper = new MyColumnMapper();
			var pd=PetaPoco.Database.PocoData.ForType(typeof(Poco2), new DbHandler());

			Assert.AreEqual(pd.Columns.Count, 3);
			Assert.AreEqual(pd.Columns["prop1"].PropertyInfo.Name, "prop1");
			Assert.AreEqual(pd.Columns["remapped2"].ColumnName, "remapped2");
			Assert.AreEqual(pd.Columns["prop3"].ColumnName, "prop3");
			Assert.AreEqual(string.Join(", ", pd.QueryColumns), "prop1, remapped2");
			Assert.AreEqual(pd.TableInfo.PrimaryKey, "id");
			Assert.AreEqual(pd.TableInfo.TableName, "petapoco");
		}
	}
}
