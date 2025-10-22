namespace BibliotecaMetropolis.Models
{
    public class RecursoDetailsViewModel
    {
        public int IdRec { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string? ImagenRuta { get; set; }
        public string Autores { get; set; } = "Desconocido";
        public string? Editorial { get; set; }
        public int? AnioPublicacion { get; set; }
        public string? Edicion { get; set; }
        public int Cantidad { get; set; }
        public string? TipoRecurso { get; set; }
        public string? Pais { get; set; }
        public decimal? Precio { get; set; }
        public string? PalabrasBusqueda { get; set; }
        public string? Descripcion { get; set; }
    }
}
