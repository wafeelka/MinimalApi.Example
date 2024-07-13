using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HotelContext>();
    context.Database.EnsureCreated();
}

app.MapGet("/hotels", async Task<Results<Ok<IReadOnlyList<Hotel>>, NotFound>> (IHotelRepository repository, CancellationToken cancellationToken) => 
{
    var hotels = await repository.GetHotelsAsync(cancellationToken);
    return hotels.Count > 0 ? TypedResults.Ok(hotels) : TypedResults.NotFound();
});

app.MapGet("/hotels/{id}", async Task<Results<Ok<Hotel>, NotFound>> (IHotelRepository repository, int id, CancellationToken cancellationToken) => 
{
    return await repository.GetByIdAsync(id, cancellationToken) is {}  hotel ?
    TypedResults.Ok(hotel) : TypedResults.NotFound();
});

app.MapPost("/hotels", async Task<Results<Created, BadRequest>> (IHotelRepository repository, Hotel hotel, CancellationToken cancellationToken) => 
{
    await repository.AddHotelAsync(hotel, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);
    return TypedResults.Created();
});

app.MapPut("/hotels", async Task<Results<NoContent, NotFound>> (IHotelRepository repository, Hotel hotel, CancellationToken cancellationToken) => 
{
    var findedHotel = await repository.GetByIdAsync(hotel.Id, cancellationToken);
    if(findedHotel is null)
        return TypedResults.NotFound();
    findedHotel.Lattitude = hotel.Lattitude;
    findedHotel.Longittude = hotel.Longittude;
    findedHotel.Name = hotel.Name;
    await repository.SaveChangesAsync(cancellationToken);
    return TypedResults.NoContent();
});

app.MapDelete("/hotels/{id}", async Task<Results<Ok, BadRequest>> (IHotelRepository repository, int id, CancellationToken cancellationToken) => 
{
      await repository.DeleteHotelAsync(id, cancellationToken);
      return TypedResults.Ok();
});
  
app.Run();
