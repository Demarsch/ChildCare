﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core;

namespace TestCore
{
    public class TestDataContext : IDataContext
    {
        private readonly Dictionary<Type, IEnumerable> data = new Dictionary<Type, IEnumerable>(); 

        public void Dispose()
        {
        }

        public void AddData<TData>(ICollection<TData> collection) where TData : class
        {
            data[typeof(TData)] = collection;
        }

        public IQueryable<TData> GetData<TData>() where TData : class
        {
            return data[typeof(TData)] as IQueryable<TData>;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Add<TData>(TData obj) where TData : class
        {
            throw new NotImplementedException();
        }
    }
}