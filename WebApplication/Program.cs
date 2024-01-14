using System.Text.Json;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Primitives;

var generalPath = "/TestWebApp";
var builder = WebApplication.CreateBuilder(args);

List<Person> people = new List<Person>() {
    new Person("John", 21), new Person("Ann", 22), new Person("Ira", 23), new Person("Andry", 24)
};

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

    if (path == $"{generalPath}/date" && request.Method == "GET")
        await response.WriteAsync($"Date: {now.ToShortDateString()}");
    else if (path == $"{generalPath}/time" && request.Method == "GET")
        await response.WriteAsync($"Date: {now.ToShortTimeString()}");
    else if (path == $"{generalPath}/old_developer" && request.Method == "GET")
        await response.WriteAsync($"Date: {now.ToShortTimeString()}");
    else if (path == $"{generalPath}/old_developer" && request.Method == "GET")
    {
        context.Response.Redirect("/TestWebApp/developer");
        Console.WriteLine("Redirection...");
    }
    else if (path == $"{generalPath}/developer" && request.Method == "GET")
    {
        response.ContentType = "image/jpeg";
        await response.SendFileAsync("./wwwroot/me.jpg");
    }
    else if (path == $"{generalPath}/developer_info" && request.Method == "POST")
    {
        response.ContentType = "application/json";

        // if body has json content
        if (request.HasJsonContentType())
        {
            var jsonoption = new JsonSerializerOptions();
            jsonoption.Converters.Add(new PersonConverter());
            var person = await request.ReadFromJsonAsync<Person>(jsonoption);
            if (person != null)
            {
                await response.WriteAsJsonAsync(person);
            }
        }
    }
    else if (path == $"{generalPath}/get_people" && request.Method == "GET")
        await response.WriteAsJsonAsync(people);
    else if (path == $"{generalPath}/get_current" && request.Method == "GET")
    {
        // Получаем коллекцию параметров из строки запроса
        IQueryCollection queryParameters = context.Request.Query;

        if (queryParameters != null)
        {
            if (queryParameters.TryGetValue("index", out StringValues index))
                await response.WriteAsJsonAsync(people[int.Parse(index!)]);
        }
    }
    else
        await response.WriteAsync("Hello METANIT.COM");
});

app.Run();