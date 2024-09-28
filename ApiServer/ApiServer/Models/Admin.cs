namespace StyleWerk.NBB.Models;

public record Model_LanguageItem(string Code, string Name, string Username);
public record Model_LanguageDetails(string Code, string Name, string Data);
public record Model_ColorThemeItem(Guid ID, string Name, string Base, string Username);
public record Model_ColorThemeDetails(Guid ID, string Name, string Base, string Data);
