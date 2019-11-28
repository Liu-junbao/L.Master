using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterDemo
{
    public class Model
    {
        public Model(int id)
        {
            Id = id;
        }
        public int Id { get; }
        public string Name { get; set; }
        public string Group { get; set; }
    }
}
