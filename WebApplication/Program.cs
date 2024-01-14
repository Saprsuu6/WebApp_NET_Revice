using System.Text.Json;
using Microsoft.AspNetCore.HttpLogging;

var builder = WebApplication.CreateBuilder(args);

#region enable services
builder.Services.AddW3CLogging(logging =>
{
    logging.LoggingFields = W3CLoggingFields.All;

    logging.AdditionalRequestHeaders.Add("x-forwarded-for");
    logging.AdditionalRequestHeaders.Add("x-client-ssl-protocol");
    logging.FileSizeLimit = 5 * 1024 * 1024;
    logging.RetainedFileCountLimit = 2;
    logging.FileName = "MyLogFile";
    logging.LogDirectory = "./Logs";
    logging.FlushInterval = TimeSpan.FromSeconds(2);
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#region middlewares
app.UseW3CLogging();
app.UseStaticFiles();
app.UseHttpsRedirection();
#endregion

app.Run(async (context) =>
{
    var path = context.Request.Path;
    var now = DateTime.Now;
    var response = context.Response;
    var request = context.Request;

    switch (path)
    {
        case "/TestWebApp/date":
            await response.WriteAsync($"Date: {now.ToShortDateString()}");
            break;
        case "/TestWebApp/time":
            await response.WriteAsync($"Date: {now.ToShortTimeString()}");
            break;
        case "/TestWebApp/old_developer":
            context.Response.Redirect("/TestWebApp/developer");
            Console.WriteLine("Redirection...");
            break;
        case "/TestWebApp/developer":
            response.ContentType = "image/jpeg";
            await response.SendFileAsync("./wwwroot/me.jpg");
            break;
        case "/TestWebApp/developer_info":
            response.ContentType = "application/json";

            // if body has json content
            if (request.HasJsonContentType())
            {
                var jsonoption = new JsonSerializerOptions();
                jsonoption.Converters.Add(new PersonConverter());
                var person = await request.ReadFromJsonAsync<Person>(jsonoption);
                if (person != null)
                {
                    await response.WriteAsJsonAsync<Person>(person);
                }
            }
            break;
        default:
            await response.WriteAsync("Hello METANIT.COM");
            break;

    }
});

app.Run();