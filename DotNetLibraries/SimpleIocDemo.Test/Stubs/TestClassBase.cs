﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleIocDemo.Test.Stubs
{
    public class TestClassBase
    {
        public void Remove()
        {
            SimpleIoc.Default.Unregister(this);
        }
    }
}
