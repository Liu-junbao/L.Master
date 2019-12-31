using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleB
{
    [GenericName("实体模型")]
    public class Model
    {
        [GenericName("序号")]
        public int Id { get; set; }
        [GenericName("名称")]
        public string Name { get; set; }
        [GenericName("年龄",false)]
        public int Age { get; set; }
        [GenericName("描述",false)]
        public string Descripton { get; set; }
        public string Text { get; set; }


        public override string ToString() => $"{Id}:{Name}";
    }
}
