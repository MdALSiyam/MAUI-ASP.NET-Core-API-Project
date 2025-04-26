using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.DTOs;
using Server.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(op => op.UseSqlServer(builder.Configuration.GetConnectionString("con")));

var app = builder.Build();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//  Get All
app.MapGet("/api/students", async ([FromServices] AppDbContext db) =>
{
    return await db.Students.Include(sc => sc.Addresses).ToListAsync();
}).WithName("GetStudents").Produces<Student[]>(StatusCodes.Status200OK);

//  Post
app.MapPost("/api/students", async ([FromBody] StudentDto studentDto, [FromServices] AppDbContext db) =>
{
    try
    {
        string imageUrl = null;
        if (!string.IsNullOrEmpty(studentDto.ImageBase64))
        {
            imageUrl = studentDto.ImageBase64;
        }
        List<AddressDto> addresses = new List<AddressDto>();
        if (!string.IsNullOrWhiteSpace(studentDto.AddressJson))
        {
            addresses = JsonSerializer.Deserialize<List<AddressDto>>(studentDto.AddressJson);
        }
        var addStudent = new Student
        {
            Name = studentDto.Name,
            AdmissionDate = studentDto.AdmissionDate,
            IsActive = studentDto.IsActive,
            ImageUrl = imageUrl
        };
        db.Students.Add(addStudent);
        await db.SaveChangesAsync();
        var newStudent = await db.Students.FirstOrDefaultAsync(x => x.Name == studentDto.Name);
        if (newStudent != null && addresses != null)
        {
            foreach (var a in addresses)
            {
                var address = new Address
                {
                    Street = a.Street,
                    City = a.City,
                    StudentId = newStudent.Id
                };
                db.Addresses.Add(address);
            }
            await db.SaveChangesAsync();
        }
        return Results.Created($"/api/students/{newStudent.Id}", newStudent);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"{ex.Message}");
    }
}).WithOpenApi().Produces<Student>(StatusCodes.Status201Created);


//  Get By ID
app.MapGet("/api/students/{id}", async ([FromRoute] int id, [FromServices] AppDbContext db) =>
{
    var student = await db.Students.Include(sc => sc.Addresses).FirstOrDefaultAsync(x => x.Id == id);
    return student;
}).WithName("GetStudent").Produces<Student>(StatusCodes.Status200OK);


//  Put
app.MapPut("/api/students/{id}", async ([FromRoute] int id, [FromBody] StudentDto studentDto, [FromServices] AppDbContext db) =>
{
    try
    {
        var existingstudent = await db.Students.Include(s => s.Addresses).FirstOrDefaultAsync(s => s.Id == id);
        if (existingstudent == null) { return Results.NotFound(); }
        existingstudent.Id = id;
        existingstudent.Name = studentDto.Name;
        existingstudent.AdmissionDate = studentDto.AdmissionDate;
        existingstudent.IsActive = studentDto.IsActive;
        if (!string.IsNullOrEmpty(studentDto.ImageBase64))
        {
            existingstudent.ImageUrl = studentDto.ImageBase64;
        }
        List<Address> addresses = new List<Address>();
        if (!string.IsNullOrWhiteSpace(studentDto.AddressJson))
        {
            addresses = JsonSerializer.Deserialize<List<Address>>(studentDto.AddressJson);
        }
        var addressIds = addresses.Select(a => a.Id).ToList();
        foreach (var address in addresses)
        {
            if (address.Id != 0)
            {
                var existingAddress = existingstudent.Addresses.FirstOrDefault(a => a.Id == address.Id);
                if (existingAddress != null)
                {
                    existingAddress.City = address.City;
                    existingAddress.Street = address.Street;
                }
            }
            else
            {
                var newAddress = new Address
                {
                    Street = address.Street,
                    City = address.City,
                    StudentId = existingstudent.Id,
                };
                db.Addresses.Add(newAddress);
            }
        }
        var addresToDelete = existingstudent.Addresses.Where(a => !addressIds.Contains(a.Id)).ToList();
        db.RemoveRange(addresToDelete);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
}).WithOpenApi()
.Produces(StatusCodes.Status404NotFound)
.Produces(StatusCodes.Status204NoContent)
.Produces(StatusCodes.Status500InternalServerError);


//  Delete
app.MapDelete("/api/students/{id}", async ([FromRoute] int id, [FromServices] AppDbContext db) => {
    try
    {
        var student = await db.Students.Include(s => s.Addresses).FirstOrDefaultAsync(s => s.Id == id);
        if (student == null) { return Results.NotFound(); }
        db.Addresses.RemoveRange(student.Addresses);
        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"{ex.Message}");
    }
}).WithOpenApi().Produces(StatusCodes.Status204NoContent);
app.Run();
