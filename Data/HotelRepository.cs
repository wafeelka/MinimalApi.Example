
public class HotelRepository : IHotelRepository
{
    private readonly HotelContext _hotelContext;

    public HotelRepository(HotelContext hotelContext)
    {
        _hotelContext = hotelContext;
    }

    public async Task AddHotelAsync(Hotel hotel, CancellationToken cancellationToken = default) =>
        await _hotelContext.AddAsync(hotel, cancellationToken);
    

    public async Task DeleteHotelAsync(int id, CancellationToken cancellationToken = default) =>
        await _hotelContext.Hotels.Where(h => h.Id == id).ExecuteDeleteAsync(cancellationToken);

    public async Task<Hotel?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _hotelContext.Hotels.FindAsync(id, cancellationToken);
    

    public async Task<IReadOnlyList<Hotel>> GetHotelsAsync(CancellationToken cancellationToken = default) =>
        await _hotelContext.Hotels.ToListAsync();
    
    public async Task UpdateHotelAsync(Hotel hotel, CancellationToken cancellationToken = default)
    {
        var findedHotel = await _hotelContext.Hotels.FindAsync(hotel.Id, cancellationToken);
        if(findedHotel is null)
            throw new Exception("not found");
        findedHotel.Lattitude = hotel.Lattitude;
        findedHotel.Longittude = hotel.Longittude;
        findedHotel.Name = hotel.Name;
    }
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) => 
        await _hotelContext.SaveChangesAsync(cancellationToken);

    private bool _disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed)
        {
            if(disposing)
            {
                _hotelContext.Dispose();
            }
        }   
        _disposed = true;
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}