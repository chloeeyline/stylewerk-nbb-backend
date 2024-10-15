namespace StyleWerk.NBB.Models;

/// <summary>
/// Used to show all available languages or to add a new one
/// </summary>
/// <param name="Code">language code as string. E.g "en"</param>
/// <param name="Name">name of the language</param>
/// <param name="Data">translations as json</param>
public record Model_Language(string Code, string Name, string? Data);

/// <summary>
/// Used to show all available color themes or add a new one
/// </summary>
/// <param name="ID">unique identifier</param>
/// <param name="Name">name of the theme</param>
/// <param name="Base">dark or light or system</param>
/// <param name="Data">colors as json</param>
public record Model_ColorTheme(Guid ID, string Name, string Base, string? Data);
