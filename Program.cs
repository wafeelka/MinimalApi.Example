using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

app.MapGet("/hotels", async Task<Results<Ok<List<Hotel>>, NotFound>> (HotelContext context, CancellationToken cancellationToken) => 
{
    var hotels = await context.Hotels.ToListAsync(cancellationToken);
    return hotels.Count > 0 ? TypedResults.Ok(hotels) : TypedResults.NotFound();
});

app.MapGet("/hotels/{id}", async Task<Results<Ok<Hotel>, NotFound>> (HotelContext context, int id, CancellationToken cancellationToken) => 
{
    return await context.Hotels.FindAsync(id, cancellationToken) is {}  hotel ?
    TypedResults.Ok(hotel) : TypedResults.NotFound();
});

app.MapPost("/hotels", async Task<Results<Created, BadRequest>> (HotelContext context, Hotel hotel, CancellationToken cancellationToken) => 
{
    context.Hotels.Add(hotel);
    await context.SaveChangesAsync(cancellationToken);
    return TypedResults.Created();
});

app.MapPut("/hotels", async Task<Results<NoContent, NotFound>> (HotelContext context, Hotel hotel, CancellationToken cancellationToken) => 
{
    var findedHotel = await context.Hotels.FindAsync(hotel.Id, cancellationToken);
    if(findedHotel is null)
        return TypedResults.NotFound();
    findedHotel.Lattitude = hotel.Lattitude;
    findedHotel.Longittude = hotel.Longittude;
    findedHotel.Name = hotel.Name;
    await context.SaveChangesAsync(cancellationToken);
    return TypedResults.NoContent();
});

app.MapDelete("/hotels/{id}", async Task<Results<Ok, BadRequest>> (HotelContext context, int id, CancellationToken cancellationToken) => 
{
      await context.Hotels.Where(h => h.Id == id).ExecuteDeleteAsync(cancellationToken);
      return TypedResults.Ok();
});
  
app.Run();
