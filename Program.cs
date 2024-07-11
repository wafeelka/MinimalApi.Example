var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});


var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HotelContext>();
    context.Database.EnsureCreated();
}

app.MapGet("/hotels", async (HotelContext context, CancellationToken cancellationToken) => await context.Hotels.ToListAsync(cancellationToken));
app.MapGet("/hotels/{id}", async (HotelContext context, int id, CancellationToken cancellationToken) => await context.Hotels.FindAsync(id, cancellationToken));
app.MapPost("/hotels", async (HotelContext context, Hotel hotel, CancellationToken cancellationToken) => {
    context.Hotels.Add(hotel);
    await context.SaveChangesAsync(cancellationToken);
});
app.MapPut("/hotels", async (HotelContext context, Hotel hotel, CancellationToken cancellationToken) => {
    var findedHotel = await context.Hotels.FindAsync(hotel.Id, cancellationToken);
    if(findedHotel is null)
        throw new Exception("not found");
    findedHotel = hotel;
    await context.SaveChangesAsync(cancellationToken);
    return StatusCodes.Status204NoContent;
    });
app.MapDelete("/hotels/{id}", async (HotelContext context, int id, CancellationToken cancellationToken) => 
    await context.Hotels.Where(h => h.Id == id).ExecuteDeleteAsync(cancellationToken));
app.Run();

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Lattitude { get; set; }
    public double Longittude { get; set; }
}
public class HotelContext : DbContext
{
    public HotelContext(DbContextOptions<HotelContext> options) : base(options){}
    public DbSet<Hotel> Hotels => Set<Hotel>();
}