using System.Collections.Generic;
using System.Data.SQLite;
using Dapper;
using SqlKata.Compilers;
using SqlKata.Execution;
using SqlKata.Tests.Infrastructure;
using Xunit;

namespace SqlKata.Tests.Sqlite
{
    public class SqliteLimitTests : TestSupport
    {
        private readonly SqliteCompiler compiler;

        public SqliteLimitTests()
        {
            compiler = Compilers.Get<SqliteCompiler>(EngineCodes.Sqlite);
        }

        [Fact]
        public void WithNoLimitNorOffset()
        {
            var query = new Query("Table");
            var ctx = new SqlResult { Query = query };

            Assert.Null(compiler.CompileLimit(ctx));
        }

        [Fact]
        public void WithNoOffset()
        {
            var query = new Query("Table").Limit(10);
            var ctx = new SqlResult { Query = query };

            Assert.Equal("LIMIT ?", compiler.CompileLimit(ctx));
            Assert.Equal(10, ctx.Bindings[0]);
        }

        [Fact]
        public void WithNoLimit()
        {
            var query = new Query("Table").Offset(20);
            var ctx = new SqlResult { Query = query };

            Assert.Equal("LIMIT -1 OFFSET ?", compiler.CompileLimit(ctx));
            Assert.Equal(20, ctx.Bindings[0]);
            Assert.Single(ctx.Bindings);
        }

        [Fact]
        public void WithLimitAndOffset()
        {
            var query = new Query("Table").Limit(5).Offset(20);
            var ctx = new SqlResult { Query = query };

            Assert.Equal("LIMIT ? OFFSET ?", compiler.CompileLimit(ctx));
            Assert.Equal(5, ctx.Bindings[0]);
            Assert.Equal(20, ctx.Bindings[1]);
            Assert.Equal(2, ctx.Bindings.Count);
        }

        [Fact]
        public void AsInsert()
        {
            using var connection = new SQLiteConnection("Data Source=:memory:;New=True;");
            connection.Open();
            connection.Execute("CREATE TABLE TestTable (id INTEGER PRIMARY KEY,TestColumn int);");
            
            IEnumerable<KeyValuePair<string, object>> data = new[]
            {
                new KeyValuePair<string, object>("TestColumn", 1)
            };
            var query = new Query("TestTable").AsInsert(data, true);
            using var queryFactory = new QueryFactory(connection, compiler);
            var sqlResult = compiler.Compile(query);
            var result = connection.Query<int>(sqlResult.Sql, sqlResult.Bindings);

            var result1 = queryFactory(query);
            //result = queryFactory.Execute(query);
            //result = queryFactory.Execute(query);
        }
    }
}
