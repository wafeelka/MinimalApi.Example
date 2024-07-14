using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HotelContext>(options => {
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options => 
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseAuthorization();
app.UseAuthentication();
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<HotelContext>();
    context.Database.EnsureCreated();
}
app.UseHttpsRedirection();

app.MapGet("/login", [AllowAnonymous] async  (HttpContext context, ITokenService tokenService,
 IUserRepository userRepository) => 
{
    var userModel = new UserModel()
    {
        UserName = context.Request.Query["username"],
        Password = context.Request.Query["password"]
    };
    var userDto = userRepository.GetUser(userModel);
    if(userDto is null) return Results.Unauthorized();
    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"], 
              builder.Configuration["Jwt:Issuer"], userDto);
    return Results.Ok(token);
});

app.MapGet("/hotels", [Authorize] async Task<Results<Ok<IReadOnlyList<Hotel>>, NotFound>> (IHotelRepository repository, CancellationToken cancellationToken) => 
{
    var hotels = await repository.GetHotelsAsync(cancellationToken);
    return hotels.Count > 0 ? TypedResults.Ok(hotels) : TypedResults.NotFound();
})
.WithName("GetAllHotels")
.WithTags("Getters");

app.MapGet("/hotels/{id}", [Authorize] async Task<Results<Ok<Hotel>, NotFound>> (IHotelRepository repository, int id, CancellationToken cancellationToken) => 
{
    return await repository.GetByIdAsync(id, cancellationToken) is {}  hotel ?
    TypedResults.Ok(hotel) : TypedResults.NotFound();
})
.WithName("GetHotelById")
.WithTags("Getters");

app.MapGet("/hotels/{name}",[Authorize]  async Task<Results<Ok<Hotel>, NotFound>> (IHotelRepository repository, string name, CancellationToken cancellationToken) => 
{
    return await repository.GetByNameAsync(name, cancellationToken) is {}  hotel ?
    TypedResults.Ok(hotel) : TypedResults.NotFound();
})
.WithName("GetHotelByName")
.WithTags("Getters");;

app.MapPost("/hotels", [Authorize] async Task<Results<Created, BadRequest>> (IHotelRepository repository, Hotel hotel, CancellationToken cancellationToken) => 
{
    await repository.AddHotelAsync(hotel, cancellationToken);
    await repository.SaveChangesAsync(cancellationToken);
    return TypedResults.Created();
})
.Accepts<Hotel>("application/json")
//.Produces(StatusCodes.Status201Created)
.WithName("CreateHotel")
.WithTags("Posts");

app.MapPut("/hotels", [Authorize]  async Task<Results<NoContent, NotFound>> (IHotelRepository repository, Hotel hotel, CancellationToken cancellationToken) => 
{
    var findedHotel = await repository.GetByIdAsync(hotel.Id, cancellationToken);
    if(findedHotel is null)
        return TypedResults.NotFound();
    findedHotel.Lattitude = hotel.Lattitude;
    findedHotel.Longittude = hotel.Longittude;
    findedHotel.Name = hotel.Name;
    await repository.SaveChangesAsync(cancellationToken);
    return TypedResults.NoContent();
})
.Accepts<Hotel>("application/json")
.Produces(StatusCodes.Status204NoContent)
.WithName("UpdateHotel")
.WithTags("Puts");

app.MapDelete("/hotels/{id}", async Task<Results<Ok, BadRequest>> (IHotelRepository repository, int id, CancellationToken cancellationToken) => 
{
      await repository.DeleteHotelAsync(id, cancellationToken);
      return TypedResults.Ok();
})
.Produces(StatusCodes.Status200OK)
.WithName("DeleteHotel")
.WithTags("Deletes");
  
app.Run();
