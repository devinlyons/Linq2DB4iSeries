using System;
using System.Linq;

using LinqToDB;
using LinqToDB.Mapping;

using NUnit.Framework;

namespace Tests.Linq
{
	[TestFixture]
	public class CharTypesTests : TestBase
	{
		[Table("ALLTYPES", Configuration = ProviderName.DB2)]
		[Table("AllTypes")]
		public class StringTestTable
		{
			[Column("ID")]
			public int Id;

			[Column("char20DataType")]
			[Column(Configuration = ProviderName.SqlCe,			 IsColumn = false)]
			[Column(Configuration = ProviderName.DB2,			 IsColumn = false)]
			[Column(Configuration = ProviderName.PostgreSQL,	 IsColumn = false)]
			[Column(Configuration = ProviderName.MySql,			 IsColumn = false)]
			[Column(Configuration = ProviderName.MySqlConnector, IsColumn = false)]
			[Column(Configuration = TestProvName.MySql55,        IsColumn = false)]
			[Column(Configuration = TestProvName.MariaDB,        IsColumn = false)]
			public string? String;

			[Column("ncharDataType")]
			[Column("nchar20DataType", Configuration = ProviderName.SapHana)]
			[Column("CHAR20DATATYPE" , Configuration = ProviderName.DB2)]
			[Column("char20DataType" , Configuration = ProviderName.PostgreSQL)]
			[Column("char20DataType" , Configuration = ProviderName.MySql)]
			[Column("char20DataType" , Configuration = ProviderName.MySqlConnector)]
			[Column("char20DataType" , Configuration = TestProvName.MySql55)]
			[Column("char20DataType" , Configuration = TestProvName.MariaDB)]
			[Column(                   Configuration = ProviderName.Firebird, IsColumn = false)]
			public string? NString;
		}

		[Table("ALLTYPES", Configuration = ProviderName.DB2)]
		[Table("AllTypes")]
		public class CharTestTable
		{
			[Column("ID")]
			public int Id;

			[Column("char20DataType")]
			[Column(Configuration = ProviderName.SqlCe,			 IsColumn = false)]
			[Column(Configuration = ProviderName.DB2,			 IsColumn = false)]
			[Column(Configuration = ProviderName.PostgreSQL,	 IsColumn = false)]
			[Column(Configuration = ProviderName.MySql,			 IsColumn = false)]
			[Column(Configuration = ProviderName.MySqlConnector, IsColumn = false)]
			[Column(Configuration = TestProvName.MySql55,		 IsColumn = false)]
			[Column(Configuration = TestProvName.MariaDB,		 IsColumn = false)]
			public char? Char;

			[Column("ncharDataType"  , DataType = DataType.NChar)]
			[Column("nchar20DataType", DataType = DataType.NChar, Configuration = ProviderName.SapHana)]
			[Column("CHAR20DATATYPE" , DataType = DataType.NChar, Configuration = ProviderName.DB2)]
			[Column("char20DataType" , DataType = DataType.NChar, Configuration = ProviderName.PostgreSQL)]
			[Column("char20DataType" , DataType = DataType.NChar, Configuration = ProviderName.MySql)]
			[Column("char20DataType" , DataType = DataType.NChar, Configuration = ProviderName.MySqlConnector)]
			[Column("char20DataType" , DataType = DataType.NChar, Configuration = TestProvName.MySql55)]
			[Column("char20DataType" , DataType = DataType.NChar, Configuration = TestProvName.MariaDB)]
			[Column(                   Configuration = ProviderName.Firebird, IsColumn = false)]
			public char? NChar;
		}

		// most of ending characters here trimmed by default by .net string TrimX methods
		// unicode test cases not used for String
		static readonly StringTestTable[] StringTestData =
		{
			new StringTestTable() { String = "test01",      NString = "test01"        },
			new StringTestTable() { String = "test02  ",    NString = "test02  "      },
			new StringTestTable() { String = "test03\x09 ", NString = "test03\x09 "   },
			new StringTestTable() { String = "test04\x0A ", NString = "test04\x0A "   },
			new StringTestTable() { String = "test05\x0B ", NString = "test05\x0B "   },
			new StringTestTable() { String = "test06\x0C ", NString = "test06\x0C "   },
			new StringTestTable() { String = "test07\x0D ", NString = "test07\x0D "   },
			new StringTestTable() { String = "test08\xA0 ", NString = "test08\xA0 "   },
			new StringTestTable() { String = "test09     ", NString = "test09\u2000 " },
			new StringTestTable() { String = "test10     ", NString = "test10\u2001 " },
			new StringTestTable() { String = "test11     ", NString = "test11\u2002 " },
			new StringTestTable() { String = "test12     ", NString = "test12\u2003 " },
			new StringTestTable() { String = "test13     ", NString = "test13\u2004 " },
			new StringTestTable() { String = "test14     ", NString = "test14\u2005 " },
			new StringTestTable() { String = "test15     ", NString = "test15\u2006 " },
			new StringTestTable() { String = "test16     ", NString = "test16\u2007 " },
			new StringTestTable() { String = "test17     ", NString = "test17\u2008 " },
			new StringTestTable() { String = "test18     ", NString = "test18\u2009 " },
			new StringTestTable() { String = "test19     ", NString = "test19\u200A " },
			new StringTestTable() { String = "test20     ", NString = "test20\u3000 " },
			new StringTestTable() { String = "test21\0   ", NString = "test21\0 "     },
			new StringTestTable()
		};

