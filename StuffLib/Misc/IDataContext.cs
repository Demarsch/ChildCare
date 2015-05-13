﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public interface IDataContext : IDisposable
    {
        IQueryable<TData> GetData<TData>() where TData : class;
        
        void Add<TData>(TData obj) where TData : class;

        DateTime GetCurrentDate();

        void AddRange<TData>(IEnumerable<TData> objs) where TData : class;
        
        void Remove<TData>(TData obj) where TData : class;

        void RemoveRange<TData>(IEnumerable<TData> objs) where TData : class;

        void Update<TData>(TData obj) where TData : class;

        void Save();

    }
}
