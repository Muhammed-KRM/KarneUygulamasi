using KeremProject1backend.Services;
using Microsoft.AspNetCore.Http;
// using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Servisleri ekle
builder.Services.AddAppServices(builder.Configuration);

// HttpClient Fabrikası
builder.Services.AddHttpClient();

// Controller ve Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen(c =>
// {
//     // Döngüsel referansları önlemek için
//     c.UseAllOfToExtendReferenceSchemas();
//     c.UseOneOfForPolymorphism();
//     c.UseAllOfForInheritance();
// 
//     // Hata yönetimi - benzersiz schema ID'leri
//     c.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
// 
//     // IgnoreObsoleteActions - kullanılmayan action'ları yok say
//     c.IgnoreObsoleteActions();
// 
//     // IgnoreObsoleteProperties - kullanılmayan property'leri yok say
//     c.IgnoreObsoleteProperties();
// });

var app = builder.Build();

// HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

// CORS politikasını kullan
app.UseCors("AllowAngularApp");

// Middleware Kaydı
app.UseMiddleware<KeremProject1backend.Middlewares.RequestLoggingMiddleware>();
app.UseMiddleware<KeremProject1backend.Middlewares.GlobalExceptionMiddleware>();

app.MapControllers();

app.Run();