		// need to configure sybase docker image to use utf8 character set
		[ActiveIssue(Configuration = TestProvName.AllSybase)]
		[Test]
		public void StringTrimming([DataSources(TestProvName.AllInformix)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var lastId = db.GetTable<StringTestTable>().Select(_ => _.Id).Max();

				try
				{
					var testData = GetStringData(context);

					foreach (var record in testData)
					{
						var query = db.GetTable<StringTestTable>().Value(_ => _.NString, record.NString);

						if (!SkipChar(context))
							query = query.Value(_ => _.String, record.String);

						if (   context == ProviderName.Firebird
							|| context == ProviderName.Firebird + ".LinqService"
							|| context == TestProvName.Firebird3
							|| context == TestProvName.Firebird3 + ".LinqService")
							query = db.GetTable<StringTestTable>().Value(_ => _.String, record.String);

						query.Insert();
					}

					var records = db.GetTable<StringTestTable>().Where(_ => _.Id > lastId).OrderBy(_ => _.Id).ToArray();

					Assert.AreEqual(testData.Length, records.Length);

					for (var i = 0; i < records.Length; i++)
					{
						if (!SkipChar(context))
						{
							if (context.Contains("Sybase"))
								Assert.AreEqual(testData[i].String?.TrimEnd(' ')?.TrimEnd('\0'), records[i].String);
							else
								Assert.AreEqual(testData[i].String?.TrimEnd(' '), records[i].String);
						}

						if (context != ProviderName.Firebird
							  && context != ProviderName.Firebird + ".LinqService"
							  && context != TestProvName.Firebird3
							  && context != TestProvName.Firebird3 + ".LinqService")
						{
							if (context.Contains("Sybase"))
								Assert.AreEqual(testData[i].NString?.TrimEnd(' ')?.TrimEnd('\0'), records[i].NString);
							else
								Assert.AreEqual(testData[i].NString?.TrimEnd(' '), records[i].NString);
						}
					}

				}
				finally
				{
					db.GetTable<StringTestTable>().Where(_ => _.Id > lastId).Delete();
				}
			}
		}

		private CharTestTable[] GetCharData([DataSources] string context)
		{
			var provider = GetProviderName(context, out var _);

			// filter out null-character test cases for servers/providers without support
			if (   context.Contains(ProviderName.PostgreSQL)
				|| provider == ProviderName.DB2
				|| provider == ProviderName.SqlCe
				|| context.Contains(ProviderName.SapHana))
				return CharTestData.Where(_ => _.NChar != '\0').ToArray();

			// I wonder why
			if (context.Contains(ProviderName.Firebird))
				return CharTestData.Where(_ => _.NChar != '\xA0').ToArray();

			// also strange
			if (context.Contains(TestProvName.AllInformix))
				return CharTestData.Where(_ => _.NChar != '\0' && (_.NChar ?? 0) < byte.MaxValue).ToArray();

			return CharTestData;
		}

		private StringTestTable[] GetStringData([DataSources] string context)
		{
			var provider = GetProviderName(context, out var _);

			// filter out null-character test cases for servers/providers without support
			if (context.Contains(ProviderName.PostgreSQL)
				|| provider == ProviderName.DB2
				|| context  == ProviderName.DB2           + ".LinqService"
				|| context.Contains("SQLite")
				|| provider == ProviderName.SqlCe
				|| context.Contains(ProviderName.SapHana))
				return StringTestData.Where(_ => !(_.NString ?? string.Empty).Contains("\0")).ToArray();

			// I wonder why
			if (context.Contains(ProviderName.Firebird))
				return StringTestData.Where(_ => !(_.NString ?? string.Empty).Contains("\xA0")).ToArray();

			// also strange
			if (context.Contains(TestProvName.AllInformix))
				return StringTestData.Where(_ => !(_.NString ?? string.Empty).Contains("\0")
					&& !(_.NString ?? string.Empty).Any(c => (int)c > byte.MaxValue)).ToArray();

			return StringTestData;
		}

