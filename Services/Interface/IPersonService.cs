﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLib;

namespace Core
{
    public interface IPersonService
    {
        IPersonService(ISimpleLocator serviceLocator);

        Person PersonById(int Id);
    }
}
