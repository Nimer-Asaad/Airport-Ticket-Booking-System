namespace AirportTicketBooking.Application.Contracts;

public interface IRepository<T, TKey>
{
    List<T> GetAll();
    T? GetById(TKey id);
    void Upsert(T entity, Func<T, TKey> keySelector);
    bool Delete(TKey id);
}