		static readonly CharTestTable[] CharTestData =
		{
			new CharTestTable() { Char = ' ',    NChar = ' '      },
			new CharTestTable() { Char = '\x09', NChar = '\x09'   },
			new CharTestTable() { Char = '\x0A', NChar = '\x0A'   },
			new CharTestTable() { Char = '\x0B', NChar = '\x0B'   },
			new CharTestTable() { Char = '\x0C', NChar = '\x0C'   },
			new CharTestTable() { Char = '\x0D', NChar = '\x0D'   },
			new CharTestTable() { Char = '\xA0', NChar = '\xA0'   },
			new CharTestTable() { Char = ' ',    NChar = '\u2000' },
			new CharTestTable() { Char = ' ',    NChar = '\u2001' },
			new CharTestTable() { Char = ' ',    NChar = '\u2002' },
			new CharTestTable() { Char = ' ',    NChar = '\u2003' },
			new CharTestTable() { Char = ' ',    NChar = '\u2004' },
			new CharTestTable() { Char = ' ',    NChar = '\u2005' },
			new CharTestTable() { Char = ' ',    NChar = '\u2006' },
			new CharTestTable() { Char = ' ',    NChar = '\u2007' },
			new CharTestTable() { Char = ' ',    NChar = '\u2008' },
			new CharTestTable() { Char = ' ',    NChar = '\u2009' },
			new CharTestTable() { Char = ' ',    NChar = '\u200A' },
			new CharTestTable() { Char = ' ',    NChar = '\u3000' },
			new CharTestTable() { Char = '\0',   NChar = '\0'     },
			new CharTestTable()
		};

		// need to configure sybase docker image to use utf8 character set
#if AZURE
		[ActiveIssue(Configuration = TestProvName.AllSybase)]
#endif
		[Test]
		public void CharTrimming([DataSources(TestProvName.AllInformix)] string context)
		{
			using (var db = GetDataContext(context))
			{
				var lastId = db.GetTable<CharTestTable>().Select(_ => _.Id).Max();

				try
				{
					var testData = GetCharData(context);

					foreach (var record in testData)
					{
						var query = db.GetTable<CharTestTable>().Value(_ => _.NChar, record.NChar);
						if (!SkipChar(context))
							query = query.Value(_ => _.Char, record.Char);

						if (context.Contains(ProviderName.Firebird))
							query = db.GetTable<CharTestTable>().Value(_ => _.Char, record.Char);

						query.Insert();
					}

					var records = db.GetTable<CharTestTable>().Where(_ => _.Id > lastId).OrderBy(_ => _.Id).ToArray();

					Assert.AreEqual(testData.Length, records.Length);

					for (var i = 0; i < records.Length; i++)
					{
						if (context.StartsWith(ProviderName.SapHana))
						{
							// SAP or provider trims space and we return default value, which is \0 for char
							// or we insert it incorrectly?
							if (testData[i].Char == ' ')
								Assert.AreEqual('\0', records[i].Char);
							else
								Assert.AreEqual(testData[i].Char, records[i].Char);

							if (testData[i].NChar == ' ')
								Assert.AreEqual('\0', records[i].NChar);
							else
								Assert.AreEqual(testData[i].NChar, records[i].NChar);

							continue;
						}

						if (!SkipChar(context))
						{
							if (context.Contains("Sybase"))
								Assert.AreEqual(testData[i].Char == '\0' ? ' ' : testData[i].Char, records[i].Char);
							else
								Assert.AreEqual(testData[i].Char, records[i].Char);
						}

						if (context == ProviderName.MySql
							  || context == ProviderName.MySql + ".LinqService"
							  || context == ProviderName.MySqlConnector
							  || context == ProviderName.MySqlConnector + ".LinqService"
							  || context == TestProvName.MySql55
							  || context == TestProvName.MySql55 + ".LinqService"
							  || context == TestProvName.MariaDB
							  || context == TestProvName.MariaDB + ".LinqService")
							// for some reason mysql doesn't insert space
							Assert.AreEqual(testData[i].NChar == ' ' ? '\0' : testData[i].NChar, records[i].NChar);
						else if (!context.Contains(ProviderName.Firebird))
						{
							if (context.Contains("Sybase"))
								Assert.AreEqual(testData[i].NChar == '\0' ? ' ' : testData[i].NChar, records[i].NChar);
							else
								Assert.AreEqual(testData[i].NChar, records[i].NChar);
						}
					}
				}
				finally
				{
					db.GetTable<CharTestTable>().Where(_ => _.Id > lastId).Delete();
				}
			}
		}

		private static bool SkipChar([DataSources] string context)
		{
			return context == ProviderName.SqlCe
				|| context == ProviderName.SqlCe      + ".LinqService"
				|| context == ProviderName.DB2
				|| context == ProviderName.DB2        + ".LinqService"
				|| context.Contains(ProviderName.PostgreSQL)
				|| context == ProviderName.MySql
				|| context == ProviderName.MySql + ".LinqService"
				|| context == ProviderName.MySqlConnector
				|| context == ProviderName.MySqlConnector + ".LinqService"
				|| context == TestProvName.MySql55
				|| context == TestProvName.MySql55 + ".LinqService"
				|| context == TestProvName.MariaDB
				|| context == TestProvName.MariaDB    + ".LinqService";
		}
	}
}
