using System.Text.Json;
using System.Text.Json.Serialization;

public record Person(string Name, int Age);

public class PersonConverter : JsonConverter<Person>
{
    public override Person? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var personName = "Undefined";
        var personAge = 0;
        while (reader.Read())
        {
            Console.WriteLine(reader.TokenType);
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                Console.WriteLine(propertyName);
                reader.Read();
                Console.WriteLine(reader.TokenType);

                switch (propertyName?.ToLower())
                {
                    // если свойство age и оно содержит число
                    case "age" when reader.TokenType == JsonTokenType.Number:
                        personAge = reader.GetInt32();
                        break;
                    // если свойство age и оно содержит строку
                    case "age" when reader.TokenType == JsonTokenType.String:
                        string? stringValue = reader.GetString();
                        if (int.TryParse(stringValue, out int value))
                        {
                            personAge = value;
                        }
                        break;
                    // если свойство Name/name
                    case "name":
                        string? name = reader.GetString();
                        if (name != null)
                            personName = name;
                        break;
                }
            }
        }
        return new Person(personName, personAge);
    }

    public override void Write(Utf8JsonWriter writer, Person value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);
        writer.WriteNumber("age", value.Age);
        writer.WriteEndObject();
    }
}