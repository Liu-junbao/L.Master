﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB
{
    public class Model
    {
        [Key]
        public string Name { get; set; }
        public int Age { get; set; }
        public string Descripton { get; set; }
        public string Text { get; set; }
    }
}
