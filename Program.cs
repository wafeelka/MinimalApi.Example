var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var hotels = new List<Hotel>();
app.MapGet("/hotels", () => hotels);
app.MapGet("/hotels/{id}", (int id) => hotels.Find(x => x.Id == id));
app.MapPost("/hotels", (Hotel hotel) => hotels.Add(hotel));
app.MapPut("/hotels", (Hotel hotel) => {
    var findedHotel = hotels.Find(x => x.Id == hotel.Id);
    if(findedHotel is null)
        throw new Exception("not found");
    findedHotel = hotel;
    return StatusCodes.Status204NoContent;
    });
app.MapDelete("/hotels/{id}", (int id) => 
{
    var hotel = hotels.Find(x => x.Id == id);
    if(hotel is null)
        throw new Exception("not found");
    hotels.Remove(hotel);
});
app.Run();

class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Lattitude { get; set; }
    public double Longittude { get; set; }
}