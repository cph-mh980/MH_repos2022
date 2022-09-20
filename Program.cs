using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MotorDb>(opt => opt.UseInMemoryDatabase("MotorList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Parking spot registration service");

/* Get metode - Alle records læses fra database */

app.MapGet("/Motoritems", async (MotorDb db) =>
    await db.Motors.ToListAsync());

/* Get metode - Alle records læses fra database */

app.MapGet("/Motoritems/complete", async (MotorDb db) =>
    await db.Motors.Where(t => t.IsComplete).ToListAsync());
/* -----------------------------------------------------------*/
/* GET metode - Record Id søges i database - Ok elle NotFound */
/* -----------------------------------------------------------*/

app.MapGet("/motoritems/{id}", async (int id, MotorDb db) =>
    await db.Motors.FindAsync(id)
        is Motor motor
            ? Results.Ok(motor)
            : Results.NotFound());
/* -------------------------------------------------*/
/* POST metode - motor record indskrives i database */
/* her foretages parkeringsplads + bil registrering.*/
/* -------------------------------------------------*/

app.MapPost("/motoritems", async (Motor motor, MotorDb db) =>
{
    motor.ParkTime = DateTime.Now.ToString();
    motor.IsBusy = true;
    db.Motors.Add(motor);
    await db.SaveChangesAsync();

    return Results.Created($"/motoritems/{motor.Id}", motor);
});

/* PUT metode - Ny record oprettes i database */

app.MapPut("/motoritems/{id}", async (int id, Motor inputMotor, MotorDb db) =>
{
    var motor = await db.Motors.FindAsync(id);

    if (motor is null) return Results.NotFound();

    motor.VehicleNumber = inputMotor.VehicleNumber;
    motor.IsComplete = inputMotor.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

/* DELETE metode - Record id slættes fra database */

app.MapDelete("/motoritems/{id}", async (int id, MotorDb db) =>
{
    if (await db.Motors.FindAsync(id) is Motor motor)
    {
        db.Motors.Remove(motor);
        await db.SaveChangesAsync();
        return Results.Ok(motor);
    }

    return Results.NotFound();
});
app.Run();

/* Model data record */

class Motor
{
    public int Id { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ParkTime { get; set; }
    public string? ParkingSpotNumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? Email { get; set; }
    public bool IsBusy { get; set; }
    public bool IsComplete { get; set; }
}

/* Database håndtering */
class MotorDb : DbContext
{
    public MotorDb(DbContextOptions<MotorDb> options)
        : base(options) { }

    public DbSet<Motor> Motors => Set<Motor>();
}
