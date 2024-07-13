public interface IHotelRepository : IDisposable
{
    public Task AddHotelAsync(Hotel hotel ,CancellationToken cancellationToken = default);
    public Task DeleteHotelAsync(int Id ,CancellationToken cancellationToken = default);
    public Task<Hotel> GetByIdAsync (int Id ,CancellationToken cancellationToken = default);
    public Task<IReadOnlyList<Hotel>> GetHotelsAsync (CancellationToken cancellationToken = default);
    public Task UpdateHotelAsync (Hotel hotel, CancellationToken cancellationToken = default);
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}