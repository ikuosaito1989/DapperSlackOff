using System;
using System.ComponentModel.DataAnnotations;
using Dapper;
namespace SampleProgram
{
    public class Program
    {
        static void Main(string[] args)
        {
            var repository = new DapperSlackOff("", new string[] { "CreateTime" }, new string[] { "UpdateTime" });
            // var updateCount = repository.Update<Person> (new { Id = 1, Name = "saito" });
            // var deleteCount = repository.Delete<Person> (new { Id = 1, Name = "saito" });
            // var insertCount = repository.Insert<Person> (new { Name = "saito", Age = 29 });
            // var insertCount2 = repository.CreateOrUpdate<Person> (new Person () { Name = "saito", Age = 29 });
            var updateCount2 = repository.CreateOrUpdate<Person>(new Person() { Id = 1, Name = "saito", Age = 29 });
        }
    }
    public class Person
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
