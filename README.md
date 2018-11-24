# DapperSlackOff
Dapperのめんどくさいところを楽にする拡張ライブラリ

## Usage
```c#
using Dapper;
services.AddSingleton<IDapperSlackOff> (_ =>
    new DapperSlackOff (string connectionString, string[] creationDateColumns, string[] updateDateColumn));
```
- connectionString

接続文字列

- creationDateColumns

自動で作成日を更新するカラム名

- updateDateColumn

自動で更新日を更新するカラム名

### Dependency Injection 

```c#
public class Test {
        
    private IDapperSlackOff _repository;

    public Test (IDapperSlackOff repository) {
        _repository = repository;
    }
}
```

## Select

```c#
IEnumerable<T> Get<T> (object entity = null, bool conditions = true);
```
### サンプル

```c#
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
```
#### 全件検索
```c#
var person = _repository.Get<Person> ();
```
SQL
```sql
Select * From Person
```
#### 条件あり（AND）
```c#
var person = _repository.Get<Person> (new { Id = 1, Age = 29 });
```
SQL
```sql
Select * From Person Where Id = 1 AND Age = 29
```

#### 条件あり（OR）
```c#
var person = _repository.Get<Person> (new { Id = 1, Age = 29 }, false);
```
SQL
```sql
Select * From Person Where Id = 1 OR Age = 29
```

## Select List
```c#
public class Person
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
}
```
```c#
IEnumerable<T1> GetList<T1, T2> (object lists, string keyName = null)
```
- T1

戻り値の型

- T2

listsの型。IEnumerable<int>の場合はint

#### 主キー検索
```c#
var list = new int[] {1 ,2};
var people = _repository.GetList<Person, int> (list);
```
SQL
```sql
Select * from Person WHERE Id IN (1 ,2)
```

#### 任意のカラム検索
```c#
var names = new string[] {"saito" ,"ikeda"};
var people = _repository.GetList<Person, string> (list, nameof(Person.Name));
```
SQL
```sql
Select * from Person WHERE Name IN ("saito" ,"ikeda")
```
