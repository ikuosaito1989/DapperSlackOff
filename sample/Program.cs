using System;
using System.ComponentModel.DataAnnotations;
using Dapper;

namespace sample
{
    class Program
    {
        private static readonly DapperSlackOff _repository = new DapperSlackOff(""
                , new string[] { "CreateTime" }, new string[] { "UpdateTime" });

        static void Main(string[] args)
        {
            // var selectValue = _repository.Get<Person> (new { Test = "saito" });
            var insertValue = _repository.Insert<Person>(new { Name = "saito", Age = 29 });
            // var updateValue = _repository.Update<Person> (new Person() { Id = 3, Name = "sato", Age = 30 });
        }
    }

    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
    }
}
