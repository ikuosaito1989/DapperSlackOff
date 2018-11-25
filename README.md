# DapperSlackOff
DapperのCRUDを簡単に使える拡張ライブラリ

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

## Update

```c#
int Update<T> (object entity);
```

#### 主キー更新
```c#
var updateCount = repository.Update<Person> (new { Id = 1, Name = "saito" });
```
SQL
```sql
UPDATE Person SET Name='saito' WHERE Id=1
```

## Delete
```c#
int Delete<T> (object entity, bool conditions = true);
```
#### 主キー削除
```c#
var deleteCount = repository.Delete<Person> (new { Id = 1 });
```
SQL
```sql
DELETE FROM Person WHERE Id=1
```

## Insert
```c#
int Insert<T> (object entity);
```
```c#
var insertCount = repository.Insert<Person> (new { Name = "saito", Age = 29 });
```
```sql
INSERT INTO Person (Name,Age,CreateTime,UpdateTime) VALUES ('saito',29,{CurrentTime},'0000-00-00 00:00:00.000')
```

## CreateOrUpdate
```c#
int CreateOrUpdate<T> (T entity);
```

エンティティに主キーが存在する場合、Insert
```c#
var insertCount = repository.CreateOrUpdate<Person> (new Person () { Id = 1, Name = "saito", Age = 29 });
```
```sql
INSERT INTO Person (Name,Age,CreateTime,UpdateTime) VALUES ('saito',29,{CurrentTime},'0000-00-00 00:00:00.000')
```
エンティティに主キーが存在しない場合、Update
```c#
var updateCount2 = repository.CreateOrUpdate<Person> (new Person () { Name = "saito", Age = 29 });
```
SQL
```sql
UPDATE Person SET Name='saito',Age=29,CreateTime='0000-00-00 00:00:00.000',UpdateTime={CurrentTime} WHERE Id=@Id
```