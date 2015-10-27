﻿using System.Collections.Generic;

namespace Core.Services
{
    public interface ICacheService
    {
        TData GetItemById<TData>(int id) where TData : class;

        TData GetItemByName<TData>(string name) where TData : class;

        IEnumerable<TData> GetItems<TData>() where TData : class;
    }
}