using AirportTicketBooking.Application.Contracts;
using AirportTicketBooking.Shared.Helpers;

namespace AirportTicketBooking.Infrastructure.Repositories;

public class JsonRepository<T, TKey> : IRepository<T, TKey>
{
    private readonly string _path;
    private readonly object _sync = new();

    public JsonRepository(string path)
    {
        _path = path;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (!File.Exists(_path)) JsonFile.WriteList<T>(_path, new List<T>());
    }

    public List<T> GetAll()
    {
        lock (_sync) { return JsonFile.ReadList<T>(_path); }
    }

    public T? GetById(TKey id)
    {
        lock (_sync)
        {
            var items = JsonFile.ReadList<T>(_path);
            var prop = GetIdAccessor();
            return items.FirstOrDefault(x => Equals(prop(x), id));
        }
    }

    public void Upsert(T entity, Func<T, TKey> keySelector)
    {
        lock (_sync)
        {
            var items = JsonFile.ReadList<T>(_path);
            var key = keySelector(entity);
            var prop = GetIdAccessor();

            var idx = items.FindIndex(x => Equals(prop(x), key));
            if (idx >= 0) items[idx] = entity;
            else items.Add(entity);

            JsonFile.WriteList(_path, items);
        }
    }

    public bool Delete(TKey id)
    {
        lock (_sync)
        {
            var items = JsonFile.ReadList<T>(_path);
            var prop = GetIdAccessor();

            var removed = items.RemoveAll(x => Equals(prop(x), id));
            if (removed > 0)
            {
                JsonFile.WriteList(_path, items);
                return true;
            }
            return false;
        }
    }

    private Func<T, object?> GetIdAccessor()
    {
        var t = typeof(T);
        var idProp = t.GetProperty("Id") ?? t.GetProperty("Code");
        if (idProp == null)
            throw new InvalidOperationException($"{t.Name} must have Id or Code property to be used as a key.");
        return (T x) => idProp.GetValue(x);
    }
}
