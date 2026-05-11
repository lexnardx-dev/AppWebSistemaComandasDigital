namespace AppWebSistemaComandasDigital.Helpers
{
    public class ApiResponse<T>
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errores { get; set; } = [];
    }

    public static class ResponseHelper
    {
        public static ApiResponse<T> Ok<T>(T data, string mensaje = "Operación exitosa") =>
            new() { Exitoso = true, Mensaje = mensaje, Data = data };

        public static ApiResponse<T> Creado<T>(T data, string mensaje = "Recurso creado") =>
            new() { Exitoso = true, Mensaje = mensaje, Data = data };

        public static ApiResponse<T> Error<T>(string mensaje, List<string>? errores = null) =>
            new() { Exitoso = false, Mensaje = mensaje, Errores = errores ?? [] };

        public static ApiResponse<T> NoEncontrado<T>(string recurso) =>
            new() { Exitoso = false, Mensaje = $"{recurso} no encontrado." };

        public static ApiResponse<T> NoAutorizado<T>(string mensaje = "No autorizado.") =>
            new() { Exitoso = false, Mensaje = mensaje };
    }
}
