namespace LetterClashServer.Domain.Models {
  public static class Idiomas {
    public const string ESPANOL = "ESPAÑOL";
    public const string INGLES = "INGLÉS";

    public static bool EsValido(string idioma) {
      return idioma == ESPANOL || idioma == INGLES;
    }
  }
}
