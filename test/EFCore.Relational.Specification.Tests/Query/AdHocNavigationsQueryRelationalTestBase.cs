﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class AdHocNavigationsQueryRelationalTestBase : AdHocNavigationsQueryTestBase
{
    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    #region 21803

    [ConditionalTheory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public virtual async Task Select_enumerable_navigation_backed_by_collection(bool async, bool split)
    {
        var contextFactory = await InitializeAsync<Context21803>(seed: c => c.Seed());
        using var context = contextFactory.CreateContext();
        var query = context.Set<Context21803.AppEntity>().Select(appEntity => appEntity.OtherEntities);

        if (split)
        {
            query = query.AsSplitQuery();
        }

        if (async)
        {
            await query.ToListAsync();
        }
        else
        {
            query.ToList();
        }
    }

    private class Context21803 : DbContext
    {
        public DbSet<AppEntity> Entities { get; set; }

        public Context21803(DbContextOptions options)
            : base(options)
        {
        }

        public void Seed()
        {
            var appEntity = new AppEntity();
            AddRange(
                new OtherEntity { AppEntity = appEntity },
                new OtherEntity { AppEntity = appEntity },
                new OtherEntity { AppEntity = appEntity },
                new OtherEntity { AppEntity = appEntity });

            SaveChanges();
        }

        public class AppEntity
        {
            private readonly List<OtherEntity> _otherEntities = new();

            public int Id { get; private set; }

            public IEnumerable<OtherEntity> OtherEntities
                => _otherEntities;
        }

        public class OtherEntity
        {
            public int Id { get; private set; }
            public AppEntity AppEntity { get; set; }
        }
    }

    #endregion
}
